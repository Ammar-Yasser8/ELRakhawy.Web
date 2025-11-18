$(document).ready(function () {
    console.log('🔍 Yarn Transaction Search with Arabic Calendar');
    console.log('👤 User: Ammar-Yasser8 | Time: 2025-10-11 12:38:59');

    let currentSortColumn = '';
    let currentSortDirection = 'asc';
    let currentSearchResults = [];

    function toArabicDigits(str) {
        if (!str) return str;
        return str.toString().replace(/[0-9]/g, d => '٠١٢٣٤٥٦٧٨٩'[d]);
    }

    function toLatinDigits(str) {
        if (!str) return str;
        return str.toString().replace(/[٠-٩]/g, d => '0123456789'['٠١٢٣٤٥٦٧٨٩'.indexOf(d)]);
    }

    function normalizeSearchText(text) {
        if (!text) return '';
        return text.toString().replace(/[٠-٩]/g, digit => {
            const arabicDigits = '٠١٢٣٤٥٦٧٨٩';
            const index = arabicDigits.indexOf(digit);
            return index >= 0 ? index : digit;
        }).toLowerCase().trim();
    }

    $('.form-select').select2({
        theme: 'bootstrap-5',
        width: '100%',
        dir: 'rtl',
        language: { noResults: () => "لا توجد نتائج", searching: () => "جاري البحث..." },
        templateResult: function (data) {
            if (!data.id) return data.text;
            var text = data.text;
            if (text && /\d/.test(text)) text = toArabicDigits(text);
            return $('<span>' + text + '</span>');
        },
        templateSelection: function (data) {
            var text = data.text;
            if (text && /\d/.test(text)) text = toArabicDigits(text);
            return text;
        },
        matcher: function (params, data) {
            if ($.trim(params.term) === '') return data;
            if (typeof data.text === 'undefined') return null;
            const normalizeArabic = (str) => { if (!str) return ''; return str.replace(/[أإآ]/g, 'ا').replace(/ى/g, 'ي'); };
            let searchTerm = toLatinDigits(params.term.toLowerCase().trim());
            let optionText = toLatinDigits(data.text.toLowerCase().trim());
            searchTerm = normalizeArabic(searchTerm);
            optionText = normalizeArabic(optionText);
            if (optionText.includes(searchTerm)) return data;
            return null;
        }
    });

    const fromDateInput = document.getElementById('FromDate');
    const fpFrom = flatpickr(fromDateInput, {
        dateFormat: 'Y-m-d',
        locale: { ...flatpickr.l10ns.ar, firstDayOfWeek: 6 },
        allowInput: false,
        disableMobile: true,
        defaultDate: fromDateInput.value || new Date(),
        onChange: function (selectedDates, dateStr) {
            fromDateInput.setAttribute('data-latin-date', dateStr);
            fromDateInput.value = toArabicDigits(dateStr);
            updateCalendarArabic();
        },
        onReady: function (selectedDates, dateStr) {
            if (dateStr) {
                fromDateInput.setAttribute('data-latin-date', dateStr);
                fromDateInput.value = toArabicDigits(dateStr);
            }
            updateCalendarArabic();
        },
        onMonthChange: updateCalendarArabic,
        onYearChange: updateCalendarArabic
    });

    const toDateInput = document.getElementById('ToDate');
    const fpTo = flatpickr(toDateInput, {
        dateFormat: 'Y-m-d',
        locale: { ...flatpickr.l10ns.ar, firstDayOfWeek: 6 },
        allowInput: false,
        disableMobile: true,
        defaultDate: toDateInput.value || new Date(),
        onChange: function (selectedDates, dateStr) {
            toDateInput.setAttribute('data-latin-date', dateStr);
            toDateInput.value = toArabicDigits(dateStr);
            updateCalendarArabic();
        },
        onReady: function (selectedDates, dateStr) {
            if (dateStr) {
                toDateInput.setAttribute('data-latin-date', dateStr);
                toDateInput.value = toArabicDigits(dateStr);
            }
            updateCalendarArabic();
        },
        onMonthChange: updateCalendarArabic,
        onYearChange: updateCalendarArabic
    });

    function updateCalendarArabic() {
        setTimeout(() => {
            $('.flatpickr-day').each(function () {
                const text = $(this).text();
                if (text && /\d/.test(text)) $(this).text(toArabicDigits(text));
            });
        }, 0);
    }

    $('#fromCalendarBtn').on('click', () => fpFrom.open());
    $('#toCalendarBtn').on('click', () => fpTo.open());

    if (fromDateInput.value) {
        const latinDate = fromDateInput.value;
        fromDateInput.setAttribute('data-latin-date', latinDate);
        fromDateInput.value = toArabicDigits(latinDate);
    }
    if (toDateInput.value) {
        const latinDate = toDateInput.value;
        toDateInput.setAttribute('data-latin-date', latinDate);
        toDateInput.value = toArabicDigits(latinDate);
    }

    $(document).on('click', '.btn-clear', function () {
        const targetId = $(this).data('target');
        $('#' + targetId).val('').trigger('change').focus();
    });

    $(document).on('input change', '#InternalId, #ExternalId', function () {
        const $clearBtn = $(this).siblings('.btn-clear');
        if ($(this).val().trim()) $clearBtn.show();
        else $clearBtn.hide();
    });

    $('#InternalId, #ExternalId').each(function () {
        const $clearBtn = $(this).siblings('.btn-clear');
        if (!$(this).val().trim()) $clearBtn.hide();
    });

    $('#searchForm').on('submit', function () {
        const fromLatin = $('#FromDate').attr('data-latin-date') || toLatinDigits($('#FromDate').val());
        const toLatin = $('#ToDate').attr('data-latin-date') || toLatinDigits($('#ToDate').val());
        $('#FromDate').val(fromLatin);
        $('#ToDate').val(toLatin);
        $('#InternalId, #ExternalId').each(function () {
            const latinValue = toLatinDigits($(this).val());
            $(this).val(latinValue);
        });
    });

    function convertDisplayToArabic() {
        $('.arabic-display').each(function () {
            if (!$(this).is('input, textarea, select')) {
                const originalText = $(this).text();
                const arabicText = toArabicDigits(originalText);
                $(this).text(arabicText);
            }
        });
        $('.arabic-cell').each(function () {
            const originalText = $(this).text();
            const arabicText = toArabicDigits(originalText);
            $(this).text(arabicText);
        });
    }

    window.setToday = function () {
        const today = new Date().toISOString().split('T')[0];
        fromDateInput.setAttribute('data-latin-date', today);
        toDateInput.setAttribute('data-latin-date', today);
        fromDateInput.value = toArabicDigits(today);
        toDateInput.value = toArabicDigits(today);
        fpFrom.setDate(today);
        fpTo.setDate(today);
    };

    window.setThisWeek = function () {
        const today = new Date();
        const firstDay = new Date(today.setDate(today.getDate() - today.getDay()));
        const lastDay = new Date();
        const firstStr = firstDay.toISOString().split('T')[0];
        const lastStr = lastDay.toISOString().split('T')[0];
        fromDateInput.setAttribute('data-latin-date', firstStr);
        toDateInput.setAttribute('data-latin-date', lastStr);
        fromDateInput.value = toArabicDigits(firstStr);
        toDateInput.value = toArabicDigits(lastStr);
        fpFrom.setDate(firstStr);
        fpTo.setDate(lastStr);
    };

    window.setThisMonth = function () {
        const today = new Date();
        const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
        const lastDay = new Date();
        const firstStr = firstDay.toISOString().split('T')[0];
        const lastStr = lastDay.toISOString().split('T')[0];
        fromDateInput.setAttribute('data-latin-date', firstStr);
        toDateInput.setAttribute('data-latin-date', lastStr);
        fromDateInput.value = toArabicDigits(firstStr);
        toDateInput.value = toArabicDigits(lastStr);
        fpFrom.setDate(firstStr);
        fpTo.setDate(lastStr);
    };

    window.resetForm = function () {
        $('#searchForm')[0].reset();
        $('.form-select').val('').trigger('change');
        const today = new Date();
        const thirtyDaysAgo = new Date();
        thirtyDaysAgo.setDate(today.getDate() - 30);
        const todayStr = today.toISOString().split('T')[0];
        const thirtyStr = thirtyDaysAgo.toISOString().split('T')[0];
        fromDateInput.setAttribute('data-latin-date', thirtyStr);
        toDateInput.setAttribute('data-latin-date', todayStr);
        fromDateInput.value = toArabicDigits(thirtyStr);
        toDateInput.value = toArabicDigits(todayStr);
        fpFrom.setDate(thirtyStr);
        fpTo.setDate(todayStr);
        $('.btn-clear').hide();
    };

    function initializeTableFunctionality() {
        if ($('.table tbody tr').length > 0) {
            currentSearchResults = $('.table tbody tr').get();
            addTableControls();
            initializeInlineSearch();
            initializeTableSorting();
            convertTableNumbersToArabic();
            addActionButtons();
        }
    }

    function addActionButtons() {
        $('.table tbody tr').each(function () {
            const $row = $(this);
            const $actionsCell = $row.find('td').last();
            if ($actionsCell.find('.btn-action-group').length) return;
            const transactionId = $row.data('id') || $row.data('transaction-id') || extractTransactionId($row);
            const actionsHtml = `<div class="btn-action-group btn-group btn-group-sm"><button type="button" class="btn btn-info btn-sm btn-view" data-transaction-id="${transactionId}" title="عرض التفاصيل"><i class="fas fa-eye"></i></button><a href="/YarnTransactions/Edit/${transactionId}" class="btn btn-warning btn-sm" title="تعديل"><i class="fas fa-edit"></i></a><button type="button" class="btn btn-primary btn-sm btn-print" data-transaction-id="${transactionId}" title="طباعة"><i class="fas fa-print"></i></button></div>`;
            $actionsCell.html(actionsHtml);
        });
        initializeActionButtons();
    }

    function extractTransactionId($row) {
        // First priority: get real ID from data attribute
        let transactionId = $row.data('id') || $row.data('transaction-id');

        if (!transactionId) {
            // Second priority: extract from edit link if exists
            const editLink = $row.find('a[href*="/Edit/"]').attr('href');
            if (editLink) {
                const match = editLink.match(/\/Edit\/(\d+)/);
                if (match) transactionId = match[1];
            }
        }

        if (!transactionId) {
            // Third priority: try to find hidden ID field in row
            const hiddenId = $row.find('input[type="hidden"][name*="Id"]').val();
            if (hiddenId) transactionId = hiddenId;
        }

        if (!transactionId) {
            // Last resort: extract from first cell but this might be InternalId
            console.warn('⚠️ Could not find real transaction ID, using fallback method');
            const firstCellText = $row.find('td').first().text();
            const latinText = toLatinDigits(firstCellText);
            const match = latinText.match(/\d+/);
            if (match) transactionId = match[0];
        }

        return transactionId || '0';
    }

    function initializeActionButtons() {
        $(document).off('click', '.btn-view').on('click', '.btn-view', function () {
            const transactionId = $(this).data('transaction-id');
            const $row = $(this).closest('tr');
            viewTransactionDetails(transactionId, $row);
        });
        $(document).off('click', '.btn-print').on('click', '.btn-print', function () {
            const transactionId = $(this).data('transaction-id');
            const $row = $(this).closest('tr');
            printTransaction(transactionId, $row);
        });
    }

    window.viewTransactionDetails = function (transactionId, $row) {
        showLoadingOverlay();
        const transactionData = {
            id: transactionId,
            internalId: $row.find('td').eq(0).text().split('/')[0]?.trim() || 'N/A',
            externalId: $row.find('td').eq(0).text().split('/')[1]?.trim() || '-',
            date: $row.find('td').eq(1).text().trim(),
            yarnItemName: $row.find('td').eq(2).text().trim(),
            transactionType: $row.find('td').eq(3).text().trim(),
            quantity: $row.find('td').eq(4).text().trim(),
            count: $row.find('td').eq(5).text().trim(),
            stakeholderName: $row.find('td').eq(6).text().trim(),
            packagingType: $row.find('td').eq(7).text().trim(),
            balance: $row.find('td').eq(8).text().trim(),
            comment: $row.find('td').eq(9).text().trim() || 'لا توجد ملاحظات'
        };
        setTimeout(() => { showTransactionModal(transactionData); hideLoadingOverlay(); }, 300);
    };

    function showTransactionModal(t) {
        const modalHtml = `<div class="modal fade" id="transactionDetailModal" tabindex="-1"><div class="modal-dialog modal-lg modal-dialog-centered"><div class="modal-content"><div class="modal-header bg-primary text-white"><h5 class="modal-title"><i class="fas fa-info-circle me-2"></i>تفاصيل المعاملة #${t.id}</h5><button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button></div><div class="modal-body"><div class="row g-3"><div class="col-md-6"><div class="card border-0 bg-light h-100"><div class="card-body"><h6 class="text-primary mb-3 border-bottom pb-2"><i class="fas fa-file-alt me-2"></i>معلومات الإذن</h6><div class="mb-2"><strong>رقم الإذن الداخلي:</strong><div class="mt-1">${t.internalId}</div></div><div class="mb-2"><strong>رقم الإذن الخارجي:</strong><div class="mt-1">${t.externalId}</div></div><div><strong>التاريخ:</strong><div class="mt-1">${t.date}</div></div></div></div></div><div class="col-md-6"><div class="card border-0 bg-light h-100"><div class="card-body"><h6 class="text-primary mb-3 border-bottom pb-2"><i class="fas fa-box me-2"></i>معلومات الصنف</h6><div class="mb-2"><strong>صنف الغزل:</strong><div class="mt-1">${t.yarnItemName}</div></div><div class="mb-2"><strong>نوع المعاملة:</strong><div class="mt-1"><span class="badge ${t.transactionType.includes('وارد') ? 'bg-success' : 'bg-danger'} px-3 py-2">${t.transactionType}</span></div></div><div><strong>نمط التعبئة:</strong><div class="mt-1">${t.packagingType}</div></div></div></div></div><div class="col-md-6"><div class="card border-0 bg-light h-100"><div class="card-body"><h6 class="text-primary mb-3 border-bottom pb-2"><i class="fas fa-balance-scale me-2"></i>الكميات</h6><div class="mb-2"><strong>الكمية:</strong><div class="fs-5 fw-bold text-success mt-1">${t.quantity} كجم</div></div><div class="mb-2"><strong>العدد:</strong><div class="fs-5 fw-bold mt-1">${t.count} وحدة</div></div><div><strong>الرصيد:</strong><div class="fs-5 fw-bold text-primary mt-1">${t.balance}</div></div></div></div></div><div class="col-md-6"><div class="card border-0 bg-light h-100"><div class="card-body"><h6 class="text-primary mb-3 border-bottom pb-2"><i class="fas fa-user me-2"></i>معلومات التاجر</h6><div><strong>اسم التاجر:</strong><div class="mt-1">${t.stakeholderName}</div></div></div></div></div><div class="col-12"><div class="card border-0 bg-light"><div class="card-body"><h6 class="text-primary mb-3 border-bottom pb-2"><i class="fas fa-comment me-2"></i>الملاحظات</h6><p class="mb-0">${t.comment}</p></div></div></div></div></div><div class="modal-footer bg-light"><button type="button" class="btn btn-secondary" data-bs-dismiss="modal"><i class="fas fa-times me-1"></i>إغلاق</button><a href="/YarnTransactions/Edit/${t.id}" class="btn btn-warning"><i class="fas fa-edit me-1"></i>تعديل</a><button type="button" class="btn btn-primary" onclick="$('.btn-print[data-transaction-id=${t.id}]').click();$('#transactionDetailModal').modal('hide');"><i class="fas fa-print me-1"></i>طباعة</button></div></div></div></div>`;
        $('#transactionDetailModal').remove();
        $('body').append(modalHtml);
        new bootstrap.Modal(document.getElementById('transactionDetailModal')).show();
        $('#transactionDetailModal').on('hidden.bs.modal', function () { $(this).remove(); });
    }

    window.printTransaction = function (transactionId, $row) {
        const t = {
            id: transactionId,
            internalId: $row.find('td').eq(0).text().split('/')[0]?.trim() || 'N/A',
            externalId: $row.find('td').eq(0).text().split('/')[1]?.trim() || '-',
            date: $row.find('td').eq(1).text().trim(),
            yarnItemName: $row.find('td').eq(2).text().trim(),
            transactionType: $row.find('td').eq(3).text().trim(),
            quantity: $row.find('td').eq(4).text().trim(),
            count: $row.find('td').eq(5).text().trim(),
            stakeholderName: $row.find('td').eq(6).text().trim(),
            packagingType: $row.find('td').eq(7).text().trim(),
            balance: $row.find('td').eq(8).text().trim(),
            comment: $row.find('td').eq(9).text().trim() || 'لا توجد ملاحظات'
        };
        const w = window.open('', '_blank', 'width=800,height=600');
        if (!w) { alert('يرجى السماح بفتح النوافذ المنبثقة'); return; }
        w.document.write(`<!DOCTYPE html><html dir="rtl"><head><meta charset="UTF-8"><title>طباعة #${transactionId}</title><link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet"><style>body{font-family:Arial;margin:20px}.print-header{text-align:center;margin-bottom:30px;border-bottom:3px solid #0d6efd;padding-bottom:15px}.print-header h2{color:#0d6efd;font-weight:bold}.info-table{width:100%;border-collapse:collapse}.info-table td{padding:12px;border:1px solid #dee2e6}.info-table td:first-child{background:#f8f9fa;font-weight:bold;width:35%}.badge{padding:6px 12px;border-radius:5px;color:white;font-weight:bold}.bg-success{background:#198754}.bg-danger{background:#dc3545}.text-success{color:#198754!important}.text-primary{color:#0d6efd!important}.fw-bold{font-weight:bold}.print-footer{margin-top:40px;padding-top:15px;border-top:2px solid #dee2e6;text-align:center;color:#6c757d}@media print{body{margin:10mm}}</style></head><body><div class="print-header"><h2>معاملة غزل</h2><p>رقم: ${transactionId}</p><p>تاريخ الطباعة: ${new Date().toLocaleDateString('ar-EG')}</p></div><table class="info-table"><tr><td>رقم الإذن الداخلي</td><td>${t.internalId}</td></tr><tr><td>رقم الإذن الخارجي</td><td>${t.externalId}</td></tr><tr><td>التاريخ</td><td>${t.date}</td></tr><tr><td>صنف الغزل</td><td>${t.yarnItemName}</td></tr><tr><td>نوع المعاملة</td><td><span class="badge ${t.transactionType.includes('وارد') ? 'bg-success' : 'bg-danger'}">${t.transactionType}</span></td></tr><tr><td>الكمية</td><td class="text-success fw-bold">${t.quantity} كجم</td></tr><tr><td>العدد</td><td class="fw-bold">${t.count} وحدة</td></tr><tr><td>تاجر الغزل</td><td>${t.stakeholderName}</td></tr><tr><td>نمط التعبئة</td><td>${t.packagingType}</td></tr><tr><td>الرصيد</td><td class="text-primary fw-bold">${t.balance}</td></tr><tr><td>الملاحظات</td><td>${t.comment}</td></tr></table><div class="print-footer"><p>تم الطباعة: ${new Date().toLocaleString('ar-EG')}</p></div><script>window.onload=()=>setTimeout(()=>window.print(),250)<\/script></body></html>`);
        w.document.close();
    };

    function addTableControls() {
        const $c = $('.card:has(.table)').find('.card-header');
        if (!$c.find('.table-controls').length) {
            $c.append(`<div class="table-controls d-flex justify-content-between mt-2"><div class="d-flex gap-2"><button class="btn btn-success btn-sm" onclick="exportToExcel()"><i class="fas fa-file-excel me-1"></i>تصدير Excel</button><button class="btn btn-warning btn-sm" onclick="printResults()"><i class="fas fa-print me-1"></i>طباعة الكل</button></div><span class="text-muted small" id="resultsCount">${toArabicDigits($('.table tbody tr').length)} نتيجة</span></div>`);
        }
        addSearchInputs();
    }

    function addSearchInputs() {
        const $h = $('.table thead tr');
        let html = '<tr class="search-row">';
        $h.find('th').each(function () {
            const txt = $(this).text().trim();
            let col = 'other';
            if (txt.includes('أرقام الإذن')) col = 'ids';
            else if (txt.includes('التاريخ')) col = 'date';
            else if (txt.includes('صنف الغزل')) col = 'yarnItem';
            else if (txt.includes('نوع المعاملة')) col = 'type';
            else if (txt.includes('الكمية')) col = 'quantity';
            else if (txt.includes('العدد')) col = 'count';
            else if (txt.includes('تاجر الغزل')) col = 'stakeholder';
            else if (txt.includes('نمط التعبئة')) col = 'packaging';
            else if (txt.includes('الرصيد')) col = 'balance';
            else if (txt.includes('ملاحظات')) col = 'comment';
            html += `<th class="search-header"><input type="text" class="form-control form-control-sm table-search" placeholder="بحث" data-column="${col}"></th>`;
        });
        html += '</tr>';
        $h.after(html);
    }

    function initializeInlineSearch() {
        $('.table-search').on('input', function (e) {
            e.stopPropagation();
            
            // Convert Latin digits to Arabic digits in the input display
            const currentValue = $(this).val();
            const arabicValue = toArabicDigits(currentValue);
            
            // Only update if there's a difference to avoid cursor jumping
            if (currentValue !== arabicValue) {
                const cursorPos = this.selectionStart;
                $(this).val(arabicValue);
                // Restore cursor position
                this.setSelectionRange(cursorPos, cursorPos);
            }
            
            filterResultsTable();
        }).on('click', e => e.stopPropagation());
    }

    function filterResultsTable() {
        const sv = {};
        $('.table-search').each(function () {
            const col = $(this).data('column');
            const txt = $(this).val().trim();
            sv[col] = { normalized: normalizeSearchText(txt) };
        });
        let cnt = 0;
        $('.table tbody tr').each(function () {
            const $r = $(this);
            let show = true;
            for (const [col, sd] of Object.entries(sv)) {
                if (sd.normalized) {
                    let cv = '';
                    const idx = { ids: 0, date: 1, yarnItem: 2, type: 3, quantity: 4, count: 5, stakeholder: 6, packaging: 7, balance: 8, comment: 9 }[col] || 0;
                    cv = $r.find('td').eq(idx).text().trim();
                    if (!normalizeSearchText(cv).includes(sd.normalized)) { show = false; break; }
                }
            }
            $r.toggle(show);
            if (show) cnt++;
        });
        $('#resultsCount').text(toArabicDigits(cnt) + ' نتيجة');
    }

    function initializeTableSorting() {
        $('.table thead th:not(.search-header)').on('click', function (e) {
            if ($(e.target).hasClass('table-search')) return;
            const idx = $(this).index();
            $('.table thead th i').remove();
            if (currentSortColumn === idx) currentSortDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
            else { currentSortColumn = idx; currentSortDirection = 'asc'; }
            $(this).append(`<i class="fas fa-sort-${currentSortDirection === 'asc' ? 'up' : 'down'} ms-1"></i>`);
            sortResultsTable(idx, currentSortDirection);
        });
    }

    function sortResultsTable(idx, dir) {
        const $t = $('.table');
        const rows = $t.find('tbody tr').get();
        rows.sort((a, b) => {
            let aVal = $(a).find('td').eq(idx).text().trim();
            let bVal = $(b).find('td').eq(idx).text().trim();
            if (idx === 1) { aVal = new Date(toLatinDigits(aVal)); bVal = new Date(toLatinDigits(bVal)); }
            else if ([4, 5, 8].includes(idx)) { aVal = parseFloat(toLatinDigits(aVal)) || 0; bVal = parseFloat(toLatinDigits(bVal)) || 0; }
            return dir === 'asc' ? (aVal > bVal ? 1 : -1) : (aVal < bVal ? 1 : -1);
        });
        $.each(rows, (i, r) => $t.find('tbody').append(r));
    }

    function convertTableNumbersToArabic() {
        $('.table td').each(function () {
            const txt = $(this).text().trim();
            if (txt && /\d/.test(txt) && !txt.match(/[٠-٩]/)) $(this).text(toArabicDigits(txt));
        });
    }

    window.exportToExcel = function () {
        showLoadingOverlay();
        try {
            const data = [['أرقام الإذن', 'التاريخ', 'صنف الغزل', 'نوع المعاملة', 'الكمية', 'العدد', 'تاجر الغزل', 'نمط التعبئة', 'الرصيد', 'ملاحظات']];
            $('.table tbody tr:visible').each(function () {
                const $r = $(this);
                data.push([$r.find('td').eq(0).text().trim(), $r.find('td').eq(1).text().trim(), $r.find('td').eq(2).text().trim(), $r.find('td').eq(3).text().trim(), toLatinDigits($r.find('td').eq(4).text().trim()), toLatinDigits($r.find('td').eq(5).text().trim()), $r.find('td').eq(6).text().trim(), $r.find('td').eq(7).text().trim(), $r.find('td').eq(8).text().trim(), $r.find('td').eq(9).text().trim()]);
            });
            const wb = XLSX.utils.book_new();
            const ws = XLSX.utils.aoa_to_sheet(data);
            if (!ws['!views']) ws['!views'] = [];
            ws['!views'].push({ rightToLeft: true });
            ws['!cols'] = data[0].map(() => ({ wch: 20 }));
            XLSX.utils.book_append_sheet(wb, ws, 'البيانات');
            XLSX.writeFile(wb, `نتائج_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.xlsx`);
        } catch (err) { alert('حدث خطأ: ' + err.message); }
        finally { setTimeout(hideLoadingOverlay, 1000); }
    };

    window.printResults = function () {
        let html = `<div class="print-header"><h2>نتائج البحث في معاملات الغزل</h2><p>تاريخ: ${toArabicDigits(new Date().toLocaleDateString('ar-EG'))}</p><p>عدد: ${toArabicDigits($('.table tbody tr:visible').length)}</p></div><table class="table table-bordered"><thead><tr><th>أرقام الإذن</th><th>التاريخ</th><th>صنف الغزل</th><th>نوع المعاملة</th><th>الكمية</th><th>العدد</th><th>تاجر الغزل</th><th>نمط التعبئة</th><th>الرصيد</th><th>ملاحظات</th></tr></thead><tbody>`;
        $('.table tbody tr:visible').each(function () {
            const $r = $(this);
            html += `<tr><td>${$r.find('td').eq(0).html()}</td><td>${$r.find('td').eq(1).text()}</td><td>${$r.find('td').eq(2).html()}</td><td>${$r.find('td').eq(3).html()}</td><td>${$r.find('td').eq(4).text()}</td><td>${$r.find('td').eq(5).text()}</td><td>${$r.find('td').eq(6).html()}</td><td>${$r.find('td').eq(7).text()}</td><td>${$r.find('td').eq(8).html()}</td><td>${$r.find('td').eq(9).text()}</td></tr>`;
        });
        html += '</tbody></table>';
        const w = window.open('', '_blank', 'width=1000,height=600');
        w.document.write(`<!DOCTYPE html><html dir="rtl"><head><meta charset="UTF-8"><title>طباعة النتائج</title><link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet"><style>body{font-family:Arial;margin:20px}.print-header{text-align:center;margin-bottom:20px;border-bottom:2px solid #333;padding-bottom:10px}.table{width:100%;border-collapse:collapse;font-size:12px}.table th{background:#f8f9fa;border:1px solid #dee2e6;padding:8px;text-align:center}.table td{border:1px solid #dee2e6;padding:6px;text-align:center}@media print{body{margin:0.5cm}}</style></head><body>${html}<script>window.onload=()=>setTimeout(()=>window.print(),500)<\/script></body></html>`);
        w.document.close();
    };

    function showLoadingOverlay() {
        $('body').append('<div id="loadingOverlay" style="position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.5);z-index:9999;display:flex;align-items:center;justify-content:center"><div class="spinner-border text-light"></div></div>');
    }

    function hideLoadingOverlay() {
        $('#loadingOverlay').remove();
    }

    convertDisplayToArabic();

    const today = new Date();
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(today.getDate() - 30);
    const todayStr = today.toISOString().split('T')[0];
    const thirtyStr = thirtyDaysAgo.toISOString().split('T')[0];

    if (!fromDateInput.value) {
        fromDateInput.setAttribute('data-latin-date', thirtyStr);
        fromDateInput.value = toArabicDigits(thirtyStr);
        fpFrom.setDate(thirtyStr);
    }
    if (!toDateInput.value) {
        toDateInput.setAttribute('data-latin-date', todayStr);
        toDateInput.value = toArabicDigits(todayStr);
        fpTo.setDate(todayStr);
    }

    initializeTableFunctionality();
    console.log('✅ Search form ready with Arabic calendar and table functionality!');
});