// Calculate total amount
function calculateTotal() {
    const rate = parseFloat(document.getElementById('rate').value) || 0;
    const hours = parseFloat(document.getElementById('hoursWorked').value) || 0;
    const total = rate * hours;

    document.getElementById('rateDisplay').textContent = 'R ' + rate.toFixed(2);
    document.getElementById('hoursDisplay').textContent = hours.toFixed(1) + ' hours';
    document.getElementById('totalDisplay').textContent = 'R ' + total.toFixed(2);

    // Update hidden calculated amount field if exists
    const calculatedAmountField = document.getElementById('Calculated_Amount');
    if (calculatedAmountField) {
        calculatedAmountField.value = total.toFixed(2);
    }
}

// Enhanced file upload functionality
const fileInput = document.getElementById('uploadFile');
const fileList = document.getElementById('fileList');
const supportingDocumentsInput = document.getElementById('supportingDocuments');
let uploadedFiles = [];

fileInput.addEventListener('change', function (e) {
    const files = Array.from(e.target.files);

    for (const file of files) {
        if (file.size > 5 * 1024 * 1024) {
            showNotification(`File ${file.name} is too large. Maximum size is 5MB.`, 'error');
            continue;
        }

        const allowedExtensions = ['.pdf', '.docx', '.xlsx'];
        const fileExtension = '.' + file.name.split('.').pop().toLowerCase();

        if (!allowedExtensions.includes(fileExtension)) {
            showNotification(`File ${file.name} is not allowed. Only PDF, DOCX, and XLSX files are permitted.`, 'error');
            continue;
        }

        uploadedFiles.push(file.name);
        displayFile(file.name);
        showNotification(`File ${file.name} uploaded successfully!`, 'success');
    }

    // Update hidden field with comma-separated file names
    supportingDocumentsInput.value = uploadedFiles.join(',');
    fileInput.value = ''; // Reset file input
});

function displayFile(fileName) {
    const li = document.createElement('li');
    li.className = 'file-item';
    li.innerHTML = `
        <span class="file-name">${fileName}</span>
        <button type="button" class="file-remove" onclick="removeFile('${fileName}')" title="Remove file">×</button>
    `;
    fileList.appendChild(li);
}

function removeFile(fileName) {
    uploadedFiles = uploadedFiles.filter(f => f !== fileName);
    supportingDocumentsInput.value = uploadedFiles.join(',');

    // Remove from UI
    const items = fileList.getElementsByTagName('li');
    for (let item of items) {
        if (item.textContent.includes(fileName)) {
            item.remove();
            break;
        }
    }

    showNotification(`File ${fileName} removed.`, 'info');
}

// Form validation and submission
function validateClaimForm() {
    const rate = parseFloat(document.getElementById('rate').value) || 0;
    const hours = parseFloat(document.getElementById('hoursWorked').value) || 0;

    if (rate <= 0 || rate > 1000) {
        showNotification('Hourly rate must be between R1 and R1000.', 'error');
        return false;
    }

    if (hours <= 0 || hours > 200) {
        showNotification('Hours worked must be between 1 and 200.', 'error');
        return false;
    }

    return true;
}

// Notification system
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <span class="notification-message">${message}</span>
        <button class="notification-close" onclick="this.parentElement.remove()">×</button>
    `;

    document.body.appendChild(notification);

    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);
}

// Real-time form validation
document.addEventListener('DOMContentLoaded', function () {
    // Initialize calculation
    calculateTotal();

    // Add real-time validation
    const rateInput = document.getElementById('rate');
    const hoursInput = document.getElementById('hoursWorked');

    if (rateInput) {
        rateInput.addEventListener('input', function () {
            const value = parseFloat(this.value) || 0;
            if (value < 1 || value > 1000) {
                this.style.borderColor = 'var(--error)';
            } else {
                this.style.borderColor = '';
            }
            calculateTotal();
        });
    }

    if (hoursInput) {
        hoursInput.addEventListener('input', function () {
            const value = parseFloat(this.value) || 0;
            if (value < 1 || value > 200) {
                this.style.borderColor = 'var(--error)';
            } else {
                this.style.borderColor = '';
            }
            calculateTotal();
        });
    }

    // Form submission handler
    const claimForm = document.querySelector('form');
    if (claimForm) {
        claimForm.addEventListener('submit', function (e) {
            if (!validateClaimForm()) {
                e.preventDefault();
                showNotification('Please fix the errors before submitting.', 'error');
            }
        });
    }
});

// Auto-save draft functionality
let autoSaveTimer;
function autoSaveDraft() {
    clearTimeout(autoSaveTimer);
    autoSaveTimer = setTimeout(() => {
        const formData = new FormData(document.querySelector('form'));
        localStorage.setItem('claimDraft', JSON.stringify(Object.fromEntries(formData)));
        showNotification('Draft saved automatically.', 'info');
    }, 2000);
}

// Load draft if exists
function loadDraft() {
    const draft = localStorage.getItem('claimDraft');
    if (draft) {
        if (confirm('Would you like to restore your previously saved draft?')) {
            const formData = JSON.parse(draft);
            for (const [key, value] of Object.entries(formData)) {
                const element = document.querySelector(`[name="${key}"]`);
                if (element) {
                    element.value = value;
                }
            }
            calculateTotal();
            showNotification('Draft restored successfully!', 'success');
        }
    }
}

// Clear draft on successful submission
function clearDraft() {
    localStorage.removeItem('claimDraft');
}