// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function changeAllCheckboxs() {
    let mainCheckbox = document.querySelector('#mainCheckbox');
    document.querySelectorAll('input[type="checkbox"]').forEach(elem => {
        elem.checked = mainCheckbox.checked;
    });
}
