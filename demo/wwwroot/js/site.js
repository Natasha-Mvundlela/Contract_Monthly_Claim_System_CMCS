// Enhanced with real-time calculations and validation
function calculateTotal() {
    const rate = parseFloat(document.getElementById('rate').value) || 0;
    const hours = parseFloat(document.getElementById('hoursWorked').value) || 0;
    const total = rate * hours;

    document.getElementById('rateDisplay').textContent = 'R ' + rate.toFixed(2);
    document.getElementById('hoursDisplay').textContent = hours.toFixed(1) + ' hours';
    document.getElementById('totalDisplay').textContent = 'R ' + total.toFixed(2);
}

// Enhanced file upload with validation
document.addEventListener('DOMContentLoaded', function () {
    // Initialize calculations
    calculateTotal();

    // File upload handling
    const fileInput = document.getElementById('uploadFile');
    if (fileInput) {
        fileInput.addEventListener('change', function (e) {
            const fileList = document.getElementById('fileList');
            fileList.innerHTML = '';

            if (e.target.files.length > 0) {
                Array.from(e.target.files).forEach(file => {
                    // Validate file size (5MB limit)
                    if (file.size > 5 * 1024 * 1024) {
                        alert(`File ${file.name} exceeds 5MB limit`);
                        return;
                    }

                    // Validate file type
                    const allowedTypes = ['.pdf', '.docx', '.xlsx'];
                    const fileExtension = '.' + file.name.split('.').pop().toLowerCase();
                    if (!allowedTypes.includes(fileExtension)) {
                        alert(`File ${file.name} is not a supported type. Please upload PDF, DOCX, or XLSX files.`);
                        return;
                    }

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
    }

    // Form validation enhancement
    const claimForm = document.getElementById('claimForm');
    if (claimForm) {
        claimForm.addEventListener('submit', function (e) {
            const hours = parseFloat(document.getElementById('hoursWorked').value) || 0;
            const rate = parseFloat(document.getElementById('rate').value) || 0;

            if (hours < 1 || hours > 200) {
                e.preventDefault();
                alert('Hours worked must be between 1 and 200');
                return;
            }

            if (rate < 1 || rate > 1000) {
                e.preventDefault();
                alert('Hourly rate must be between R1 and R1000');
                return;
            }
        });
    }
});

// Auto-save form data
function autoSaveForm() {
    const formData = {
        faculty: document.getElementById('faculty')?.value,
        module: document.getElementById('module')?.value,
        hours: document.getElementById('hoursWorked')?.value,
        rate: document.getElementById('rate')?.value
    };
    localStorage.setItem('claimFormDraft', JSON.stringify(formData));
}

// Load saved form data
function loadSavedForm() {
    const saved = localStorage.getItem('claimFormDraft');
    if (saved) {
        const formData = JSON.parse(saved);
        if (document.getElementById('faculty')) document.getElementById('faculty').value = formData.faculty || '';
        if (document.getElementById('module')) document.getElementById('module').value = formData.module || '';
        if (document.getElementById('hoursWorked')) document.getElementById('hoursWorked').value = formData.hours || '';
        if (document.getElementById('rate')) document.getElementById('rate').value = formData.rate || '';
        calculateTotal();
    }
}

// Clear saved form data
function clearSavedForm() {
    localStorage.removeItem('claimFormDraft');
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    loadSavedForm();

    // Auto-save on input change
    const inputs = ['faculty', 'module', 'hoursWorked', 'rate'];
    inputs.forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.addEventListener('input', autoSaveForm);
        }
    });

    // Clear saved data on successful form submission
    const form = document.getElementById('claimForm');
    if (form) {
        form.addEventListener('submit', function () {
            setTimeout(clearSavedForm, 1000);
        });
    }
});