
// ============================================================================
// EXCEL EXPORT SYSTEM
// ============================================================================

/**
 * Export search results to Excel
 */
function exportToExcel() {
    const currentTime = '2025-11-26 15:52:00';
    const currentUser = 'Ammar-Yasser8';

    console.log(`[${currentTime}] [${currentUser}] 🚀 Starting search results Excel export`);

    // Collect data from the table
    const data = collectSearchDataForExcel();

    if (data.length <= 1) {
        alert('لا توجد بيانات للتصدير');
        return;
    }

    exportToExcelFile(data, 'نتائج_بحث_المعاملات');
}

/**
 * Collect search data for Excel export
 */
function collectSearchDataForExcel() {
    const headers = [
        'رقم إذن داخلي',
        'رقم إذن خارجي',
        'التاريخ',
        'صنف الغزل',
        'نوع المعاملة',
        'الكمية (كجم)',
        'العدد',
        'تاجر الغزل',
        'نوع التاجر',
        'نمط التعبئة',
        'رصيد الكمية',
        'رصيد العدد',
        'ملاحظات'
    ];

    const data = [headers];

    $('table tbody tr').each(function () {
        const $row = $(this);

        // Helper to get text and clean it
        const getText = (index) => $row.find('td').eq(index).text().trim();
        const getCleanText = (index) => getText(index).replace(/\s+/g, ' ').trim();

        // Parse date to remove time and ensure RTL format if needed (though Excel handles dates)
        // For this requirement: "التاريخ RTL وبدون ساعة"
        // We will format it as string yyyy-MM-dd or dd/MM/yyyy
        let dateStr = getCleanText(2);
        // Assuming dateStr is currently "dd/MM/yyyy HH:mm" or similar
        // We want just the date part. 
        if (dateStr.includes(' ')) {
            dateStr = dateStr.split(' ')[0];
        }

        // Internal and External IDs are now in separate columns (0 and 1)
        const internalId = toArabicDigits(getCleanText(0).replace('-', ''));
        const externalId = toArabicDigits(getCleanText(1).replace('-', ''));

        // Stakeholder info is in column 6 (Name + Type in small tag)
        // We need to extract them. In the new view, they are in one cell?
        // Let's check the view change.
        // View: 
        // <td>
        //     <strong>@item.StakeholderName</strong>
        //     <br><small class="text-muted">@item.StakeholderTypeName</small>
        // </td>
        // So we need to parse this.
        const stakeholderCell = $row.find('td').eq(6);
        const stakeholderName = stakeholderCell.find('strong').text().trim();
        const stakeholderType = stakeholderCell.find('small').text().trim();

        // Balance columns are 9 and 10
        // Requirement: "الرصيد رقم فقط وبدون فاصلة الألف"
        // We will parse as number then format as Arabic without separators if needed, 
        // OR just keep as raw number for Excel and let Excel format, 
        // BUT user asked for "Arabic digits" and "No thousand separator".
        // If we pass numbers to Excel, it uses system locale. 
        // To force Arabic digits in Excel cell content (as text), we convert.
        // To force "No thousand separator", we just don't add commas.

        const quantityBalance = $row.find('td').eq(9).text().trim().replace(/,/g, '');
        const countBalance = $row.find('td').eq(10).text().trim().replace(/,/g, '');

        const rowData = [
            internalId,
            externalId,
            toArabicDigits(dateStr),
            getCleanText(3).split(' ')[0], // Yarn Name (simplifying if complex) - actually just take full text
            getCleanText(4), // Transaction Type
            toArabicDigits(getCleanText(5).replace(/,/g, '')), // Quantity
            toArabicDigits(getCleanText(6)), // Count
            stakeholderName,
            stakeholderType, // Entity Type (Stakeholder Type)
            getCleanText(7), // Packaging Style
            toArabicDigits(quantityBalance),
            toArabicDigits(countBalance),
            getCleanText(11) // Comments
        ];
        data.push(rowData);
    });

    return data;
}

/**
 * Enhanced Excel file export with Arabic RTL support
 */
function exportToExcelFile(data, fileName) {
    const wb = XLSX.utils.book_new();

    // Set workbook properties
    wb.Props = {
        Title: fileName,
        Subject: 'تقرير معاملات الغزل',
        Author: 'Ammar-Yasser8',
        CreatedDate: new Date(),
        Language: 'ar-SA'
    };

    // Set workbook view properties for RTL
    wb.Workbook = {
        Views: [{
            rightToLeft: true
        }]
    };

    const ws = XLSX.utils.aoa_to_sheet(data);

    // Configure RTL
    if (!ws['!views']) ws['!views'] = [];
    ws['!views'].push({ rightToLeft: true });

    // Auto-width columns
    const colWidths = data[0].map(() => ({ wch: 20 }));
    ws['!cols'] = colWidths;

    XLSX.utils.book_append_sheet(wb, ws, "المعاملات");

    // Generate filename with timestamp
    const date = new Date().toISOString().split('T')[0];
    const finalFileName = `${fileName}_${date}.xlsx`;

    XLSX.writeFile(wb, finalFileName);
}

/**
 * Print Search Results
 */
function printSearch() {
    window.print();
}

// Add print styles
const printStyle = document.createElement('style');
printStyle.textContent = `
    @media print {
        body * {
            visibility: hidden;
        }
        .search-container, .search-container * {
            visibility: visible;
        }
        .search-container {
            position: absolute;
            left: 0;
            top: 0;
            width: 100%;
        }
        .card-header, .btn, form {
            display: none !important;
        }
        .card {
            border: none !important;
            box-shadow: none !important;
        }
        table {
            width: 100% !important;
            border-collapse: collapse !important;
            font-size: 10pt !important;
        }
        th, td {
            border: 1px solid #ddd !important;
            padding: 4px !important;
            text-align: center !important;
        }
        /* Ensure Arabic digits in print */
        .arabic-display, .arabic-cell {
            font-family: 'Arial', sans-serif;
        }
    }
`;
document.head.appendChild(printStyle);
