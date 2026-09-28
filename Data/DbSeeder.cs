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

            // Ensure Admin and standard user Ahmed exist
            if (!db.Users.Any(u => u.Username == "Ahmed"))
            {
                db.Users.Add(new User
                {
                    Username = "Ahmed",
                    PasswordHash = PasswordHasher.Hash("user123"),
                    FullName = "Ahmed User",
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                });
                db.SaveChanges();
            }

            // Replace all birth records with the requested 2015-2035 Somalia dataset
            var somaliaCountry = db.Countries.FirstOrDefault(c => c.CountryCode == "SOM");
            if (somaliaCountry == null)
            {
                somaliaCountry = new Country { CountryName = "Somalia", CountryCode = "SOM", Continent = "Africa" };
                db.Countries.Add(somaliaCountry);
                db.SaveChanges();
            }

            // Check if existing records match the 2015-2035 Somalia data
            bool needsSeed = db.BirthRecords.Count() != 21 ||
                             !db.BirthRecords.All(r => r.CountryId == somaliaCountry.Id && r.Year >= 2015 && r.Year <= 2035);

            if (needsSeed)
            {
                // Clear any existing birth records
                var existing = db.BirthRecords.ToList();
                db.BirthRecords.RemoveRange(existing);
                db.SaveChanges();

                var records = new List<BirthRecord>
                {
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2015, TotalBirths = 651292, MaleBirths = 332031, FemaleBirths = 319261, RecordType = RecordType.Estimated, DataSource = "Somalia National Demographic Estimates", SourceReference = "Demographic Survey 2015-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2016, TotalBirths = 668096, MaleBirths = 340598, FemaleBirths = 327498, RecordType = RecordType.Estimated, DataSource = "Somalia National Demographic Estimates", SourceReference = "Demographic Survey 2015-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2017, TotalBirths = 688481, MaleBirths = 350990, FemaleBirths = 337491, RecordType = RecordType.Estimated, DataSource = "Somalia National Demographic Estimates", SourceReference = "Demographic Survey 2015-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2018, TotalBirths = 705717, MaleBirths = 359777, FemaleBirths = 345940, RecordType = RecordType.Estimated, DataSource = "Somalia National Demographic Estimates", SourceReference = "Demographic Survey 2015-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2019, TotalBirths = 723617, MaleBirths = 368903, FemaleBirths = 354714, RecordType = RecordType.Estimated, DataSource = "Somalia National Demographic Estimates", SourceReference = "Demographic Survey 2015-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2020, TotalBirths = 741705, MaleBirths = 378124, FemaleBirths = 363581, RecordType = RecordType.Estimated, DataSource = "Somalia National Demographic Estimates", SourceReference = "Demographic Survey 2015-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2021, TotalBirths = 761567, MaleBirths = 388250, FemaleBirths = 373317, RecordType = RecordType.Estimated, DataSource = "Somalia National Demographic Estimates", SourceReference = "Demographic Survey 2015-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2022, TotalBirths = 779534, MaleBirths = 397409, FemaleBirths = 382125, RecordType = RecordType.Estimated, DataSource = "Somalia National Demographic Estimates", SourceReference = "Demographic Survey 2015-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2023, TotalBirths = 788763, MaleBirths = 402114, FemaleBirths = 386649, RecordType = RecordType.Estimated, DataSource = "Somalia National Demographic Estimates", SourceReference = "Demographic Survey 2015-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2024, TotalBirths = 804966, MaleBirths = 410375, FemaleBirths = 394591, RecordType = RecordType.Estimated, DataSource = "Somalia National Demographic Estimates", SourceReference = "Demographic Survey 2015-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2025, TotalBirths = 822215, MaleBirths = 419168, FemaleBirths = 403047, RecordType = RecordType.Predicted, DataSource = "Somalia Demographic Projection Model", SourceReference = "Demographic Projections 2025-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2026, TotalBirths = 836420, MaleBirths = 426410, FemaleBirths = 410010, RecordType = RecordType.Predicted, DataSource = "Somalia Demographic Projection Model", SourceReference = "Demographic Projections 2025-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, CityId = null, Year = 2027, TotalBirths = 847521, MaleBirths = 432070, FemaleBirths = 415451, RecordType = RecordType.Predicted, DataSource = "Somalia Demographic Projection Model", SourceReference = "Demographic Projections 2025-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, Year = 2028, TotalBirths = 859891, MaleBirths = 438376, FemaleBirths = 421515, RecordType = RecordType.Predicted, DataSource = "Somalia Demographic Projection Model", SourceReference = "Demographic Projections 2025-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, Year = 2029, TotalBirths = 869665, MaleBirths = 443359, FemaleBirths = 426306, RecordType = RecordType.Predicted, DataSource = "Somalia Demographic Projection Model", SourceReference = "Demographic Projections 2025-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, Year = 2030, TotalBirths = 878445, MaleBirths = 447835, FemaleBirths = 430610, RecordType = RecordType.Predicted, DataSource = "Somalia Demographic Projection Model", SourceReference = "Demographic Projections 2025-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, Year = 2031, TotalBirths = 891665, MaleBirths = 454574, FemaleBirths = 437091, RecordType = RecordType.Predicted, DataSource = "Somalia Demographic Projection Model", SourceReference = "Demographic Projections 2025-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, Year = 2032, TotalBirths = 901357, MaleBirths = 459515, FemaleBirths = 441842, RecordType = RecordType.Predicted, DataSource = "Somalia Demographic Projection Model", SourceReference = "Demographic Projections 2025-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, Year = 2033, TotalBirths = 915030, MaleBirths = 466486, FemaleBirths = 448544, RecordType = RecordType.Predicted, DataSource = "Somalia Demographic Projection Model", SourceReference = "Demographic Projections 2025-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, Year = 2034, TotalBirths = 924402, MaleBirths = 471264, FemaleBirths = 453138, RecordType = RecordType.Predicted, DataSource = "Somalia Demographic Projection Model", SourceReference = "Demographic Projections 2025-2035", CreatedAt = DateTime.UtcNow },
                    new() { CountryId = somaliaCountry.Id, Year = 2035, TotalBirths = 936181, MaleBirths = 477269, FemaleBirths = 458912, RecordType = RecordType.Predicted, DataSource = "Somalia Demographic Projection Model", SourceReference = "Demographic Projections 2025-2035", CreatedAt = DateTime.UtcNow }
                };

                db.BirthRecords.AddRange(records);
                db.SaveChanges();
            }
        }
    }
}
