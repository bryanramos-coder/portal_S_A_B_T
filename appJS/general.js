const menuBtn = document.getElementById('menuBtn');
const dropdownMenu = document.getElementById('dropdownMenu');

menuBtn.addEventListener('click', function (e) {
    e.stopPropagation();
    const abierto = dropdownMenu.classList.toggle('show');
    menuBtn.setAttribute('aria-expanded', abierto);
});
document.addEventListener('click', function (e) {
    if (!dropdownMenu.contains(e.target)) {
        dropdownMenu.classList.remove('show');
        menuBtn.setAttribute('aria-expanded', 'false');
    }
});
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        dropdownMenu.classList.remove('show');
        menuBtn.setAttribute('aria-expanded', 'false');
    }
});