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
