// Global variables
let currentSortColumn = '';
let currentSortDirection = 'asc';
let currentYarnItemId = 0;
let currentPage = 1;
let currentTransactions = [];
let transactionSortColumn = '';
let transactionSortDirection = 'asc';

// Enhanced Arabic digits conversion with better handling
const arabicDigits = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
const latinDigits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

// Convert any number to Arabic digits for display
function toArabicDigits(number) {
    if (number === null || number === undefined) return '';
    return number.toString()
        .replace(/\./g, '٫')  // Replace decimal point with Arabic decimal
        .replace(/\d/g, digit => arabicDigits[digit]);
}

// Convert Arabic digits back to Latin for processing
function toLatinDigits(arabicNumber) {
    if (arabicNumber === null || arabicNumber === undefined) return '';
    return arabicNumber.toString()
        .replace(/[٠-٩]/g, digit => latinDigits[arabicDigits.indexOf(digit)])
        .replace(/٫/g, '.'); // Convert Arabic decimal back to Latin
}

// Enhanced normalize search text - convert both Arabic and Latin numbers for comparison
function normalizeSearchText(text) {
    if (!text) return '';

    // Convert Arabic numbers to Latin for consistent searching
    let normalized = text.toString()
        .replace(/[٠-٩]/g, digit => latinDigits[arabicDigits.indexOf(digit)])
        .replace(/٫/g, '.')
        .replace(/[\u064B-\u065F\u0670]/g, '') // Remove diacritics
        .toLowerCase()
        .trim();

    return normalized;
}

// Convert user input to Arabic digits in real-time
function convertInputToArabic(inputElement) {
    const latinText = inputElement.value;
    const arabicText = toArabicDigits(latinText);
    if (latinText !== arabicText) {
        const cursorPosition = inputElement.selectionStart;
        inputElement.value = arabicText;
        // Maintain cursor position after conversion
        const newPosition = cursorPosition + (arabicText.length - latinText.length);
        inputElement.setSelectionRange(newPosition, newPosition);
    }
}

$(() => {
    // Initialize table sorting
    initializeTableSorting();

    // Initialize inline search
    initializeInlineSearch();

    // Load packaging breakdown for all items
    loadAllPackagingBreakdowns();

    // Initialize tooltips
    $('[title]').tooltip();

    // Convert numbers to Arabic
    convertNumbersToArabic();
});

// Convert all numbers to Arabic digits
function convertNumbersToArabic() {
    // Convert statistics cards
    $('#totalItemsDisplay, #availableItemsDisplay, #totalQuantityDisplay, #totalCountDisplay').each(function () {
        const text = $(this).text();
        $(this).text(toArabicDigits(text));
    });

    // Convert table numbers
    $('.quantity-balance, .total-transactions, .inbound-transactions, .outbound-transactions, .days-since').each(function () {
        const text = $(this).text();
        $(this).text(toArabicDigits(text));
    });

    // Convert last updated date numbers
    const lastUpdatedText = $('#lastUpdatedDisplay').text();
    $('#lastUpdatedDisplay').text(toArabicDigits(lastUpdatedText));
}

// Enhanced Inline Search Functionality with Arabic Number Support
function initializeInlineSearch() {
    $('.table-search').off('input keyup paste');
    
    $('.table-search').on('input keyup paste', function (e) {
        e.stopPropagation();
        
        // Convert input to Arabic digits in real-time
        convertInputToArabic(this);
        
        // Perform filtering
        filterTable();
    });

    // Convert search input placeholder text to Arabic numbers
    $('.table-search').each(function () {
        const placeholder = $(this).attr('placeholder');
        if (placeholder) {
            $(this).attr('placeholder', toArabicDigits(placeholder));
        }
    });

    // Prevent event bubbling on click
    $('.table-search').on('click', function (e) {
        e.stopPropagation();
    });

    // Handle focus events to ensure Arabic conversion
    $('.table-search').on('focus', function () {
        const $this = $(this);
        setTimeout(() => {
            if ($this.val()) {
                convertInputToArabic(this);
            }
        }, 10);
    });
}

function filterTable() {
    const availableOnly = $('#availableOnlyFilter').is(':checked');

    // Get all search values
    const searchValues = {};
    $('.table-search').each(function () {
        const column = $(this).data('column');
        const searchText = $(this).val().trim();
        searchValues[column] = normalizeSearchText(searchText);
    });

    $('.yarn-item-row').each(function () {
        const $row = $(this);
        const isAvailable = $row.data('available');
        let showRow = true;

        // Check available filter
        if (availableOnly && !isAvailable) {
            showRow = false;
        }

        // Check each search field
        if (showRow) {
            for (const [column, normalizedSearch] of Object.entries(searchValues)) {
                if (normalizedSearch) {
                    let cellValue = '';
                    let cellDataValue = '';

                    switch (column) {
                        case 'yarnItemName':
                            cellValue = $row.find('.yarn-item-name').text().trim();
                            break;
                        case 'originYarnName':
                            cellValue = $row.find('.origin-yarn-name').text().trim();
                            break;
                        case 'manufacturerNames':
                            cellValue = $row.find('.manufacturer-names').text().trim();
                            break;
                        case 'quantityBalance':
                            cellValue = $row.find('.quantity-balance').text().trim();
                            cellDataValue = $row.data('quantity-balance') || '';
                            break;
                        case 'countBalance':
                            cellValue = $row.find('.count-balance-cell').text().trim();
                            cellDataValue = $row.data('count-balance') || '';
                            break;
                        case 'status':
                            cellValue = $row.find('.status-badge').text().trim();
                            break;
                        case 'totalTransactions':
                            cellValue = $row.find('.total-transactions').text().trim();
                            cellDataValue = $row.data('total-transactions') || '';
                            break;
                        case 'lastTransactionDate':
                            cellValue = $row.find('.last-transaction-date').text().trim();
                            break;
                    }

                    // Normalize cell value for comparison (convert both Arabic and Latin to Latin)
                    const normalizedCellValue = normalizeSearchText(cellValue);
                    const normalizedCellDataValue = normalizeSearchText(cellDataValue.toString());

                    // Search in both displayed text and data attributes
                    const matchesDisplay = normalizedCellValue.includes(normalizedSearch);
                    const matchesData = normalizedCellDataValue.includes(normalizedSearch);

                    if (!matchesDisplay && !matchesData) {
                        showRow = false;
                        break;
                    }
                }
            }
        }

        $row.toggle(showRow);
    });
}

// Table Sorting Functionality
function initializeTableSorting() {
    $('th[data-sort]').on('click', function (e) {
        if ($(e.target).hasClass('table-search')) {
            return;
        }

        const column = $(this).data('sort');
        const $icon = $(this).find('i.fa-sort, i.fa-sort-up, i.fa-sort-down');

        $('th[data-sort] i.fa-sort, th[data-sort] i.fa-sort-up, th[data-sort] i.fa-sort-down')
            .removeClass('fa-sort-up fa-sort-down').addClass('fa-sort');

        if (currentSortColumn === column) {
            currentSortDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortColumn = column;
            currentSortDirection = 'asc';
        }

        $icon.removeClass('fa-sort').addClass(currentSortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down');

        sortTable(column, currentSortDirection);
    });
}

function sortTable(column, direction) {
    const $table = $('#overviewTable');
    const $rows = $table.find('tbody tr').get();

    $rows.sort(function (a, b) {
        let aValue, bValue;

        switch (column) {
            case 'yarnItemName':
                aValue = $(a).find('.yarn-item-name').text().trim();
                bValue = $(b).find('.yarn-item-name').text().trim();
                break;
            case 'originYarnName':
                aValue = $(a).find('.origin-yarn-name').text().trim();
                bValue = $(b).find('.origin-yarn-name').text().trim();
                break;
            case 'manufacturerNames':
                aValue = $(a).find('.manufacturer-names').text().trim();
                bValue = $(b).find('.manufacturer-names').text().trim();
                break;
            case 'quantityBalance':
                aValue = parseFloat($(a).data('quantity-balance')) || 0;
                bValue = parseFloat($(b).data('quantity-balance')) || 0;
                break;
            case 'countBalance':
                aValue = parseFloat($(a).data('count-balance')) || 0;
                bValue = parseFloat($(b).data('count-balance')) || 0;
                break;
            case 'status':
                aValue = $(a).data('status');
                bValue = $(b).data('status');
                break;
            case 'totalTransactions':
                aValue = parseInt($(a).data('total-transactions')) || 0;
                bValue = parseInt($(b).data('total-transactions')) || 0;
                break;
            case 'lastTransactionDate':
                aValue = $(a).data('last-transaction-date') || '';
                bValue = $(b).data('last-transaction-date') || '';
                break;
            default:
                aValue = $(a).find('td').eq(0).text().trim();
                bValue = $(b).find('td').eq(0).text().trim();
        }

        if (direction === 'asc') {
            return aValue > bValue ? 1 : aValue < bValue ? -1 : 0;
        } else {
            return aValue < bValue ? 1 : aValue > bValue ? -1 : 0;
        }
    });

    $.each($rows, function (index, row) {
        $table.find('tbody').append(row);
    });
}

function toggleAvailableFilter() {
    filterTable();
}

// Packaging Breakdown Functions
function loadAllPackagingBreakdowns() {
    $('.count-balance-cell').each(function () {
        const $cell = $(this);
        const yarnItemId = $cell.data('yarn-id');
        loadPackagingBreakdown(yarnItemId, $cell);
    });
}

function loadPackagingBreakdown(yarnItemId, $targetCell) {
    $.ajax({
        url: '/YarnTransactions/GetYarnItemBalance',
        type: 'GET',
        data: { yarnItemId: yarnItemId },
        success: function (response) {
            if (response && response.success) {
                renderPackagingBreakdown(response, $targetCell);
            } else {
                $targetCell.html('<div class="text-danger small">خطأ في التحميل</div>');
            }
        },
        error: function () {
            $targetCell.html('<div class="text-danger small">خطأ في الاتصال</div>');
        }
    });
}

function renderPackagingBreakdown(data, $targetCell) {
    if (!data.packagingBreakdown || data.packagingBreakdown.length === 0) {
        $targetCell.html(`
            <div class="text-center text-muted">
                <small>لا توجد تعبئة</small>
            </div>
        `);
        return;
    }

    let html = '<div class="packaging-breakdown">';

    html += `
        <div class="balance-summary mb-2 p-2 bg-light rounded">
            <div class="row text-center small">
                <div class="col-6">
                    <div class="fw-bold">${toArabicDigits(data.totalQuantityBalance.toFixed(2))}</div>
                    <div class="text-muted">كجم إجمالي</div>
                </div>
                <div class="col-6">
                    <div class="fw-bold">${toArabicDigits(data.totalCountBalance)}</div>
                    <div class="text-muted">وحدة إجمالي</div>
                </div>
            </div>
        </div>
    `;

    data.packagingBreakdown.forEach(pkg => {
        const bgClass = pkg.totalCount > 50 ? 'bg-success' :
            pkg.totalCount > 20 ? 'bg-secondary' : 'bg-light text-dark border';

        html += `
            <div class="packaging-item mb-1">
                <span class="badge ${bgClass} me-1">
                    ${toArabicDigits(pkg.totalCount)} ${pkg.packagingType}
                </span>
                <small class="text-success fw-bold">
                    ${toArabicDigits(pkg.specificWeight.toFixed(2))} كجم
                </small>
            </div>
        `;
    });

    html += '</div>';
    $targetCell.html(html);
}

// Transaction Detail Modal
function viewTransactionDetails(transaction) {
    const transactionDate = new Date(transaction.date || transaction.Date);
    const isInbound = transaction.isInbound || transaction.IsInbound;
    const transactionId = transaction.transactionId || transaction.TransactionId;
    const quantity = transaction.quantity || transaction.Quantity || 0;
    const count = transaction.count || transaction.Count || 0;
    const stakeholder = transaction.stakeholderName || transaction.StakeholderName || 'غير محدد';
    const stakeholderType = transaction.stakeholderType || transaction.StakeholderType || 'غير محدد';
    const balance = transaction.quantityBalance || transaction.QuantityBalance || 0;
    const comment = transaction.comment || transaction.Comment || 'لا توجد ملاحظات';
    const createdBy = transaction.createdBy || transaction.CreatedBy || 'غير معروف';

    const modalHtml = `
        <div class="modal fade" id="transactionDetailModal" tabindex="-1" aria-labelledby="transactionDetailModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header bg-primary text-white">
                        <h5 class="modal-title" id="transactionDetailModalLabel">
                            <i class="fas fa-info-circle me-2"></i>
                            تفاصيل المعاملة
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label class="fw-bold text-primary">رقم المعاملة:</label>
                                    <p class="ms-2">${toArabicDigits(transactionId)}</p>
                                </div>
                                <div class="mb-3">
                                    <label class="fw-bold text-primary">التاريخ:</label>
                                    <p class="ms-2">${toArabicDigits(transactionDate.toLocaleDateString('ar-EG'))}</p>
                                </div>
                                <div class="mb-3">
                                    <label class="fw-bold text-primary">النوع:</label>
                                    <p class="ms-2">
                                        <span class="badge ${isInbound ? 'bg-success' : 'bg-danger'}">
                                            ${isInbound ? 'وارد' : 'صادر'}
                                        </span>
                                    </p>
                                </div>
                                <div class="mb-3">
                                    <label class="fw-bold text-primary">الكمية:</label>
                                    <p class="ms-2 ${isInbound ? 'text-success' : 'text-danger'} fw-bold">
                                        ${toArabicDigits(quantity.toFixed(3))} كجم
                                    </p>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label class="fw-bold text-primary">العدد:</label>
                                    <p class="ms-2 fw-bold">${toArabicDigits(count)} وحدة</p>
                                </div>
                                <div class="mb-3">
                                    <label class="fw-bold text-primary">التاجر:</label>
                                    <p class="ms-2">${stakeholder}</p>
                                </div>
                                <div class="mb-3">
                                    <label class="fw-bold text-primary">نوع التاجر:</label>
                                    <p class="ms-2">${stakeholderType}</p>
                                </div>
                                <div class="mb-3">
                                    <label class="fw-bold text-primary">رصيد الكمية:</label>
                                    <p class="ms-2 fw-bold text-primary">${toArabicDigits(balance.toFixed(3))} كجم</p>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-12">
                                <div class="mb-3">
                                    <label class="fw-bold text-primary">ملاحظات:</label>
                                    <p class="ms-2 bg-light p-3 rounded">${comment}</p>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-12">
                                <div class="text-muted small">
                                    <i class="fas fa-user me-1"></i>
                                    تم الإنشاء بواسطة: ${createdBy}
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            <i class="fas fa-times me-1"></i>
                            إغلاق
                        </button>
                        
                    </div>
                </div>
            </div>
        </div>
    `;

    $('#transactionDetailModal').remove();
    $('body').append(modalHtml);
    const transactionModal = new bootstrap.Modal(document.getElementById('transactionDetailModal'));
    transactionModal.show();

    $('#transactionDetailModal').on('hidden.bs.modal', function () {
        $(this).remove();
    });
}

// Modal Functions
function viewItemDetails(yarnItemId, yarnItemName) {
    currentYarnItemId = yarnItemId;
    currentPage = 1;

    $('#itemDetailsModalLabel').html(`<i class="fas fa-info-circle me-2"></i>تفاصيل صنف الغزل: ${yarnItemName}`);
    loadItemDetails();
    $('#itemDetailsModal').modal('show');
}

function loadItemDetails() {
    $('#modalBody').html(`
        <div class="text-center py-4">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">جاري التحميل...</span>
            </div>
        </div>
    `);

    $.ajax({
        url: '/YarnTransactions/ItemDetails',
        type: 'GET',
        data: {
            id: currentYarnItemId,
            page: currentPage,
            pageSize: 1000
        },
        success: function (response) {
            if (response && response.success) {
                currentTransactions = response.transactions || [];
                renderItemDetails(response);
            } else {
                $('#modalBody').html('<div class="alert alert-danger">خطأ في تحميل البيانات</div>');
            }
        },
        error: function () {
            $('#modalBody').html('<div class="alert alert-danger">خطأ في الاتصال</div>');
        }
    });
}

// Enhanced Transaction Inline Search with Arabic Number Support
function initializeTransactionInlineSearch() {
    // Remove any existing event handlers to prevent duplicates
    $('.transaction-search').off('input keyup paste');

    $('.transaction-search').on('input keyup paste', function () {
        const column = $(this).data('column');
        
        // Convert input to Arabic digits in real-time
        convertInputToArabic(this);
        
        let searchText = $(this).val().trim();
        const normalizedSearch = normalizeSearchText(searchText);

        $('#transactionsTable tbody tr').each(function () {
            const $row = $(this);
            let cellValue = '';
            let cellDataValue = '';

            switch (column) {
                case 'date':
                    cellValue = $row.find('td').eq(0).find('.fw-bold').text().trim();
                    cellDataValue = $row.data('date') || '';
                    break;
                case 'type':
                    cellValue = $row.find('td').eq(1).find('.badge').text().trim();
                    cellDataValue = $row.data('type') || '';
                    break;
                case 'quantity':
                    cellValue = $row.find('td').eq(2).text().trim();
                    cellDataValue = $row.data('quantity') || '';
                    break;
                case 'count':
                    cellValue = $row.find('td').eq(3).text().trim();
                    cellDataValue = $row.data('count') || '';
                    break;
                case 'stakeholder':
                    const stakeholderName = $row.find('td').eq(4).find('.fw-bold').text().trim();
                    const stakeholderType = $row.find('td').eq(4).find('.text-muted').text().trim();
                    cellValue = stakeholderName + ' ' + stakeholderType;
                    cellDataValue = $row.data('stakeholder') || '';
                    break;
                case 'balance':
                    cellValue = $row.find('td').eq(5).text().trim();
                    cellDataValue = $row.data('balance') || '';
                    break;
                case 'comment':
                    cellValue = $row.find('td').eq(6).text().trim();
                    cellDataValue = $row.data('comment') || '';
                    break;
            }

            // Normalize for comparison (convert both Arabic and Latin to Latin)
            const normalizedCellValue = normalizeSearchText(cellValue);
            const normalizedCellDataValue = normalizeSearchText(cellDataValue.toString());

            if (normalizedSearch &&
                !normalizedCellValue.includes(normalizedSearch) &&
                !normalizedCellDataValue.includes(normalizedSearch)) {
                $row.hide();
            } else {
                $row.show();
            }
        });

        // Update search result count
        updateTransactionSearchResultCount();
    });

    // Convert search placeholders to Arabic
    $('.transaction-search').each(function () {
        const placeholder = $(this).attr('placeholder');
        if (placeholder) {
            $(this).attr('placeholder', toArabicDigits(placeholder));
        }
    });

    // Handle focus events for Arabic conversion
    $('.transaction-search').on('focus', function () {
        const $this = $(this);
        setTimeout(() => {
            if ($this.val()) {
                convertInputToArabic(this);
            }
        }, 10);
    });

    // Clear search on modal close
    $('#itemDetailsModal').on('hidden.bs.modal', function () {
        $('.transaction-search').val('').trigger('input');
    });
}

// Update transaction search result count
function updateTransactionSearchResultCount() {
    const visibleRows = $('#transactionsTable tbody tr:visible').length;
    const totalRows = $('#transactionsTable tbody tr').length;

    // Add or update search result indicator
    let $indicator = $('.transaction-search-result-indicator');
    if ($indicator.length === 0) {
        $indicator = $('<div class="transaction-search-result-indicator alert alert-info py-2 mb-2 text-center small"></div>');
        $('#transactionsTable').parent().before($indicator);
    }

    if (visibleRows < totalRows) {
        $indicator.html(`
            <i class="fas fa-filter me-2"></i>
            عرض ${toArabicDigits(visibleRows)} من ${toArabicDigits(totalRows)} معاملة
            <button class="btn btn-sm btn-outline-secondary ms-2" onclick="clearAllTransactionSearches()">
                <i class="fas fa-times me-1"></i>إلغاء البحث
            </button>
        `).show();
    } else {
        $indicator.hide();
    }
}

// Clear all transaction searches
function clearAllTransactionSearches() {
    $('.transaction-search').val('').trigger('input');
    $('.transaction-search-result-indicator').hide();
}

// Transaction Sorting Functionality
function initializeTransactionSorting() {
    // Remove existing event handlers
    $('#transactionsTable th[data-sort]').off('click');

    $('#transactionsTable th[data-sort]').on('click', function (e) {
        if ($(e.target).hasClass('transaction-search') || $(e.target).closest('.transaction-search').length) {
            return;
        }

        const column = $(this).data('sort');
        const $icon = $(this).find('i.fa-sort, i.fa-sort-up, i.fa-sort-down');

        // Reset all icons
        $('#transactionsTable th[data-sort] i.fa-sort, #transactionsTable th[data-sort] i.fa-sort-up, #transactionsTable th[data-sort] i.fa-sort-down')
            .removeClass('fa-sort-up fa-sort-down').addClass('fa-sort');

        if (transactionSortColumn === column) {
            transactionSortDirection = transactionSortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            transactionSortColumn = column;
            transactionSortDirection = 'asc';
        }

        $icon.removeClass('fa-sort').addClass(transactionSortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down');

        sortTransactions(column, transactionSortDirection);
    });
}

function sortTransactions(column, direction) {
    const $table = $('#transactionsTable');
    const $rows = $table.find('tbody tr').get();

    $rows.sort(function (a, b) {
        let aValue, bValue;

        switch (column) {
            case 'date':
                aValue = new Date($(a).data('date'));
                bValue = new Date($(b).data('date'));
                break;
            case 'type':
                aValue = $(a).data('type');
                bValue = $(b).data('type');
                break;
            case 'quantity':
                aValue = parseFloat($(a).data('quantity')) || 0;
                bValue = parseFloat($(b).data('quantity')) || 0;
                break;
            case 'count':
                aValue = parseInt($(a).data('count')) || 0;
                bValue = parseInt($(b).data('count')) || 0;
                break;
            case 'stakeholder':
                aValue = $(a).data('stakeholder');
                bValue = $(b).data('stakeholder');
                break;
            case 'balance':
                aValue = parseFloat($(a).data('balance')) || 0;
                bValue = parseFloat($(b).data('balance')) || 0;
                break;
            case 'comment':
                aValue = $(a).data('comment');
                bValue = $(b).data('comment');
                break;
            default:
                aValue = $(a).find('td').eq(0).text().trim();
                bValue = $(b).find('td').eq(0).text().trim();
        }

        if (direction === 'asc') {
            return aValue > bValue ? 1 : aValue < bValue ? -1 : 0;
        } else {
            return aValue < bValue ? 1 : aValue > bValue ? -1 : 0;
        }
    });

    $.each($rows, function (index, row) {
        $table.find('tbody').append(row);
    });
}

function renderItemDetails(data) {
    const yarnItem = data.yarnItem;
    const transactions = data.transactions || [];

    let html = `
        <div class="row mb-4">
            <div class="col-12">
                <div class="card border">
                    <div class="card-header bg-light">
                        <h6 class="mb-0"><i class="fas fa-info-circle me-2"></i>معلومات الصنف</h6>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label class="fw-bold">اسم الصنف:</label>
                                    <span class="ms-2">${yarnItem.itemName}</span>
                                </div>
                                <div class="mb-3">
                                    <label class="fw-bold">الغزل الأصلي:</label>
                                    <span class="ms-2">${yarnItem.originYarnName || 'غير محدد'}</span>
                                </div>
                                <div class="mb-3">
                                    <label class="fw-bold">الشركة المصنعة:</label>
                                    <span class="ms-2">${yarnItem.manufacturerNames || 'غير محدد'}</span>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="row text-center">
                                    <div class="col-6">
                                        <h4 class="mb-1 ${yarnItem.quantityBalance >= 0 ? 'text-success' : 'text-danger'}">
                                            ${toArabicDigits(yarnItem.quantityBalance.toFixed(3))}
                                        </h4>
                                        <small class="text-muted">رصيد الكمية (كجم)</small>
                                    </div>
                                    <div class="col-6">
                                        <h4 class="mb-1 ${yarnItem.countBalance >= 0 ? 'text-success' : 'text-danger'}">
                                            ${toArabicDigits(yarnItem.countBalance)}
                                        </h4>
                                        <small class="text-muted">رصيد العدد</small>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;

    if (transactions.length > 0) {
        html += `
            <div class="row">
                <div class="col-12">
                    <div class="card border">
                        <div class="card-header bg-light d-flex justify-content-between align-items-center">
                            <h6 class="mb-0">
                                <i class="fas fa-history me-2"></i>المعاملات 
                                <span class="badge bg-primary ms-2">${toArabicDigits(transactions.length)}</span>
                            </h6>
                            <div>
                                <button class="btn btn-sm btn-outline-primary me-2" onclick="exportTransactionsToExcel()">
                                    <i class="fas fa-file-excel me-1"></i>تصدير إكسل
                                </button>
                                <button class="btn btn-sm btn-outline-secondary" onclick="clearAllTransactionSearches()">
                                    <i class="fas fa-eraser me-1"></i>مسح البحث
                                </button>
                            </div>
                        </div>
                        <div class="card-body p-0">
                            <div class="table-responsive">
                                <table class="table table-hover table-sm mb-0" id="transactionsTable">
                                    <thead class="table-light">
                                        <tr>
                                            <th data-sort="date">
                                                <div class="d-flex flex-column">
                                                    <div class="d-flex align-items-center mb-1">
                                                        <span>التاريخ</span>
                                                        <i class="fas fa-sort ms-1"></i>
                                                    </div>
                                                    <input type="text" class="form-control form-control-sm transaction-search"
                                                           placeholder="بحث..." data-column="date">
                                                </div>
                                            </th>
                                            <th data-sort="type">
                                                <div class="d-flex flex-column">
                                                    <div class="d-flex align-items-center mb-1">
                                                        <span>النوع</span>
                                                        <i class="fas fa-sort ms-1"></i>
                                                    </div>
                                                    <input type="text" class="form-control form-control-sm transaction-search"
                                                           placeholder="بحث..." data-column="type">
                                                </div>
                                            </th>
                                            <th data-sort="quantity">
                                                <div class="d-flex flex-column">
                                                    <div class="d-flex align-items-center mb-1">
                                                        <span>الكمية</span>
                                                        <i class="fas fa-sort ms-1"></i>
                                                    </div>
                                                    <input type="text" class="form-control form-control-sm transaction-search"
                                                           placeholder="بحث..." data-column="quantity">
                                                </div>
                                            </th>
                                            <th data-sort="count">
                                                <div class="d-flex flex-column">
                                                    <div class="d-flex align-items-center mb-1">
                                                        <span>العدد</span>
                                                        <i class="fas fa-sort ms-1"></i>
                                                    </div>
                                                    <input type="text" class="form-control form-control-sm transaction-search"
                                                           placeholder="بحث..." data-column="count">
                                                </div>
                                            </th>
                                            <th data-sort="stakeholder">
                                                <div class="d-flex flex-column">
                                                    <div class="d-flex align-items-center mb-1">
                                                        <span>التاجر ونوعه</span>
                                                        <i class="fas fa-sort ms-1"></i>
                                                    </div>
                                                    <input type="text" class="form-control form-control-sm transaction-search"
                                                           placeholder="بحث..." data-column="stakeholder">
                                                </div>
                                            </th>
                                            <th data-sort="balance">
                                                <div class="d-flex flex-column">
                                                    <div class="d-flex align-items-center mb-1">
                                                        <span>رصيد الكمية</span>
                                                        <i class="fas fa-sort ms-1"></i>
                                                    </div>
                                                    <input type="text" class="form-control form-control-sm transaction-search"
                                                           placeholder="بحث..." data-column="balance">
                                                </div>
                                            </th>
                                            <th data-sort="comment">
                                                <div class="d-flex flex-column">
                                                    <div class="d-flex align-items-center mb-1">
                                                        <span>ملاحظات</span>
                                                        <i class="fas fa-sort ms-1"></i>
                                                    </div>
                                                    <input type="text" class="form-control form-control-sm transaction-search"
                                                           placeholder="بحث..." data-column="comment">
                                                </div>
                                            </th>
                                            <th>إجراءات</th>
                                        </tr>
                                    </thead>
                                    <tbody>
        `;

        transactions.forEach(transaction => {
            const transactionDate = new Date(transaction.date || transaction.Date);
            const isToday = new Date().toDateString() === transactionDate.toDateString();
            const transactionId = transaction.transactionId || transaction.TransactionId;
            const isInbound = transaction.isInbound || transaction.IsInbound;
            const quantity = transaction.quantity || transaction.Quantity || 0;
            const count = transaction.count || transaction.Count || 0;
            const stakeholder = transaction.stakeholderName || transaction.StakeholderName || 'غير محدد';
            const stakeholderType = transaction.stakeholderType || transaction.StakeholderType || 'غير محدد';
            const balance = transaction.quantityBalance || transaction.QuantityBalance || 0;
            const comment = transaction.comment || transaction.Comment || '-';

            html += `
                <tr class="transaction-row" 
                    data-transaction-id="${transactionId}"
                    data-date="${transactionDate.toISOString()}"
                    data-type="${isInbound ? 'وارد' : 'صادر'}"
                    data-quantity="${quantity}"
                    data-count="${count}"
                    data-stakeholder="${stakeholder} - ${stakeholderType}"
                    data-balance="${balance}"
                    data-comment="${comment}">
                    <td>
                        <div class="fw-bold small">${toArabicDigits(transactionDate.toLocaleDateString('ar-EG'))}</div>
                        <small class="text-muted">${isToday ? 'اليوم' : ''}</small>
                    </td>
                    <td>
                        <span class="badge rounded-pill ${isInbound ? 'bg-success' : 'bg-danger'} small">
                            ${isInbound ? 'وارد' : 'صادر'}
                        </span>
                    </td>
                    <td class="${isInbound ? 'text-success' : 'text-danger'} fw-bold small">
                        ${toArabicDigits(quantity.toFixed(3))}
                    </td>
                    <td class="fw-bold small">${toArabicDigits(count)}</td>
                    <td>
                        <div class="fw-bold small">${stakeholder}</div>
                        <small class="text-muted">${stakeholderType}</small>
                    </td>
                    <td class="fw-bold small">${toArabicDigits(balance.toFixed(3))}</td>
                    <td class="small">${comment}</td>
                    <td>
                        <div class="btn-group btn-group-sm">
                            <button type="button" class="btn btn-outline-info btn-sm" title="عرض التفاصيل"
                                    onclick='viewTransactionDetails(${JSON.stringify(transaction).replace(/'/g, "\\'")})'> 
                                <i class="fas fa-eye"></i>
                            </button>
                            <a href="/YarnTransactions/Edit/${transaction.id}" class="btn btn-outline-secondary btn-sm" title="تعديل المعاملة">
                                <i class="fas fa-edit"></i>
                            </a>
                        </div>
                    </td>
                </tr>
            `;
        });

        html += `
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    } else {
        html += `
            <div class="row">
                <div class="col-12">
                    <div class="alert alert-info text-center py-4">
                        <i class="fas fa-info-circle fa-2x mb-3"></i>
                        <h6 class="mb-2">لا توجد معاملات</h6>
                        <p class="mb-0 text-muted small">لم يتم تسجيل أي معاملات لهذا الصنف حتى الآن.</p>
                    </div>
                </div>
            </div>
        `;
    }

    $('#modalBody').html(html);

    // Initialize transaction inline search and sorting
    initializeTransactionInlineSearch();
    initializeTransactionSorting();
}

function seeMoreTransactions() {
    currentPage++;
    loadItemDetails();
}

// Export Functions
function exportToExcel() {
    showLoadingOverlay();
    try {
        const data = collectOverviewData();
        exportToExcelFile(data, 'نظرة_عامة_أرصدة_الغزل');
    } catch (error) {
        alert('حدث خطأ أثناء التصدير: ' + error.message);
    } finally {
        setTimeout(hideLoadingOverlay, 1000);
    }
}

function exportTransactionsToExcel() {
    if (currentTransactions.length === 0) {
        alert('لا توجد معاملات للتصدير');
        return;
    }

    showLoadingOverlay();
    try {
        const data = collectTransactionsData();
        exportToExcelFile(data, 'معاملات_الصنف');
    } catch (error) {
        alert('حدث خطأ أثناء تصدير المعاملات: ' + error.message);
    } finally {
        setTimeout(hideLoadingOverlay, 1000);
    }
}

function collectOverviewData() {
    const headers = [
        'صنف الغزل', 'الغزل الأصلي', 'الشركة المصنعة', 'الكمية (كجم)',
        'رصيد العدد', 'الحالة', 'إجمالي المعاملات', 'آخر معاملة'
    ];

    const data = [headers];

    $('.yarn-item-row:visible').each(function () {
        const $row = $(this);
        const rowData = [
            $row.find('.yarn-item-name').text().trim(),
            $row.find('.origin-yarn-name').text().trim(),
            $row.find('.manufacturer-names').text().trim(),
            $row.find('.quantity-balance').text().trim(),
            $row.find('.count-balance-cell').text().trim(),
            $row.find('.status-badge').text().trim(),
            $row.find('.total-transactions').text().trim(),
            $row.find('.last-transaction-date').text().trim()
        ];
        data.push(rowData);
    });

    return data;
}

function collectTransactionsData() {
    const headers = [
        'التاريخ', 'النوع', 'الكمية (كجم)',
        'العدد', 'التاجر', 'نوع التاجر', 'رصيد الكمية', 'ملاحظات'
    ];

    const data = [headers];

    currentTransactions.forEach(transaction => {
        const transactionDate = new Date(transaction.date || transaction.Date);
        const isInbound = transaction.isInbound || transaction.IsInbound;
        const stakeholderType = transaction.stakeholderType || transaction.StakeholderType || 'غير محدد';

        const rowData = [
            transactionDate.toLocaleDateString('ar-EG'),
            isInbound ? 'وارد' : 'صادر',
            (transaction.quantity || transaction.Quantity || 0).toFixed(3),
            transaction.count || transaction.Count || 0,
            transaction.stakeholderName || transaction.StakeholderName || 'غير محدد',
            stakeholderType,
            (transaction.quantityBalance || transaction.QuantityBalance || 0).toFixed(3),
            transaction.comment || transaction.Comment || '-'
        ];
        data.push(rowData);
    });

    return data;
}

function exportToExcelFile(data, fileName) {
    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.aoa_to_sheet(data);

    if (!ws['!views']) ws['!views'] = [];
    ws['!views'].push({
        rightToLeft: true,
        showGridLines: false
    });

    const colWidths = data[0].map(() => ({ wch: 20 }));
    ws['!cols'] = colWidths;

    XLSX.utils.book_append_sheet(wb, ws, 'البيانات');

    const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
    XLSX.writeFile(wb, `${fileName}_${timestamp}.xlsx`);

    setTimeout(() => alert('تم التصدير بنجاح!'), 500);
}


function printTransactions() {
    const transactionsHTML = generatePrintableTransactionsHTML();
    const printWindow = window.open('', '_blank', 'width=1000,height=600');

    printWindow.document.write(`
<!DOCTYPE html>
<html dir="rtl" lang="ar">
<head>
    <meta charset="UTF-8">
    <title>طباعة المعاملات</title>
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet">
    <style>
        body {
            font-family: 'Segoe UI', 'Tahoma', 'Geneva', 'Verdana', sans-serif;
            margin: 20px;
            background: white;
            line-height: 1.6;
        }
        .print-container {
            max-width: 100%;
            margin: 0 auto;
        }
        .print-header {
            text-align: center;
            margin-bottom: 30px;
            border-bottom: 3px solid #333;
            padding-bottom: 15px;
        }
        .print-header h2 {
            color: #2c3e50;
            margin-bottom: 10px;
            font-weight: bold;
        }
        .print-header p {
            color: #7f8c8d;
            margin: 0;
        }
        .card {
            border: 2px solid #34495e !important;
            border-radius: 10px;
            margin-bottom: 25px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        }
        .card-header {
            background: linear-gradient(135deg, #34495e, #2c3e50) !important;
            color: white !important;
            border-bottom: 2px solid #34495e !important;
            padding: 15px 20px;
            font-weight: bold;
            border-radius: 8px 8px 0 0 !important;
        }
        .table {
            width: 100%;
            border-collapse: collapse;
            font-size: 14px;
            margin-bottom: 0;
        }
        .table th {
            background: linear-gradient(135deg, #ecf0f1, #bdc3c7) !important;
            color: #2c3e50 !important;
            border: 2px solid #34495e !important;
            padding: 12px 8px;
            font-weight: bold;
            text-align: center;
        }
        .table td {
            border: 1px solid #bdc3c7 !important;
            padding: 10px 8px;
            text-align: center;
        }
        .badge {
            font-weight: bold;
            padding: 6px 12px;
            border-radius: 20px;
        }
        .bg-success {
            background: linear-gradient(135deg, #27ae60, #2ecc71) !important;
            color: white !important;
        }
        .bg-danger {
            background: linear-gradient(135deg, #e74c3c, #c0392b) !important;
            color: white !important;
        }
        .text-success {color: #27ae60 !important; font-weight: bold;}
        .text-danger {color: #e74c3c !important; font-weight: bold;}
        @media print {
            body {
                margin: 0.5cm;
                font-size: 12px;
            }
            .card {
                box-shadow: none !important;
                border: 2px solid #000 !important;
            }
            .no-print {
                display: none !important;
            }
            .print-header {
                margin-bottom: 20px;
            }
            .table {
                font-size: 11px;
            }
        }
    </style>
</head>
<body>
    <div class="print-container">
        ${transactionsHTML}
    </div>
    <script>
        window.onload = function() {
            window.print();
            setTimeout(function() {
                window.close();
            }, 1000);
        }
    <\/script>
</body>
</html>
    `);
    printWindow.document.close();
}

function generatePrintableTransactionsHTML() {
    if (!currentTransactions.length) {
        return `
            <div class="print-header">
                <h2>تقرير المعاملات</h2>
                <p>لا توجد معاملات للطباعة</p>
            </div>
        `;
    }

    let html = `
        <div class="print-header">
            <h2><i class="fas fa-file-invoice me-2"></i>تقرير المعاملات</h2>
            <p>تاريخ الطباعة: ${toArabicDigits(new Date().toLocaleDateString('ar-EG'))}</p>
        </div>
        <div class="card">
            <div class="card-header">
                <i class="fas fa-history me-2"></i>المعاملات (${toArabicDigits(currentTransactions.length)} معاملة)
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-bordered">
                        <thead>
                            <tr>
                                <th>التاريخ</th>
                                <th>النوع</th>
                                <th>الكمية</th>
                                <th>العدد</th>
                                <th>التاجر</th>
                                <th>نوع التاجر</th>
                                <th>رصيد الكمية</th>
                                <th>ملاحظات</th>
                            </tr>
                        </thead>
                        <tbody>
    `;

    currentTransactions.forEach(transaction => {
        const transactionDate = new Date(transaction.date || transaction.Date);
        const isInbound = transaction.isInbound || transaction.IsInbound;
        const quantity = transaction.quantity || transaction.Quantity || 0;
        const count = transaction.count || transaction.Count || 0;
        const stakeholder = transaction.stakeholderName || transaction.StakeholderName || 'غير محدد';
        const stakeholderType = transaction.stakeholderType || transaction.StakeholderType || 'غير محدد';
        const balance = transaction.quantityBalance || transaction.QuantityBalance || 0;
        const comment = transaction.comment || transaction.Comment || '-';

        html += `
            <tr>
                <td>${toArabicDigits(transactionDate.toLocaleDateString('ar-EG'))}</td>
                <td>
                    <span class="badge ${isInbound ? 'bg-success' : 'bg-danger'}">
                        ${isInbound ? 'وارد' : 'صادر'}
                    </span>
                </td>
                <td class="${isInbound ? 'text-success' : 'text-danger'} fw-bold">
                    ${toArabicDigits(quantity.toFixed(3))}
                </td>
                <td class="fw-bold">${toArabicDigits(count)}</td>
                <td>${stakeholder}</td>
                <td>${stakeholderType}</td>
                <td class="fw-bold">${toArabicDigits(balance.toFixed(3))}</td>
                <td>${comment}</td>
            </tr>
        `;
    });

    html += `
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    `;

    return html;
}

function printOverview() {
    window.print();
}

function shareWhatsApp() {
    const text = `نظرة عامة على أرصدة الغزل\nإجمالي الأصناف: ${$('#totalItemsDisplay').text()}\nالأصناف المتاحة: ${$('#availableItemsDisplay').text()}\nإجمالي الكمية: ${$('#totalQuantityDisplay').text()} كجم\nآخر تحديث: ${$('#lastUpdatedDisplay').text()}`;
    const encodedText = encodeURIComponent(text);
    window.open(`https://wa.me/?text=${encodedText}`, '_blank');
}

function refreshData() {
    showLoadingOverlay();
    const availableOnly = $('#availableOnlyFilter').is(':checked');
    window.location.href = `/YarnItems/Overview?availableOnly=${availableOnly}`;
}

function showLoadingOverlay() {
    $('#loadingOverlay').show();
}

function hideLoadingOverlay() {
    $('#loadingOverlay').hide();
}