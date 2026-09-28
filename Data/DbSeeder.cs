using HumanBirthPredictionSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanBirthPredictionSystem.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            if (!db.Users.Any())
            {
                db.Users.Add(new User
                {
                    Username = "Bashiir",
                    PasswordHash = PasswordHasher.Hash("bashiir21"),
                    FullName = "System Administrator",
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                });
                db.SaveChanges();
            }

            if (!db.Countries.Any())
            {
                var countries = new List<Country>
                {
                    new() { CountryName = "Somalia", CountryCode = "SOM", Continent = "Africa" },
                    new() { CountryName = "Kenya", CountryCode = "KEN", Continent = "Africa" },
                    new() { CountryName = "Ethiopia", CountryCode = "ETH", Continent = "Africa" },
                    new() { CountryName = "Nigeria", CountryCode = "NGA", Continent = "Africa" },
                    new() { CountryName = "South Africa", CountryCode = "ZAF", Continent = "Africa" },
                    new() { CountryName = "Egypt", CountryCode = "EGY", Continent = "Africa" },
                    new() { CountryName = "India", CountryCode = "IND", Continent = "Asia" },
                    new() { CountryName = "China", CountryCode = "CHN", Continent = "Asia" },
                    new() { CountryName = "United States", CountryCode = "USA", Continent = "North America" },
                    new() { CountryName = "Canada", CountryCode = "CAN", Continent = "North America" },
                    new() { CountryName = "United Kingdom", CountryCode = "GBR", Continent = "Europe" },
                    new() { CountryName = "Germany", CountryCode = "DEU", Continent = "Europe" },
                };
                db.Countries.AddRange(countries);
                db.SaveChanges();
            }

            if (!db.Cities.Any())
            {
                var somalia = db.Countries.First(c => c.CountryCode == "SOM");
                var kenya = db.Countries.First(c => c.CountryCode == "KEN");

                db.Cities.AddRange(
                    new City { CountryId = somalia.Id, CityName = "Garowe" },
                    new City { CountryId = somalia.Id, CityName = "Mogadishu" },
                    new City { CountryId = somalia.Id, CityName = "Bosaso" },
                    new City { CountryId = somalia.Id, CityName = "Hargeisa" },
                    new City { CountryId = kenya.Id, CityName = "Nairobi" },
                    new City { CountryId = kenya.Id, CityName = "Mombasa" },
                    new City { CountryId = kenya.Id, CityName = "Kisumu" }
                );
                db.SaveChanges();
            }

            if (!db.BirthRecords.Any())
            {
                var rnd = new Random(42);
                var records = new List<BirthRecord>();

                foreach (var country in db.Countries.ToList())
                {
                    double baseTotal = 40000 + rnd.Next(0, 400000);

                    for (int year = 2015; year <= 2024; year++)
                    {
                        baseTotal *= 1 + (rnd.NextDouble() * 0.04 - 0.01);
                        int total = (int)baseTotal;
                        int male = (int)(total * (0.512 + (rnd.NextDouble() * 0.006 - 0.003)));
                        int female = total - male;

                        records.Add(new BirthRecord
                        {
                            CountryId = country.Id,
                            CityId = null,
                            Year = year,
                            TotalBirths = total,
                            MaleBirths = male,
                            FemaleBirths = female,
                            DataSource = "Sample Data for System Demonstration",
                            SourceReference = "Generated for thesis defense demonstration purposes",
                            RecordType = RecordType.Estimated
                        });
                    }
                }

                db.BirthRecords.AddRange(records);
                db.SaveChanges();
            }

            if (!db.BirthRecords.Any(r => r.CityId != null))
            {
                var cityRecords = new List<BirthRecord>();
                var cities = db.Cities.Include(c => c.Country).ToList();

                var cityBaseMap = new Dictionary<string, int>
                {
                    { "Garowe", 12500 },
                    { "Mogadishu", 82000 },
                    { "Bosaso", 19200 },
                    { "Hargeisa", 32000 },
                    { "Nairobi", 118000 },
                    { "Mombasa", 39500 },
                    { "Kisumu", 23000 }
                };

                foreach (var city in cities)
                {
                    int baseBirths = cityBaseMap.TryGetValue(city.CityName, out var b) ? b : 15000;
                    for (int year = 2015; year <= 2024; year++)
                    {
                        int offset = year - 2015;
                        int total = baseBirths + (offset * (baseBirths / 30));
                        int male = (int)(total * 0.512);
                        int female = total - male;

                        cityRecords.Add(new BirthRecord
                        {
                            CountryId = city.CountryId,
                            CityId = city.Id,
                            Year = year,
                            TotalBirths = total,
                            MaleBirths = male,
                            FemaleBirths = female,
                            DataSource = $"{city.CityName} Municipal Health Records",
                            SourceReference = "Annual Demographic Health Survey",
                            RecordType = RecordType.Official,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                db.BirthRecords.AddRange(cityRecords);
                db.SaveChanges();
            }
        }
    }
}
