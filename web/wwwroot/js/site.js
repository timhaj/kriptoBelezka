// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

for (let i = 0; i < 100; i++) {
    value = document.getElementsByClassName('change')[i].innerHTML;
    value = parseFloat(value);
    if (value > 0) {
        document.getElementsByClassName('change')[i].classList.toggle("up");
        svg_up = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-caret-up-fill" viewBox="0 0 16 16">
  <path d="m7.247 4.86-4.796 5.481c-.566.647-.106 1.659.753 1.659h9.592a1 1 0 0 0 .753-1.659l-4.796-5.48a1 1 0 0 0-1.506 0z"/>
</svg>`;
        document.getElementsByClassName('change')[i].innerHTML = svg_up + ' ' + value;
    }
    else {
        document.getElementsByClassName('change')[i].classList.toggle("down");
        svg_down = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-caret-down-fill" viewBox="0 0 16 16">
            <path d="M7.247 11.14 2.451 5.658C1.885 5.013 2.345 4 3.204 4h9.592a1 1 0 0 1 .753 1.659l-4.796 5.48a1 1 0 0 1-1.506 0z" />
        </svg>`;
        document.getElementsByClassName('change')[i].innerHTML = svg_down + ' ' + value;
    }
}

for (let i = 0; i < 100; i++) {
    value = document.getElementsByClassName('mcap')[i].innerHTML;
    value = BigInt(value).toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    document.getElementsByClassName('mcap')[i].innerHTML = "$" + value;
}

for (let i = 0; i < 100; i++) {
    value = document.getElementsByClassName('volume')[i].innerHTML;
    value = BigInt(value).toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    document.getElementsByClassName('volume')[i].innerHTML = "$" + value;
}

for (let i = 0; i < 100; i++) {
    value = document.getElementsByClassName('supply')[i].innerHTML;
    value = BigInt(value).toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    document.getElementsByClassName('supply')[i].innerHTML = value;
}

for (let i = 0; i < 100; i++) {
    value = document.getElementsByClassName('msupply')[i].innerHTML;
    if (value == "N/A                    ") {
        document.getElementsByClassName('msupply')[i].innerHTML = value;
    }
    else {
        value = BigInt(value).toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
        document.getElementsByClassName('msupply')[i].innerHTML = value;
    }
}