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

    countrySelect.addEventListener('change', function () {
      var countryId = countrySelect.value;
      citySelect.innerHTML = '<option value="">-- None / Country level --</option>';
      if (!countryId) return;

      fetch('/Cities/GetByCountry?countryId=' + countryId)
        .then(function (r) { return r.json(); })
        .then(function (cities) {
          cities.forEach(function (c) {
            var opt = document.createElement('option');
            opt.value = c.id;
            opt.textContent = c.cityName;
            citySelect.appendChild(opt);
          });
        });
    });
  });
});
