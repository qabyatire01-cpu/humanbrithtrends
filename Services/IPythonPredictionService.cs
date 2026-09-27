namespace HumanBirthPredictionSystem.Services
{
    public class PredictionRequest
    {
        public int CountryId { get; set; }
        public int? CityId { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
    }

    public class PredictedYear
    {
        public int Year { get; set; }
        public int TotalBirths { get; set; }
        public int MaleBirths { get; set; }
        public int FemaleBirths { get; set; }
    }

    public class PredictionResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string Model { get; set; } = "Linear Regression";
        public List<PredictedYear> Predictions { get; set; } = new();
    }

    public interface IPythonPredictionService
    {
        Task<PredictionResult> RunPredictionAsync(PredictionRequest request);
    }
}
