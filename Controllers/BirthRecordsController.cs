using HumanBirthPredictionSystem.Data;
using HumanBirthPredictionSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HumanBirthPredictionSystem.Controllers
{
    [Authorize]
    public class BirthRecordsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BirthRecordsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(int? countryId, int? cityId, int? year, RecordType? recordType, string? search)
        {
            var query = _db.BirthRecords.AsNoTracking()
                .Include(r => r.Country)
                .Include(r => r.City)
                .AsQueryable();

            if (countryId.HasValue) query = query.Where(r => r.CountryId == countryId);
            if (cityId.HasValue) query = query.Where(r => r.CityId == cityId);
            if (year.HasValue) query = query.Where(r => r.Year == year);
            if (recordType.HasValue) query = query.Where(r => r.RecordType == recordType);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(r => r.Country!.CountryName.Contains(search) || r.DataSource.Contains(search) || (r.City != null && r.City.CityName.Contains(search)));

            ViewBag.Countries = new SelectList(await _db.Countries.OrderBy(c => c.CountryName).ToListAsync(), "Id", "CountryName", countryId);
            ViewBag.Cities = new SelectList(
                countryId.HasValue
                    ? await _db.Cities.Where(c => c.CountryId == countryId.Value).OrderBy(c => c.CityName).ToListAsync()
                    : await _db.Cities.OrderBy(c => c.CityName).ToListAsync(),
                "Id", "CityName", cityId);
            ViewBag.CountryId = countryId;
            ViewBag.CityId = cityId;
            ViewBag.Year = year;
            ViewBag.RecordType = recordType;
            ViewBag.Search = search;

            var records = await query.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id).ToListAsync();
            return View(records);
        }

        public async Task<IActionResult> Create(int? countryId)
        {
            var countries = await _db.Countries.OrderBy(c => c.CountryName).ToListAsync();
            var somalia = countries.FirstOrDefault(c => c.CountryCode == "SOM") ?? countries.FirstOrDefault();
            var selectedCountryId = countryId ?? somalia?.Id;

            ViewBag.Countries = new SelectList(countries, "Id", "CountryName", selectedCountryId);
            ViewBag.Cities = new SelectList(
                selectedCountryId.HasValue
                    ? await _db.Cities.Where(c => c.CountryId == selectedCountryId.Value).OrderBy(c => c.CityName).ToListAsync()
                    : new List<City>(),
                "Id", "CityName");

            return View(new BirthRecord
            {
                CountryId = selectedCountryId ?? 0,
                RecordType = RecordType.Official,
                Year = DateTime.UtcNow.Year
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BirthRecord model)
        {
            await ValidateRecordAsync(model);

            if (!ModelState.IsValid)
            {
                ViewBag.Countries = new SelectList(await _db.Countries.OrderBy(c => c.CountryName).ToListAsync(), "Id", "CountryName", model.CountryId);
                ViewBag.Cities = new SelectList(await _db.Cities.Where(c => c.CountryId == model.CountryId).OrderBy(c => c.CityName).ToListAsync(), "Id", "CityName", model.CityId);
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            _db.BirthRecords.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Birth record was added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var record = await _db.BirthRecords.FindAsync(id);
            if (record == null) return NotFound();

            ViewBag.Countries = new SelectList(await _db.Countries.OrderBy(c => c.CountryName).ToListAsync(), "Id", "CountryName", record.CountryId);
            ViewBag.Cities = new SelectList(await _db.Cities.Where(c => c.CountryId == record.CountryId).OrderBy(c => c.CityName).ToListAsync(), "Id", "CityName", record.CityId);
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, BirthRecord model)
        {
            if (id != model.Id) return NotFound();

            await ValidateRecordAsync(model, id);

            if (!ModelState.IsValid)
            {
                ViewBag.Countries = new SelectList(await _db.Countries.OrderBy(c => c.CountryName).ToListAsync(), "Id", "CountryName", model.CountryId);
                ViewBag.Cities = new SelectList(await _db.Cities.Where(c => c.CountryId == model.CountryId).OrderBy(c => c.CityName).ToListAsync(), "Id", "CityName", model.CityId);
                return View(model);
            }

            var existing = await _db.BirthRecords.FindAsync(id);
            if (existing == null) return NotFound();

            existing.CountryId = model.CountryId;
            existing.CityId = model.CityId;
            existing.Year = model.Year;
            existing.TotalBirths = model.TotalBirths;
            existing.MaleBirths = model.MaleBirths;
            existing.FemaleBirths = model.FemaleBirths;
            existing.DataSource = model.DataSource ?? string.Empty;
            existing.SourceReference = model.SourceReference ?? string.Empty;
            existing.RecordType = model.RecordType;
            existing.CreatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Birth record was updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _db.BirthRecords.FindAsync(id);
            if (record == null) return NotFound();

            _db.BirthRecords.Remove(record);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Birth record deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateRecordAsync(BirthRecord model, int? excludingId = null)
        {
            // Auto-balance if user supplied total births or male/female
            if (model.TotalBirths > 0 && model.MaleBirths == 0 && model.FemaleBirths == 0)
            {
                model.MaleBirths = (int)Math.Round(model.TotalBirths * 0.512);
                model.FemaleBirths = model.TotalBirths - model.MaleBirths;
            }
            else if (model.MaleBirths + model.FemaleBirths > 0 && model.TotalBirths != model.MaleBirths + model.FemaleBirths)
            {
                model.TotalBirths = model.MaleBirths + model.FemaleBirths;
            }

            if (model.MaleBirths + model.FemaleBirths != model.TotalBirths)
            {
                ModelState.AddModelError(string.Empty,
                    "Male births plus female births must equal total births.");
            }

            if (model.CityId.HasValue)
            {
                var cityBelongs = await _db.Cities.AnyAsync(c => c.Id == model.CityId && c.CountryId == model.CountryId);
                if (!cityBelongs)
                    ModelState.AddModelError(nameof(model.CityId), "The selected city does not belong to the selected country.");
            }

            var duplicateQuery = _db.BirthRecords.Where(r =>
                r.CountryId == model.CountryId &&
                r.CityId == model.CityId &&
                r.Year == model.Year &&
                r.RecordType == model.RecordType);

            if (excludingId.HasValue)
                duplicateQuery = duplicateQuery.Where(r => r.Id != excludingId);

            if (await duplicateQuery.AnyAsync())
            {
                ModelState.AddModelError(string.Empty,
                    "A record already exists for this country, city, year, and record type.");
            }
        }
    }
}
