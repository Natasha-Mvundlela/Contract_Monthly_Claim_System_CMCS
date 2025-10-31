function calculateTotal() {
    const rate = parseFloat(document.getElementById('rate').value) || 0;
    const hours = parseFloat(document.getElementById('hoursWorked').value) || 0;
    const total = rate * hours;

    document.getElementById('rateDisplay').textContent = 'R ' + rate.toFixed(2);
    document.getElementById('hoursDisplay').textContent = hours.toFixed(1) + ' hours';
    document.getElementById('totalDisplay').textContent = 'R ' + total.toFixed(2);
}

// File upload display
document.getElementById('uploadFile').addEventListener('change', function (e) {
    const fileList = document.getElementById('fileList');
    fileList.innerHTML = '';

    if (e.target.files.length > 0) {
        Array.from(e.target.files).forEach(file => {
            const fileItem = document.createElement('div');
            fileItem.className = 'file-item';
            fileItem.innerHTML = `
                <span>📄 ${file.name}</span>
                <span>(${(file.size / 1024 / 1024).toFixed(2)} MB)</span>
            `;
            fileList.appendChild(fileItem);
        });
    }
});

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    calculateTotal();
});