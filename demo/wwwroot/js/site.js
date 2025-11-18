// Enhanced with comprehensive automation features
class ClaimAutomation {
    constructor() {
        this.initEventListeners();
        this.calculateTotal();
        this.autoSaveForm();
    }

    initEventListeners() {
        // Real-time calculation listeners
        const calculationInputs = ['rate', 'hoursWorked'];
        calculationInputs.forEach(id => {
            const element = document.getElementById(id);
            if (element) {
                element.addEventListener('input', () => {
                    this.calculateTotal();
                    this.validateInputs();
                    this.autoSaveForm();
                });
            }
        });

        // Faculty-based rate suggestions
        const facultySelect = document.getElementById('faculty');
        if (facultySelect) {
            facultySelect.addEventListener('change', () => {
                this.suggestHourlyRate();
                this.autoSaveForm();
            });
        }

        // File upload handling
        const fileInput = document.getElementById('uploadFile');
        if (fileInput) {
            fileInput.addEventListener('change', (e) => this.handleFileUpload(e));
        }

        // Form submission validation
        const claimForm = document.getElementById('claimForm');
        if (claimForm) {
            claimForm.addEventListener('submit', (e) => this.validateForm(e));
        }

        // Auto-load saved form data
        this.loadSavedForm();
    }

    calculateTotal() {
        const rate = parseFloat(document.getElementById('rate')?.value) || 0;
        const hours = parseFloat(document.getElementById('hoursWorked')?.value) || 0;
        const total = rate * hours;

        // Update display elements
        const rateDisplay = document.getElementById('rateDisplay');
        const hoursDisplay = document.getElementById('hoursDisplay');
        const totalDisplay = document.getElementById('totalDisplay');

        if (rateDisplay) rateDisplay.textContent = 'R ' + rate.toFixed(2);
        if (hoursDisplay) hoursDisplay.textContent = hours.toFixed(1) + ' hours';
        if (totalDisplay) totalDisplay.textContent = 'R ' + total.toFixed(2);

        // Visual feedback for calculations
        this.updateCalculationFeedback(rate, hours, total);
    }

    updateCalculationFeedback(rate, hours, total) {
        const summary = document.querySelector('.calculation-summary');
        if (!summary) return;

        // Remove existing feedback
        const existingFeedback = summary.querySelector('.calculation-feedback');
        if (existingFeedback) {
            existingFeedback.remove();
        }

        // Add new feedback
        let feedbackMessage = '';
        let feedbackClass = '';

        if (rate > 800) {
            feedbackMessage = '⚠️ High hourly rate - may require additional approval';
            feedbackClass = 'warning';
        } else if (hours > 40) {
            feedbackMessage = 'ℹ️ Significant hours worked - ensure proper documentation';
            feedbackClass = 'info';
        } else if (total > 5000) {
            feedbackMessage = '💰 Large claim amount - will be prioritized for review';
            feedbackClass = 'success';
        }

        if (feedbackMessage) {
            const feedbackDiv = document.createElement('div');
            feedbackDiv.className = `calculation-feedback ${feedbackClass}`;
            feedbackDiv.innerHTML = `<p>${feedbackMessage}</p>`;
            summary.appendChild(feedbackDiv);
        }
    }

    suggestHourlyRate() {
        const faculty = document.getElementById('faculty')?.value;
        const rateInput = document.getElementById('rate');

        if (!faculty || !rateInput) return;

        const rateSuggestions = {
            'ICT': '650 - 850',
            'Education': '450 - 650',
            'Law': '700 - 900',
            'Commerce': '600 - 800',
            'Humanities': '400 - 600',
            'Finance and Accounting': '750 - 950'
        };

        const suggestion = rateSuggestions[faculty];
        if (suggestion) {
            // Update placeholder with suggestion
            rateInput.placeholder = `Suggested: R${suggestion}`;

            // Show suggestion tooltip
            this.showRateSuggestion(suggestion, faculty);
        }
    }

    showRateSuggestion(suggestion, faculty) {
        // Remove existing suggestion
        const existingSuggestion = document.getElementById('rateSuggestion');
        if (existingSuggestion) {
            existingSuggestion.remove();
        }

        // Create new suggestion element
        const suggestionDiv = document.createElement('div');
        suggestionDiv.id = 'rateSuggestion';
        suggestionDiv.className = 'rate-suggestion';
        suggestionDiv.innerHTML = `
            <small>💡 Suggested rate for ${faculty}: R${suggestion}</small>
        `;

        const rateGroup = document.querySelector('label[for="rate"]').parentElement;
        rateGroup.appendChild(suggestionDiv);

        // Auto-hide after 10 seconds
        setTimeout(() => {
            if (suggestionDiv.parentElement) {
                suggestionDiv.remove();
            }
        }, 10000);
    }

    validateInputs() {
        const rate = parseFloat(document.getElementById('rate')?.value) || 0;
        const hours = parseFloat(document.getElementById('hoursWorked')?.value) || 0;

        this.validateRate(rate);
        this.validateHours(hours);
    }

    validateRate(rate) {
        const rateInput = document.getElementById('rate');
        if (!rateInput) return;

        if (rate < 1) {
            this.showValidationError(rateInput, 'Hourly rate must be at least R1');
        } else if (rate > 1000) {
            this.showValidationError(rateInput, 'Hourly rate cannot exceed R1000');
        } else {
            this.clearValidationError(rateInput);
        }
    }

    validateHours(hours) {
        const hoursInput = document.getElementById('hoursWorked');
        if (!hoursInput) return;

        if (hours < 1) {
            this.showValidationError(hoursInput, 'Hours worked must be at least 1');
        } else if (hours > 200) {
            this.showValidationError(hoursInput, 'Hours worked cannot exceed 200 per month');
        } else {
            this.clearValidationError(hoursInput);
        }
    }

    showValidationError(input, message) {
        this.clearValidationError(input);
        input.classList.add('is-invalid');

        const errorDiv = document.createElement('div');
        errorDiv.className = 'invalid-feedback';
        errorDiv.textContent = message;
        input.parentNode.appendChild(errorDiv);
    }

    clearValidationError(input) {
        input.classList.remove('is-invalid');
        const existingError = input.parentNode.querySelector('.invalid-feedback');
        if (existingError) {
            existingError.remove();
        }
    }

    handleFileUpload(e) {
        const fileList = document.getElementById('fileList');
        if (!fileList) return;

        fileList.innerHTML = '';

        if (e.target.files.length > 0) {
            Array.from(e.target.files).forEach(file => {
                // Validate file size (5MB limit)
                if (file.size > 5 * 1024 * 1024) {
                    this.showNotification(`File ${file.name} exceeds 5MB limit`, 'error');
                    return;
                }

                // Validate file type
                const allowedTypes = ['.pdf', '.docx', '.xlsx'];
                const fileExtension = '.' + file.name.split('.').pop().toLowerCase();
                if (!allowedTypes.includes(fileExtension)) {
                    this.showNotification(`File ${file.name} is not a supported type. Please upload PDF, DOCX, or XLSX files.`, 'error');
                    return;
                }

                const fileItem = this.createFileItem(file);
                fileList.appendChild(fileItem);
            });

            this.showNotification(`Successfully added ${e.target.files.length} file(s)`, 'success');
        }
    }

    createFileItem(file) {
        const fileItem = document.createElement('div');
        fileItem.className = 'file-item';
        fileItem.innerHTML = `
            <div class="file-info">
                <span class="file-icon">📄</span>
                <span class="file-name">${file.name}</span>
                <span class="file-size">(${(file.size / 1024 / 1024).toFixed(2)} MB)</span>
            </div>
            <button type="button" class="file-remove" onclick="this.closest('.file-item').remove()">×</button>
        `;
        return fileItem;
    }

    validateForm(e) {
        const rate = parseFloat(document.getElementById('rate')?.value) || 0;
        const hours = parseFloat(document.getElementById('hoursWorked')?.value) || 0;
        const faculty = document.getElementById('faculty')?.value;
        const module = document.getElementById('module')?.value;

        let isValid = true;

        // Validate required fields
        if (!faculty) {
            this.showNotification('Please select a faculty', 'error');
            isValid = false;
        }

        if (!module) {
            this.showNotification('Please select a module', 'error');
            isValid = false;
        }

        if (hours < 1 || hours > 200) {
            this.showNotification('Hours worked must be between 1 and 200', 'error');
            isValid = false;
        }

        if (rate < 1 || rate > 1000) {
            this.showNotification('Hourly rate must be between R1 and R1000', 'error');
            isValid = false;
        }

        if (!isValid) {
            e.preventDefault();
            this.showNotification('Please fix the errors before submitting', 'error');
        } else {
            this.clearSavedForm(); // Clear saved data on successful submission
            this.showNotification('Submitting your claim...', 'info');
        }

        return isValid;
    }

    autoSaveForm() {
        const formData = {
            faculty: document.getElementById('faculty')?.value,
            module: document.getElementById('module')?.value,
            hours: document.getElementById('hoursWorked')?.value,
            rate: document.getElementById('rate')?.value,
            claimDate: document.getElementById('claimDate')?.value
        };
        localStorage.setItem('claimFormDraft', JSON.stringify(formData));
    }

    loadSavedForm() {
        const saved = localStorage.getItem('claimFormDraft');
        if (saved) {
            const formData = JSON.parse(saved);

            const setValue = (id, value) => {
                const element = document.getElementById(id);
                if (element && value) element.value = value;
            };

            setValue('faculty', formData.faculty);
            setValue('module', formData.module);
            setValue('hoursWorked', formData.hours);
            setValue('rate', formData.rate);
            setValue('claimDate', formData.claimDate);

            this.calculateTotal();
            this.suggestHourlyRate();

            this.showNotification('Loaded previously saved form data', 'info');
        }
    }

    clearSavedForm() {
        localStorage.removeItem('claimFormDraft');
    }

    showNotification(message, type) {
        // Remove existing notifications
        const existingNotifications = document.querySelectorAll('.automation-notification');
        existingNotifications.forEach(notification => notification.remove());

        const notification = document.createElement('div');
        notification.className = `automation-notification notification-${type}`;
        notification.innerHTML = `
            <span>${message}</span>
            <button onclick="this.parentElement.remove()">×</button>
        `;

        document.body.appendChild(notification);

        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (notification.parentElement) {
                notification.remove();
            }
        }, 5000);
    }
}

// Initialize automation when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    new ClaimAutomation();

    // Initialize calculations
    if (typeof calculateTotal === 'function') {
        calculateTotal();
    }
});

// Global function for backward compatibility
function calculateTotal() {
    const rate = parseFloat(document.getElementById('rate')?.value) || 0;
    const hours = parseFloat(document.getElementById('hoursWorked')?.value) || 0;
    const total = rate * hours;

    const rateDisplay = document.getElementById('rateDisplay');
    const hoursDisplay = document.getElementById('hoursDisplay');
    const totalDisplay = document.getElementById('totalDisplay');

    if (rateDisplay) rateDisplay.textContent = 'R ' + rate.toFixed(2);
    if (hoursDisplay) hoursDisplay.textContent = hours.toFixed(1) + ' hours';
    if (totalDisplay) totalDisplay.textContent = 'R ' + total.toFixed(2);
}