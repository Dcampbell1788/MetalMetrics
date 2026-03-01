// MetalMetrics — Site JavaScript

// Sidebar toggle (mobile)
(function () {
    var toggle = document.getElementById('sidebarToggle');
    var sidebar = document.getElementById('sidebar');
    var overlay = document.getElementById('sidebarOverlay');

    if (toggle && sidebar && overlay) {
        toggle.addEventListener('click', function () {
            sidebar.classList.toggle('show');
            overlay.classList.toggle('show');
        });

        overlay.addEventListener('click', function () {
            sidebar.classList.remove('show');
            overlay.classList.remove('show');
        });
    }
})();

// Admin/Users — search filter
(function () {
    var search = document.getElementById('userSearch');
    if (!search) return;

    search.addEventListener('keyup', function () {
        var term = this.value.toLowerCase();
        var items = document.querySelectorAll('.mm-user-item');
        items.forEach(function (item) {
            var text = item.textContent.toLowerCase();
            item.style.display = text.indexOf(term) !== -1 ? '' : 'none';
        });
    });
})();

// Sortable tables — auto-apply to any table with .mm-sortable
(function () {
    var tables = document.querySelectorAll('table.mm-sortable');
    tables.forEach(function (table) {
        var headers = table.querySelectorAll('th.sortable');
        var sortDir = {};

        headers.forEach(function (th, colIdx) {
            th.style.cursor = 'pointer';
            th.addEventListener('click', function () {
                var type = th.getAttribute('data-sort');
                var asc = sortDir[colIdx] = !sortDir[colIdx];
                var tbody = table.querySelector('tbody');
                var rows = Array.from(tbody.querySelectorAll('tr'));

                rows.sort(function (a, b) {
                    var aText = a.children[colIdx].textContent.trim();
                    var bText = b.children[colIdx].textContent.trim();

                    if (type === 'number') {
                        var aVal = parseFloat(aText.replace(/[^0-9.\-]/g, '')) || 0;
                        var bVal = parseFloat(bText.replace(/[^0-9.\-]/g, '')) || 0;
                        return asc ? aVal - bVal : bVal - aVal;
                    } else if (type === 'date') {
                        var aDate = new Date(aText);
                        var bDate = new Date(bText);
                        return asc ? aDate - bDate : bDate - aDate;
                    } else {
                        return asc ? aText.localeCompare(bText) : bText.localeCompare(aText);
                    }
                });

                rows.forEach(function (row) { tbody.appendChild(row); });

                headers.forEach(function (h) {
                    h.textContent = h.textContent.replace(/ ▲| ▼/g, '');
                });
                th.textContent += asc ? ' ▲' : ' ▼';
            });
        });
    });
})();

// Admin/Users — role change save button
(function () {
    var selects = document.querySelectorAll('.mm-role-select');
    selects.forEach(function (sel) {
        sel.addEventListener('change', function () {
            var btn = this.parentElement.querySelector('.mm-role-save');
            if (!btn) return;
            btn.style.display = this.value !== this.dataset.currentRole ? '' : 'none';
        });
    });
})();
