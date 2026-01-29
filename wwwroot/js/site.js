// NCR Management System - Main JavaScript File
// Global functions for the NCR system

$(document).ready(function () {
    // Initialize common functionality
    initializeCommonFeatures();

    // Initialize form validation
    initializeFormValidation();

    // Initialize file upload functionality
    initializeFileUpload();

    // Initialize tooltips and popovers
    initializeBootstrapComponents();
});

// Common feature initialization
function initializeCommonFeatures() {
    // Auto-hide success/error messages after 5 seconds
    setTimeout(function () {
        $('.alert:not(.alert-permanent)').fadeOut();
    }, 5000);

    // Add loading state to buttons with data-loading attribute
    $('[data-loading]').on('click', function () {
        const $btn = $(this);
        const originalText = $btn.html();

        $btn.prop('disabled', true)
            .html('<span class="spinner-border spinner-border-sm me-2"></span>กำลังประมวลผล...');

        // Restore button after 5 seconds (fallback)
        setTimeout(function () {
            $btn.prop('disabled', false).html(originalText);
        }, 5000);
    });

    // Confirm delete actions
    $('[data-confirm-delete]').on('click', function (e) {
        e.preventDefault();
        const $element = $(this);
        const message = $element.data('confirm-delete') || 'คุณแน่ใจหรือไม่ว่าต้องการลบรายการนี้?';

        Swal.fire({
            title: 'ยืนยันการลบ',
            text: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'ลบ',
            cancelButtonText: 'ยกเลิก'
        }).then((result) => {
            if (result.isConfirmed) {
                if ($element.is('form')) {
                    $element.submit();
                } else {
                    window.location.href = $element.attr('href');
                }
            }
        });
    });
}

// Form validation initialization
function initializeFormValidation() {
    // Custom validation messages in Thai
    $.extend($.validator.messages, {
        required: "กรุณากรอกข้อมูลในช่องนี้",
        email: "กรุณากรอกอีเมลที่ถูกต้อง",
        url: "กรุณากรอก URL ที่ถูกต้อง",
        date: "กรุณากรอกวันที่ที่ถูกต้อง",
        number: "กรุณากรอกตัวเลขเท่านั้น",
        digits: "กรุณากรอกตัวเลขเท่านั้น",
        minlength: $.validator.format("กรุณากรอกอย่างน้อย {0} ตัวอักษร"),
        maxlength: $.validator.format("กรุณากรอกไม่เกิน {0} ตัวอักษร"),
        rangelength: $.validator.format("กรุณากรอกความยาวระหว่าง {0} ถึง {1} ตัวอักษร"),
        range: $.validator.format("กรุณากรอกค่าระหว่าง {0} ถึง {1}"),
        max: $.validator.format("กรุณากรอกค่าไม่เกิน {0}"),
        min: $.validator.format("กรุณากรอกค่าอย่างน้อย {0}")
    });

    // Real-time validation for forms
    //$('form').each(function () {
    //    const $form = $(this);

    //    $form.find('input, select, textarea').on('blur', function () {
    //        validateField($(this));
    //    });

    //    $form.on('submit', function (e) {
    //        let isValid = true;

    //        $form.find('input[required], select[required], textarea[required]').each(function () {
    //            if (!validateField($(this))) {
    //                isValid = false;
    //            }
    //        });

    //        if (!isValid) {
    //            e.preventDefault();
    //            showAlert('error', 'กรุณากรอกข้อมูลที่จำเป็นให้ครบถ้วน');
    //        }
    //    });
    //});
}

function ajaxGet(url, data, successCallback, errorCallback) {
    $.ajax({
        url: url,
        type: 'GET',
        data: data,
        success: function (response) {
            if (successCallback) successCallback(response);
        },
        error: function (xhr, status, error) {
            if (errorCallback) errorCallback({ success: false, message: error });

            if (xhr.status === 401) {
                window.location.href = '/Auth/Login';
            } else {
                showAlert('error', 'ไม่สามารถโหลดข้อมูลได้');
            }
        }
    });
}

// NCR specific functions
function updateNCRStatus(ncrId, newStatus, comments = '') {
    const data = {
        ncrId: ncrId,
        newStatus: newStatus,
        comments: comments,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    };

    // Debug
    console.log('Updating NCR status:', data);

    ajaxPost('/NCR/UpdateStatus', data, function (response) {
        if (response.success) {
            location.reload();
        }
    }, function (error) {
        console.log('Update status error:', error);
    });
}

function addNCRComment(ncrId, commentText, commentType = 'General') {
    if (!commentText.trim()) {
        showAlert('error', 'กรุณากรอกความคิดเห็น');
        return;
    }

    const data = {
        ncrId: ncrId,
        commentText: commentText,
        commentType: commentType,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    };

    ajaxPost('/NCR/AddComment', data, function (response) {
        if (response.success) {
            $('#commentText').val('');
            loadNCRComments(ncrId);
        }
    });
}

function uploadNCRFile(ncrId, fileInput, category = 'General') {
    const files = fileInput.files;
    if (files.length === 0) {
        showAlert('error', 'กรุณาเลือกไฟล์');
        return;
    }

    const formData = new FormData();
    formData.append('ncrId', ncrId);
    formData.append('file', files[0]);
    formData.append('category', category);
    formData.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());

    $.ajax({
        url: '/NCR/UploadFile',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                showToast('อัปโหลดไฟล์เรียบร้อยแล้ว');
                $(fileInput).val('');
                loadNCRFiles(ncrId);
            } else {
                showAlert('error', response.message || 'ไม่สามารถอัปโหลดไฟล์ได้');
            }
        },
        error: function () {
            showAlert('error', 'เกิดข้อผิดพลาดในการอัปโหลดไฟล์');
        }
    });
}

function deleteNCRFile(fileId) {
    Swal.fire({
        title: 'ยืนยันการลบไฟล์',
        text: 'คุณแน่ใจหรือไม่ว่าต้องการลบไฟล์นี้?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'ลบ',
        cancelButtonText: 'ยกเลิก'
    }).then((result) => {
        if (result.isConfirmed) {
            const data = {
                fileId: fileId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            };

            ajaxPost('/NCR/DeleteFile', data, function (response) {
                if (response.success) {
                    $(`[data-file-id="${fileId}"]`).fadeOut(() => {
                        $(`[data-file-id="${fileId}"]`).remove();
                    });
                }
            });
        }
    });
}

// Table helpers
function initializeDataTable(tableSelector, options = {}) {
    const defaultOptions = {
        responsive: true,
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.4/i18n/th.json'
        },
        pageLength: 10,
        ordering: true,
        searching: true,
        info: true,
        pagingType: 'full_numbers'
    };

    const finalOptions = { ...defaultOptions, ...options };
    return $(tableSelector).DataTable(finalOptions);
}

// Print functionality
function printPage() {
    window.print();
}

function printElement(elementSelector) {
    const element = $(elementSelector)[0];
    if (!element) return;

    const printWindow = window.open('', '_blank');
    printWindow.document.write(`
        <html>
            <head>
                <title>พิมพ์เอกสาร</title>
                <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
                <style>
                    body { font-family: 'Sarabun', sans-serif; }
                    @media print {
                        .btn, .no-print { display: none !important; }
                        .table { font-size: 12px; }
                    }
                </style>
            </head>
            <body>
                ${element.outerHTML}
            </body>
        </html>
    `);

    printWindow.document.close();
    printWindow.focus();

    setTimeout(() => {
        printWindow.print();
        printWindow.close();
    }, 500);
}

// Export functionality
function exportToExcel(data, filename = 'export.xlsx') {
    // This would require a library like SheetJS
    // For now, we'll redirect to a server-side export endpoint
    const params = new URLSearchParams(data);
    window.open(`/Report/ExportExcel?${params}`, '_blank');
}

// Search functionality
function initializeSearch(searchInputSelector, targetSelector, searchCallback) {
    let searchTimeout;

    $(searchInputSelector).on('input', function () {
        const searchTerm = $(this).val();

        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            if (searchCallback) {
                searchCallback(searchTerm);
            } else {
                // Default search behavior
                const $targets = $(targetSelector);

                if (searchTerm.length === 0) {
                    $targets.show();
                } else {
                    $targets.each(function () {
                        const text = $(this).text().toLowerCase();
                        const matches = text.includes(searchTerm.toLowerCase());
                        $(this).toggle(matches);
                    });
                }
            }
        }, 300);
    });
}

// Form helpers
function resetForm(formSelector) {
    const $form = $(formSelector);
    $form[0].reset();
    $form.find('.is-invalid').removeClass('is-invalid');
    $form.find('.invalid-feedback').remove();
    $form.find('.file-list').empty();
}

function serializeFormToObject(formSelector) {
    const formArray = $(formSelector).serializeArray();
    const formObject = {};

    $.each(formArray, function (i, field) {
        if (formObject[field.name]) {
            if (!Array.isArray(formObject[field.name])) {
                formObject[field.name] = [formObject[field.name]];
            }
            formObject[field.name].push(field.value);
        } else {
            formObject[field.name] = field.value;
        }
    });

    return formObject;
}

// Date helpers
function formatThaiDate(dateString) {
    const date = new Date(dateString);
    const thaiMonths = [
        'มกราคม', 'กุมภาพันธ์', 'มีนาคม', 'เมษายน', 'พฤษภาคม', 'มิถุนายน',
        'กรกฎาคม', 'สิงหาคม', 'กันยายน', 'ตุลาคม', 'พฤศจิกายน', 'ธันวาคม'
    ];

    const day = date.getDate();
    const month = thaiMonths[date.getMonth()];
    const year = date.getFullYear() + 543; // Convert to Buddhist Era

    return `${day} ${month} ${year}`;
}

function getDaysRemaining(dueDateString) {
    const dueDate = new Date(dueDateString);
    const today = new Date();
    const diffTime = dueDate - today;
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    return diffDays;
}

// Local storage helpers
function saveToLocalStorage(key, data) {
    try {
        localStorage.setItem(key, JSON.stringify(data));
    } catch (e) {
        console.warn('Cannot save to localStorage:', e);
    }
}

function getFromLocalStorage(key, defaultValue = null) {
    try {
        const item = localStorage.getItem(key);
        return item ? JSON.parse(item) : defaultValue;
    } catch (e) {
        console.warn('Cannot read from localStorage:', e);
        return defaultValue;
    }
}

function removeFromLocalStorage(key) {
    try {
        localStorage.removeItem(key);
    } catch (e) {
        console.warn('Cannot remove from localStorage:', e);
    }
}

// Performance monitoring
function measurePerformance(name, fn) {
    const start = performance.now();
    const result = fn();
    const end = performance.now();

    console.log(`${name} took ${end - start} milliseconds`);
    return result;
}

// Error reporting
function reportError(error, context = '') {
    console.error('Error occurred:', error, 'Context:', context);

    // In production, you might want to send this to a logging service
    // fetch('/api/log-error', {
    //     method: 'POST',
    //     headers: { 'Content-Type': 'application/json' },
    //     body: JSON.stringify({ error: error.toString(), context })
    // });
}

// Global error handler
window.addEventListener('error', function (e) {
    reportError(e.error, 'Global error handler');
});

window.addEventListener('unhandledrejection', function (e) {
    reportError(e.reason, 'Unhandled promise rejection');
});

// เพิ่มฟังก์ชันนี้ใน site.js
function initFileUpload(uploadAreaSelector, fileInputSelector, callback) {
    const $uploadArea = $(uploadAreaSelector);
    const $fileInput = $(fileInputSelector);

    if (!$uploadArea.length || !$fileInput.length) return;

    // Click to select file
    $uploadArea.on('click', function (e) {
        if (!$(e.target).is('button, a')) {
            $fileInput.click();
        }
    });

    // Drag and drop
    $uploadArea.on('dragover', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass('dragover');
    });

    $uploadArea.on('dragleave', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('dragover');
    });

    $uploadArea.on('drop', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('dragover');

        const files = e.originalEvent.dataTransfer.files;
        if (files.length > 0) {
            $fileInput[0].files = files;
            if (callback) callback(files);
        }
    });

    // File input change
    $fileInput.on('change', function () {
        if (callback) callback(this.files);
    });
}

window.NCRSystem = {
    showAlert,
    showToast,
    ajaxPost,
    ajaxGet,
    updateNCRStatus,
    addNCRComment,
    uploadNCRFile,
    deleteNCRFile,
    formatFileSize,
    formatDate,
    formatThaiDate,
    getDaysRemaining,
    initializeDataTable,
    printPage,
    printElement,
    exportToExcel,
    resetForm,
    serializeFormToObject,
    initFileUpload,
    getFromLocalStorage,                   // ← เพิ่มตรงนี้
    saveToLocalStorage,                    // ← เพิ่มตรงนี้
    removeFromLocalStorage,                // ← เพิ่มตรงนี้
    validateField,                         // ← เพิ่มตรงนี้
    updateFileList,                        // ← เพิ่มตรงนี้
    removeFile                             // ← เพิ่มตรงนี้
};

// Field validation function
function validateField($field) {
    const value = $field.val();
    const fieldType = $field.attr('type');
    const isRequired = $field.prop('required');

    // Remove previous error styling
    $field.removeClass('is-invalid');
    $field.siblings('.invalid-feedback').remove();

    // Check if required field is empty
    if (isRequired && (!value || value.trim() === '')) {
        showFieldError($field, 'กรุณากรอกข้อมูลในช่องนี้');
        return false;
    }

    // Validate email format
    if (fieldType === 'email' && value && !isValidEmail(value)) {
        showFieldError($field, 'กรุณากรอกอีเมลที่ถูกต้อง');
        return false;
    }

    // Validate file size and type
    if (fieldType === 'file' && $field[0].files.length > 0) {
        const file = $field[0].files[0];
        const maxSize = 10 * 1024 * 1024; // 10MB
        const allowedTypes = ['.jpg', '.jpeg', '.png', '.pdf', '.xlsx', '.xls', '.doc', '.docx'];

        if (file.size > maxSize) {
            showFieldError($field, 'ไฟล์มีขนาดเกิน 10MB');
            return false;
        }

        const fileExt = '.' + file.name.split('.').pop().toLowerCase();
        if (!allowedTypes.includes(fileExt)) {
            showFieldError($field, 'ประเภทไฟล์ไม่ถูกต้อง');
            return false;
        }
    }

    return true;
}

// Show field error
function showFieldError($field, message) {
    $field.addClass('is-invalid');
    $field.after(`<div class="invalid-feedback">${message}</div>`);
}

// Email validation
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// File upload initialization
function initializeFileUpload() {
    // Initialize drag and drop for file upload areas
    $('.file-upload-area').each(function () {
        const $uploadArea = $(this);
        const $fileInput = $uploadArea.siblings('input[type="file"]') || $uploadArea.find('input[type="file"]');

        if ($fileInput.length) {
            initDragAndDrop($uploadArea, $fileInput);
        }
    });
}

// Drag and drop functionality
function initDragAndDrop($uploadArea, $fileInput) {
    $uploadArea.on('click', function (e) {
        if (!$(e.target).is('button, a')) {
            $fileInput.click();
        }
    });

    $uploadArea.on('dragover', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass('dragover');
    });

    $uploadArea.on('dragleave', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('dragover');
    });

    $uploadArea.on('drop', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('dragover');

        const files = e.originalEvent.dataTransfer.files;
        if (files.length > 0) {
            $fileInput[0].files = files;
            $fileInput.trigger('change');
        }
    });

    $fileInput.on('change', function () {
        updateFileList($(this));
    });
}

// Update file list display
function updateFileList($fileInput) {
    const files = $fileInput[0].files;
    const $container = $fileInput.siblings('.file-list') || $fileInput.closest('.form-group').find('.file-list');

    if (!$container.length) return;

    $container.empty();

    Array.from(files).forEach(file => {
        const fileItem = $(`
            <div class="d-flex align-items-center justify-content-between p-2 bg-light rounded mb-2">
                <div class="d-flex align-items-center">
                    <i class="fas fa-file me-2"></i>
                    <div>
                        <div class="fw-medium">${file.name}</div>
                        <small class="text-muted">${formatFileSize(file.size)}</small>
                    </div>
                </div>
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeFile(this)">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `);

        $container.append(fileItem);
    });
}

// Remove file from list
function removeFile(button) {
    $(button).closest('.d-flex').remove();
}

// Bootstrap components initialization
function initializeBootstrapComponents() {
    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

// Utility functions
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function formatDate(dateString, includeTime = false) {
    const date = new Date(dateString);
    const options = {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    };

    if (includeTime) {
        options.hour = '2-digit';
        options.minute = '2-digit';
    }

    return date.toLocaleDateString('th-TH', options);
}



function showToast(message, type = 'success') {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

    Toast.fire({
        icon: type,
        title: message
    });
}

function showAlert(type, message, title = '') {
    const iconType = type === 'error' ? 'error' : type === 'success' ? 'success' : 'info';

    Swal.fire({
        icon: iconType,
        title: title || (type === 'error' ? 'เกิดข้อผิดพลาด' : type === 'success' ? 'สำเร็จ' : 'แจ้งเตือน'),
        text: message,
        confirmButtonText: 'ตกลง'
    });
}

// AJAX helpers
function ajaxPost(url, data, successCallback, errorCallback) {
    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                if (successCallback) successCallback(response);
                if (response.message) showToast(response.message, 'success');
            } else {
                if (errorCallback) errorCallback(response);
                showAlert('error', response.message || 'การดำเนินการไม่สำเร็จ');
            }
        },
        error: function (xhr, status, error) {
            // เพิ่ม debug ตรงนี้
            console.log('AJAX Error:', xhr.status, xhr.responseText);

            if (errorCallback) errorCallback({ success: false, message: error });

            if (xhr.status === 401) {
                showAlert('error', 'กรุณาเข้าสู่ระบบใหม่', 'หมดเวลาการใช้งาน');
                setTimeout(() => {
                    window.location.href = '/Auth/Login';
                }, 2000);
            } else {
                showAlert('error', 'ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์ได้');
            }
        }
    });
}