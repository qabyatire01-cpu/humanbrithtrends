using System.Diagnostics;
using System.Text.Json;
using HumanBirthPredictionSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace HumanBirthPredictionSystem.Services
{
    public class PythonPredictionService : IPythonPredictionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<PythonPredictionService> _logger;
        private readonly IWebHostEnvironment _env;

        public PythonPredictionService(
            ApplicationDbContext db,
            IConfiguration config,
            ILogger<PythonPredictionService> logger,
            IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _logger = logger;
            _env = env;
        }

        public async Task<PredictionResult> RunPredictionAsync(PredictionRequest request)
        {
            var query = _db.BirthRecords.AsNoTracking()
                .Where(r => r.CountryId == request.CountryId && r.Year < request.StartYear);

            if (request.CityId.HasValue)
                query = query.Where(r => r.CityId == request.CityId);

            var records = await query.OrderBy(r => r.Year).ToListAsync();

            if (records.Count < 2)
            {
                var fallbackQuery = _db.BirthRecords.AsNoTracking().Where(r => r.CountryId == request.CountryId);
                if (request.CityId.HasValue) fallbackQuery = fallbackQuery.Where(r => r.CityId == request.CityId);
                records = await fallbackQuery.OrderBy(r => r.Year).ToListAsync();
            }

            if (records.Count < 2)
            {
                return new PredictionResult
                {
                    Success = false,
                    ErrorMessage = "At least two years of historical data are required to run a prediction for this selection."
                };
            }

            var payload = new
            {
                start_year = request.StartYear,
                end_year = request.EndYear,
                history = records.Select(r => new
                {
                    year = r.Year,
                    total_births = r.TotalBirths,
                    male_births = r.MaleBirths,
                    female_births = r.FemaleBirths
                })
            };

            var inputJson = JsonSerializer.Serialize(payload);

            try
            {
                var pythonExe = _config["PythonSettings:PythonExecutablePath"] ?? "python";
                var scriptsFolder = _config["PythonSettings:ScriptsFolder"] ?? "python";
                var scriptName = _config["PythonSettings:PredictionScript"] ?? "prediction.py";
                var timeoutSeconds = int.TryParse(_config["PythonSettings:TimeoutSeconds"], out var t) ? t : 60;

                var scriptPath = Path.Combine(_env.ContentRootPath, scriptsFolder, scriptName);

                var psi = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = $"\"{scriptPath}\"",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi)
                    ?? throw new InvalidOperationException("Failed to start the Python prediction process.");

                await process.StandardInput.WriteAsync(inputJson);
                process.StandardInput.Close();

                var stdoutTask = process.StandardOutput.ReadToEndAsync();
                var stderrTask = process.StandardError.ReadToEndAsync();

                var finished = process.WaitForExit(timeoutSeconds * 1000);
                if (!finished)
                {
                    process.Kill(true);
                    return new PredictionResult { Success = false, ErrorMessage = "The prediction process timed out." };
                }

                var stdout = await stdoutTask;
                var stderr = await stderrTask;

                if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(stdout))
                {
                    _logger.LogWarning("Python prediction script failed (Exit code: {Code}). Falling back to internal Linear Regression engine.", process.ExitCode);
                    return FallbackLinearRegression(records, request.StartYear, request.EndYear);
                }

                var parsed = JsonSerializer.Deserialize<PythonScriptOutput>(stdout, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (parsed is null || !parsed.success)
                {
                    _logger.LogWarning("Python prediction script returned error ({Error}). Falling back to internal Linear Regression engine.", parsed?.error);
                    return FallbackLinearRegression(records, request.StartYear, request.EndYear);
                }

                return new PredictionResult
                {
                    Success = true,
                    Model = parsed.model ?? "Linear Regression",
                    Predictions = parsed.predictions.Select(p => new PredictedYear
                    {
                        Year = p.year,
                        TotalBirths = p.total_births,
                        MaleBirths = p.male_births,
                        FemaleBirths = p.female_births
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Python environment not available. Using built-in Linear Regression model.");
                return FallbackLinearRegression(records, request.StartYear, request.EndYear);
            }
        }

        private static PredictionResult FallbackLinearRegression(List<Models.BirthRecord> records, int startYear, int endYear)
        {
            double n = records.Count;
            double sumX = records.Sum(r => (double)r.Year);
            double sumY = records.Sum(r => (double)r.TotalBirths);
            double sumXY = records.Sum(r => (double)r.Year * r.TotalBirths);
            double sumX2 = records.Sum(r => (double)r.Year * r.Year);

            double denominator = n * sumX2 - sumX * sumX;
            double slope = Math.Abs(denominator) > 1e-9 ? (n * sumXY - sumX * sumY) / denominator : 0;
            double intercept = (sumY - slope * sumX) / n;

            long totalHistoricalBirths = records.Sum(r => (long)r.TotalBirths);
            long totalHistoricalMales = records.Sum(r => (long)r.MaleBirths);
            double maleRatio = totalHistoricalBirths > 0 ? (double)totalHistoricalMales / totalHistoricalBirths : 0.51;

            var predictions = new List<PredictedYear>();
            for (int year = startYear; year <= endYear; year++)
            {
                double predicted = slope * year + intercept;
                int total = (int)Math.Round(Math.Max(0, predicted));
                int male = (int)Math.Round(total * maleRatio);
                int female = total - male;

                predictions.Add(new PredictedYear
                {
                    Year = year,
                    TotalBirths = total,
                    MaleBirths = male,
                    FemaleBirths = female
                });
            }

            return new PredictionResult
            {
                Success = true,
                Model = "Linear Regression",
                Predictions = predictions
            };
        }

        private class PythonScriptOutput
        {
            public bool success { get; set; }
            public string? error { get; set; }
            public string? model { get; set; }
            public List<PythonPredictedYear> predictions { get; set; } = new();
        }

        private class PythonPredictedYear
        {
            public int year { get; set; }
            public int total_births { get; set; }
            public int male_births { get; set; }
            public int female_births { get; set; }
        }
    }
}
