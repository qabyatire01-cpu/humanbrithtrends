document.addEventListener('DOMContentLoaded', function () {
  var toggle = document.querySelector('.menu-toggle');
  var sidebar = document.querySelector('.sidebar');
  if (toggle && sidebar) {
    toggle.addEventListener('click', function () {
      sidebar.classList.toggle('open');
    });
  }

  document.querySelectorAll('.alert[data-autodismiss]').forEach(function (el) {
    setTimeout(function () {
      el.style.transition = 'opacity .4s ease';
      el.style.opacity = '0';
      setTimeout(function () { el.remove(); }, 400);
    }, 5000);
  });

  document.querySelectorAll('form[data-confirm]').forEach(function (form) {
    form.addEventListener('submit', function (e) {
      var msg = form.getAttribute('data-confirm') || 'Are you sure?';
      if (!confirm(msg)) {
        e.preventDefault();
      }
    });
  });

  document.querySelectorAll('select[data-country-source]').forEach(function (countrySelect) {
    var citySelectId = countrySelect.getAttribute('data-city-target');
    var citySelect = document.getElementById(citySelectId);
    if (!citySelect) return;

    var defaultLabel = (citySelect.options.length > 0 && citySelect.options[0].textContent.indexOf('All Cities') !== -1)
      ? 'All Cities'
      : '-- None / Country level --';

    function populateCities(countryId, keepSelection) {
      var currentVal = keepSelection ? citySelect.value : '';
      citySelect.innerHTML = '<option value="">' + defaultLabel + '</option>';
      if (!countryId) return;

      fetch('/Cities/GetByCountry?countryId=' + countryId)
        .then(function (r) { return r.json(); })
        .then(function (cities) {
          if (Array.isArray(cities)) {
            cities.forEach(function (c) {
              var id = (c.id !== undefined && c.id !== null) ? c.id : c.Id;
              var name = (c.cityName !== undefined && c.cityName !== null) ? c.cityName : c.CityName;
              var opt = document.createElement('option');
              opt.value = id;
              opt.textContent = name;
              if (String(id) === String(currentVal)) {
                opt.selected = true;
              }
              citySelect.appendChild(opt);
            });
          }
        })
        .catch(function (err) {
          console.error('Failed to load cities:', err);
        });
    }

    countrySelect.addEventListener('change', function () {
      populateCities(countrySelect.value, false);
    });

    if (countrySelect.value && citySelect.options.length <= 1) {
      populateCities(countrySelect.value, true);
    }
  });
});
