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

            // Ensure 2015-2035 Somalia baseline records exist
            var existingYears = db.BirthRecords
                .Where(r => r.CountryId == somaliaCountry.Id && r.CityId == null && r.Year >= 2015 && r.Year <= 2035)
                .Select(r => r.Year)
                .ToHashSet();

            var baseline = new (int Year, int Total, int Male, int Female)[]
            {
                (2015, 651292, 332031, 319261),
                (2016, 668096, 340598, 327498),
                (2017, 688481, 350990, 337491),
                (2018, 705717, 359777, 345940),
                (2019, 723617, 368903, 354714),
                (2020, 741705, 378124, 363581),
                (2021, 761567, 388250, 373317),
                (2022, 779534, 397409, 382125),
                (2023, 788763, 402114, 386649),
                (2024, 804966, 410375, 394591),
                (2025, 822215, 419168, 403047),
                (2026, 836420, 426410, 410010),
                (2027, 847521, 432070, 415451),
                (2028, 859891, 438376, 421515),
                (2029, 869665, 443359, 426306),
                (2030, 878445, 447835, 430610),
                (2031, 891665, 454574, 437091),
                (2032, 901357, 459515, 441842),
                (2033, 915030, 466486, 448544),
                (2034, 924402, 471264, 453138),
                (2035, 936181, 477269, 458912)
            };

            var recordsToAdd = new List<BirthRecord>();
            foreach (var item in baseline)
            {
                if (!existingYears.Contains(item.Year))
                {
                    recordsToAdd.Add(new BirthRecord
                    {
                        CountryId = somaliaCountry.Id,
                        CityId = null,
                        Year = item.Year,
                        TotalBirths = item.Total,
                        MaleBirths = item.Male,
                        FemaleBirths = item.Female,
                        RecordType = RecordType.Estimated,
                        DataSource = "Somalia National Demographic Estimates",
                        SourceReference = "Demographic Survey 2015-2035",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (recordsToAdd.Any())
            {
                db.BirthRecords.AddRange(recordsToAdd);
                db.SaveChanges();
            }

            // Ensure Garowe city records exist
            var garoweCity = db.Cities.FirstOrDefault(c => c.CityName == "Garowe" && c.CountryId == somaliaCountry.Id);
            if (garoweCity != null)
            {
                bool garoweChanged = false;
                if (!db.BirthRecords.Any(r => r.CountryId == somaliaCountry.Id && r.CityId == garoweCity.Id && r.Year == 2025 && r.RecordType == RecordType.Estimated))
                {
                    db.BirthRecords.Add(new BirthRecord
                    {
                        CountryId = somaliaCountry.Id,
                        CityId = garoweCity.Id,
                        Year = 2025,
                        TotalBirths = 22000,
                        MaleBirths = 10500,
                        FemaleBirths = 11500,
                        RecordType = RecordType.Estimated,
                        DataSource = "puntland resource",
                        SourceReference = "puntland resource",
                        CreatedAt = DateTime.UtcNow
                    });
                    garoweChanged = true;
                }

                if (!db.BirthRecords.Any(r => r.CountryId == somaliaCountry.Id && r.CityId == garoweCity.Id && r.Year == 2026 && r.RecordType == RecordType.Official))
                {
                    db.BirthRecords.Add(new BirthRecord
                    {
                        CountryId = somaliaCountry.Id,
                        CityId = garoweCity.Id,
                        Year = 2026,
                        TotalBirths = 25000,
                        MaleBirths = 12400,
                        FemaleBirths = 12600,
                        RecordType = RecordType.Official,
                        DataSource = "garowe resource",
                        SourceReference = "garowe resource",
                        CreatedAt = DateTime.UtcNow
                    });
                    garoweChanged = true;
                }

                if (garoweChanged)
                {
                    db.SaveChanges();
                }
            }
        }
    }
}
