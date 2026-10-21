// ====================================================================
// Nobre Arte Boxe Club - Gerenciamento de Tema (Modo Claro / Escuro)
// ====================================================================
(function () {
    function updateThemeUI(theme) {
        var btn = document.getElementById('themeToggleBtn');
        var text = document.getElementById('themeToggleText');
        var iconDark = document.querySelector('.theme-icon-dark');
        var iconLight = document.querySelector('.theme-icon-light');

        if (theme === 'light') {
            document.documentElement.setAttribute('data-theme', 'light');
            if (text) text.textContent = 'Modo Escuro';
            if (iconDark) iconDark.classList.add('d-none');
            if (iconLight) iconLight.classList.remove('d-none');
            if (btn) {
                btn.classList.remove('btn-outline-secondary');
                btn.classList.add('btn-outline-dark');
            }
        } else {
            document.documentElement.setAttribute('data-theme', 'dark');
            if (text) text.textContent = 'Modo Claro';
            if (iconDark) iconDark.classList.remove('d-none');
            if (iconLight) iconLight.classList.add('d-none');
            if (btn) {
                btn.classList.remove('btn-outline-dark');
                btn.classList.add('btn-outline-secondary');
            }
        }
    }

    // Inicialização ao carregar o DOM
    document.addEventListener('DOMContentLoaded', function () {
        var currentTheme = localStorage.getItem('boxe_gym_theme') || 'dark';
        updateThemeUI(currentTheme);

        var toggleBtn = document.getElementById('themeToggleBtn');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', function () {
                var activeTheme = document.documentElement.getAttribute('data-theme') || 'dark';
                var newTheme = activeTheme === 'dark' ? 'light' : 'dark';
                localStorage.setItem('boxe_gym_theme', newTheme);
                updateThemeUI(newTheme);
            });
        }
    });
})();
