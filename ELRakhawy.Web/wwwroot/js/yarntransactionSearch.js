$(document).ready(function () {
    console.log('🔍 Yarn Transaction Search with Transaction Form Calendar');
    console.log('👤 User: Ammar-Yasser8 | Time: 2025-11-18 20:03:02');

    let currentSortColumn = '';
    let currentSortDirection = 'asc';
    let currentSearchResults = [];

    // ✅ Utility functions (Same as transaction form)
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

    // ✅ Enhanced Arabic Flatpickr with Reset to Today Button (EXACT COPY from transaction form)
    if (window.flatpickr) {
        // Arabic locale configuration with full customization
        const arabicLocale = {
            weekdays: {
                shorthand: ['أحد', 'إثنين', 'ثلاثاء', 'أربعاء', 'خميس', 'جمعة', 'سبت'],
                longhand: [
                    'الأحد',
                    'الإثنين',
                    'الثلاثاء',
                    'الأربعاء',
                    'الخميس',
                    'الجمعة',
                    'السبت'
                ]
            },
            months: {
                shorthand: [
                    'يناير',
                    'فبراير',
                    'مارس',
                    'أبريل',
                    'مايو',
                    'يونيو',
                    'يوليو',
                    'أغسطس',
                    'سبتمبر',
                    'أكتوبر',
                    'نوفمبر',
                    'ديسمبر'
                ],
                longhand: [
                    'يناير',
                    'فبراير',
                    'مارس',
                    'أبريل',
                    'مايو',
                    'يونيو',
                    'يوليو',
                    'أغسطس',
                    'سبتمبر',
                    'أكتوبر',
                    'نوفمبر',
                    'ديسمبر'
                ]
            },
            firstDayOfWeek: 6, // Saturday
            rangeSeparator: ' إلى ',
            weekAbbreviation: 'أسبوع',
            scrollTitle: 'قم بالتمرير للزيادة',
            toggleTitle: 'اضغط للتبديل',
            amPM: ['ص', 'م'],
            yearAriaLabel: 'سنة',
            monthAriaLabel: 'شهر',
            hourAriaLabel: 'ساعة',
            minuteAriaLabel: 'دقيقة',
            time_24hr: true
        };

        // Function to add "Today" button (EXACT COPY)
        function addTodayButton(instance) {
            const calendar = instance.calendarContainer;

            // Remove existing button if any
            const existingBtn = calendar.querySelector('.flatpickr-today-btn');
            if (existingBtn) {
                existingBtn.remove();
            }

            // Create today button container
            const todayBtnContainer = document.createElement('div');
            todayBtnContainer.className = 'flatpickr-today-btn-container';
            todayBtnContainer.style.cssText = `
                padding: 8px;
                border-top: 1px solid #e6e6e6;
                background: #f8f9fa;
                display: flex;
                justify-content: center;
                gap: 8px;
            `;

            // Create "Today" button
            const todayBtn = document.createElement('button');
            todayBtn.type = 'button';
            todayBtn.className = 'flatpickr-today-btn';
            todayBtn.innerHTML = '<i class="fas fa-calendar-day" style="margin-left: 5px;"></i>اليوم';
            todayBtn.style.cssText = `
                padding: 6px 16px;
                background: #198754;
                color: white;
                border: none;
                border-radius: 4px;
                cursor: pointer;
                font-size: 14px;
                font-weight: 600;
                transition: all 0.2s;
                display: flex;
                align-items: center;
                gap: 5px;
            `;

            todayBtn.addEventListener('mouseenter', function () {
                this.style.background = '#146c43';
                this.style.transform = 'translateY(-1px)';
            });

            todayBtn.addEventListener('mouseleave', function () {
                this.style.background = '#198754';
                this.style.transform = 'translateY(0)';
            });

            todayBtn.addEventListener('click', function (e) {
                e.preventDefault();
                const today = new Date();
                instance.setDate(today, true);
                instance.close();

                // Show success notification
                showTodaySetNotification();
            });

            // Create "Clear" button (optional)
            const clearBtn = document.createElement('button');
            clearBtn.type = 'button';
            clearBtn.className = 'flatpickr-clear-btn';
            clearBtn.innerHTML = '<i class="fas fa-times" style="margin-left: 5px;"></i>مسح';
            clearBtn.style.cssText = `
                padding: 6px 16px;
                background: #dc3545;
                color: white;
                border: none;
                border-radius: 4px;
                cursor: pointer;
                font-size: 14px;
                font-weight: 600;
                transition: all 0.2s;
                display: flex;
                align-items: center;
                gap: 5px;
            `;

            clearBtn.addEventListener('mouseenter', function () {
                this.style.background = '#bb2d3b';
                this.style.transform = 'translateY(-1px)';
            });

            clearBtn.addEventListener('mouseleave', function () {
                this.style.background = '#dc3545';
                this.style.transform = 'translateY(0)';
            });

            clearBtn.addEventListener('click', function (e) {
                e.preventDefault();
                instance.clear();
                const input = instance.input;
                input.value = '';
                input.removeAttribute('data-latin-date');
            });

            todayBtnContainer.appendChild(todayBtn);
            todayBtnContainer.appendChild(clearBtn);

            // Append button container to calendar
            calendar.appendChild(todayBtnContainer);
        }

        // Function to convert all calendar text to Arabic (EXACT COPY)
        function convertCalendarToArabic() {
            // Convert day numbers
            document.querySelectorAll('.flatpickr-day').forEach(function (day) {
                const text = day.textContent.trim();
                if (text && /^\d+$/.test(text)) {
                    day.textContent = toArabicDigits(text);
                }
            });

            // Convert year in year dropdown with proper Arabic display
            document.querySelectorAll('.cur-year, .numInput').forEach(function (yearInput) {
                if (yearInput.value && /^\d+$/.test(yearInput.value)) {
                    const arabicYear = toArabicDigits(yearInput.value);

                    // For input elements (year input field)
                    if (yearInput.tagName === 'INPUT') {
                        // Remove any existing overlay first
                        const existingOverlay = yearInput.parentNode.querySelector('.arabic-year-overlay');
                        if (existingOverlay) {
                            existingOverlay.remove();
                        }

                        // Store the original Latin value for functionality
                        yearInput.setAttribute('data-latin-value', yearInput.value);

                        // Create a single visual overlay for Arabic display
                        const overlay = document.createElement('div');
                        overlay.className = 'arabic-year-overlay';
                        overlay.style.cssText = `
                            position: absolute;
                            top: 0;
                            left: 0;
                            right: 0;
                            bottom: 0;
                            pointer-events: none;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            color: #495057;
                            font-size: inherit;
                            background: transparent;
                            z-index: 10;
                            font-weight: 500;
                        `;
                        overlay.textContent = arabicYear;

                        // Make parent relative for positioning
                        yearInput.style.position = 'relative';
                        yearInput.style.color = 'transparent';
                        yearInput.style.textShadow = '0 0 0 transparent';
                        yearInput.parentNode.style.position = 'relative';

                        // Append the overlay
                        yearInput.parentNode.appendChild(overlay);

                        // Clean up any existing event listeners
                        yearInput.removeEventListener('input', yearInput._arabicHandler);

                        // Add new event listener
                        yearInput._arabicHandler = function () {
                            const overlay = this.parentNode.querySelector('.arabic-year-overlay');
                            if (overlay && this.value && /^\d+$/.test(this.value)) {
                                overlay.textContent = toArabicDigits(this.value);
                            }
                        };
                        yearInput.addEventListener('input', yearInput._arabicHandler);

                    } else {
                        // For non-input elements (display text)
                        yearInput.textContent = arabicYear;
                    }
                }
            });

            // Handle year inputs with proper mutation observation
            const yearInputs = document.querySelectorAll('.numInput[data-type="year"]');
            yearInputs.forEach(function (input) {
                // Disconnect existing observers
                if (input._mutationObserver) {
                    input._mutationObserver.disconnect();
                }

                // Create new observer
                input._mutationObserver = new MutationObserver(function (mutations) {
                    mutations.forEach(function (mutation) {
                        if (mutation.type === 'attributes' &&
                            (mutation.attributeName === 'value' || mutation.attributeName === 'data-value')) {

                            const overlay = input.parentNode.querySelector('.arabic-year-overlay');
                            if (overlay && input.value && /^\d+$/.test(input.value)) {
                                overlay.textContent = toArabicDigits(input.value);
                            }
                        }
                    });
                });

                // Start observing
                input._mutationObserver.observe(input, {
                    attributes: true,
                    attributeFilter: ['value', 'data-value']
                });

                // Also listen for direct value changes
                if (!input._arabicValueHandler) {
                    input._arabicValueHandler = function (e) {
                        setTimeout(() => {
                            const overlay = this.parentNode.querySelector('.arabic-year-overlay');
                            if (overlay && this.value && /^\d+$/.test(this.value)) {
                                overlay.textContent = toArabicDigits(this.value);
                            }
                        }, 10);
                    };

                    input.addEventListener('change', input._arabicValueHandler);
                    input.addEventListener('keyup', input._arabicValueHandler);
                    input.addEventListener('blur', input._arabicValueHandler);
                }
            });

            // Convert month dropdown options
            document.querySelectorAll('.flatpickr-monthDropdown-months option').forEach(function (option) {
                const text = option.textContent;
                if (text && /\d/.test(text)) {
                    option.textContent = toArabicDigits(text);
                }
            });

            // Handle month navigation arrows and other numeric displays
            document.querySelectorAll('.flatpickr-current-month span.cur-month').forEach(function (monthSpan) {
                const text = monthSpan.textContent;
                if (text && /\d/.test(text)) {
                    monthSpan.textContent = toArabicDigits(text);
                }
            });

            // Ensure proper RTL layout
            const calendar = document.querySelector('.flatpickr-calendar');
            if (calendar) {
                calendar.dir = 'rtl';
            }
        }

        // Highlight today's date with special styling (EXACT COPY)
        function highlightTodayIfNeeded(instance) {
            const today = new Date();
            const todayStr = today.toISOString().split('T')[0];

            document.querySelectorAll('.flatpickr-day').forEach(function (day) {
                const dayDate = day.getAttribute('aria-label');
                if (day.classList.contains('today')) {
                    day.style.position = 'relative';

                    // Add a subtle indicator
                    if (!day.querySelector('.today-indicator')) {
                        const indicator = document.createElement('div');
                        indicator.className = 'today-indicator';
                        indicator.style.cssText = `
                            position: absolute;
                            bottom: 2px;
                            left: 50%;
                            transform: translateX(-50%);
                            width: 4px;
                            height: 4px;
                            background: #198754;
                            border-radius: 50%;
                        `;
                        day.appendChild(indicator);
                    }
                }
            });
        }

        // Show notification when today is set (EXACT COPY)
        function showTodaySetNotification() {
            // Create toast notification
            const toast = document.createElement('div');
            toast.className = 'today-set-toast';
            toast.innerHTML = `
                <i class="fas fa-check-circle" style="margin-left: 8px;"></i>
                تم تعيين تاريخ اليوم
            `;
            toast.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                background: #198754;
                color: white;
                padding: 12px 20px;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                z-index: 10000;
                display: flex;
                align-items: center;
                font-size: 14px;
                font-weight: 600;
                animation: slideInRight 0.3s ease-out;
            `;

            document.body.appendChild(toast);

            // Remove after 2 seconds
            setTimeout(() => {
                toast.style.animation = 'slideOutRight 0.3s ease-out';
                setTimeout(() => {
                    toast.remove();
                }, 300);
            }, 2000);
        }

        // Initialize From Date calendar
        const fromDateInput = document.getElementById('FromDate');
        let fpFrom = null;

        if (fromDateInput) {
            fpFrom = flatpickr(fromDateInput, {
                dateFormat: 'Y-m-d',
                locale: arabicLocale,
                allowInput: false,
                disableMobile: true,
                defaultDate: fromDateInput.value || new Date(),
                weekNumbers: false,
                position: 'auto center',

                // Custom rendering for Arabic numbers
                onDayCreate: function (dObj, dStr, fp, dayElem) {
                    const dayNumber = dayElem.textContent;
                    dayElem.textContent = toArabicDigits(dayNumber);
                },

                onChange: function (selectedDates, dateStr, instance) {
                    fromDateInput.setAttribute('data-latin-date', dateStr);
                    fromDateInput.value = toArabicDigits(dateStr);
                    convertCalendarToArabic();
                },

                onReady: function (selectedDates, dateStr, instance) {
                    if (dateStr) {
                        fromDateInput.setAttribute('data-latin-date', dateStr);
                        fromDateInput.value = toArabicDigits(dateStr);
                    }
                    addTodayButton(instance);
                    convertCalendarToArabic();
                    instance.calendarContainer.classList.add('flatpickr-rtl');
                },

                onMonthChange: function (selectedDates, dateStr, instance) {
                    setTimeout(() => convertCalendarToArabic(), 50);
                },

                onYearChange: function (selectedDates, dateStr, instance) {
                    setTimeout(() => convertCalendarToArabic(), 50);
                },

                onOpen: function (selectedDates, dateStr, instance) {
                    setTimeout(() => {
                        convertCalendarToArabic();
                        highlightTodayIfNeeded(instance);
                    }, 50);
                }
            });
        }

        // Initialize To Date calendar
        const toDateInput = document.getElementById('ToDate');
        let fpTo = null;

        if (toDateInput) {
            fpTo = flatpickr(toDateInput, {
                dateFormat: 'Y-m-d',
                locale: arabicLocale,
                allowInput: false,
                disableMobile: true,
                defaultDate: toDateInput.value || new Date(),
                weekNumbers: false,
                position: 'auto center',

                // Custom rendering for Arabic numbers
                onDayCreate: function (dObj, dStr, fp, dayElem) {
                    const dayNumber = dayElem.textContent;
                    dayElem.textContent = toArabicDigits(dayNumber);
                },

                onChange: function (selectedDates, dateStr, instance) {
                    toDateInput.setAttribute('data-latin-date', dateStr);
                    toDateInput.value = toArabicDigits(dateStr);
                    convertCalendarToArabic();
                },

                onReady: function (selectedDates, dateStr, instance) {
                    if (dateStr) {
                        toDateInput.setAttribute('data-latin-date', dateStr);
                        toDateInput.value = toArabicDigits(dateStr);
                    }
                    addTodayButton(instance);
                    convertCalendarToArabic();
                    instance.calendarContainer.classList.add('flatpickr-rtl');
                },

                onMonthChange: function (selectedDates, dateStr, instance) {
                    setTimeout(() => convertCalendarToArabic(), 50);
                },

                onYearChange: function (selectedDates, dateStr, instance) {
                    setTimeout(() => convertCalendarToArabic(), 50);
                },

                onOpen: function (selectedDates, dateStr, instance) {
                    setTimeout(() => {
                        convertCalendarToArabic();
                        highlightTodayIfNeeded(instance);
                    }, 50);
                }
            });
        }

        // External calendar button handlers
        $('#fromCalendarBtn').on('click', function () {
            if (fpFrom) fpFrom.open();
        });

        $('#toCalendarBtn').on('click', function () {
            if (fpTo) fpTo.open();
        });

        // Set initial Arabic format if dates exist
        if (fromDateInput && fromDateInput.value) {
            const latinDate = fromDateInput.value;
            fromDateInput.setAttribute('data-latin-date', latinDate);
            fromDateInput.value = toArabicDigits(latinDate);
        }
        if (toDateInput && toDateInput.value) {
            const latinDate = toDateInput.value;
            toDateInput.setAttribute('data-latin-date', latinDate);
            toDateInput.value = toArabicDigits(latinDate);
        }

        // Store flatpickr instances globally for external access
        window.fpFromInstance = fpFrom;
        window.fpToInstance = fpTo;
    }

    // Add CSS animations and styles (EXACT COPY from transaction form)
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideInRight {
            from {
                transform: translateX(100%);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }

        @keyframes slideOutRight {
            from {
                transform: translateX(0);
                opacity: 1;
            }
            to {
                transform: translateX(100%);
                opacity: 0;
            }
        }

        /* RTL Calendar Styling */
        .flatpickr-rtl {
            direction: rtl !important;
        }

        .flatpickr-rtl .flatpickr-months {
            direction: rtl !important;
        }

        .flatpickr-rtl .flatpickr-current-month {
            padding: 0 !important;
        }

        .flatpickr-rtl .flatpickr-prev-month {
            right: auto !important;
            left: 0 !important;
        }

        .flatpickr-rtl .flatpickr-next-month {
            left: auto !important;
            right: 0 !important;
        }

        /* Today indicator styling */
        .flatpickr-day.today {
            border-color: #198754 !important;
            font-weight: bold !important;
        }

        .flatpickr-day.today:not(.selected) {
            background: #d4edda !important;
            color: #155724 !important;
        }

        /* Selected date styling */
        .flatpickr-day.selected {
            background: #198754 !important;
            border-color: #198754 !important;
            color: white !important;
            font-weight: bold !important;
        }

        .flatpickr-day.selected:hover {
            background: #146c43 !important;
            border-color: #146c43 !important;
        }

        /* Hover effect */
        .flatpickr-day:hover:not(.selected):not(.flatpickr-disabled) {
            background: #e9ecef !important;
            border-color: #dee2e6 !important;
        }

        /* Month and year dropdowns - Arabic friendly */
        .flatpickr-monthDropdown-months,
        .numInput {
            font-family: 'Arial', 'Tahoma', sans-serif !important;
            direction: ltr !important;
        }

        /* Better spacing for buttons */
        .flatpickr-today-btn-container {
            margin-top: 0 !important;
        }

        /* Calendar container */
        .flatpickr-calendar {
            box-shadow: 0 4px 12px rgba(0,0,0,0.15) !important;
            border-radius: 8px !important;
            overflow: hidden !important;
        }

        /* Days container */
        .flatpickr-days {
            border-left: 0 !important;
            border-right: 0 !important;
        }

        /* Weekday labels */
        .flatpickr-weekday {
            font-weight: 700 !important;
            color: #198754 !important;
        }

        /* Disabled dates */
        .flatpickr-day.flatpickr-disabled {
            color: #ccc !important;
        }

        /* Better focus states */
        .flatpickr-day:focus {
            outline: 2px solid #198754 !important;
            outline-offset: 2px !important;
        }

        /* Mobile responsiveness */
        @media (max-width: 768px) {
            .flatpickr-calendar {
                width: 100% !important;
                max-width: 350px !important;
            }

            .flatpickr-today-btn,
            .flatpickr-clear-btn {
                font-size: 13px !important;
                padding: 5px 12px !important;
            }
        }

        /* Reset/Today external button styling */
        .reset-today-btn {
            background: linear-gradient(45deg, #198754, #20c997) !important;
            border: none !important;
            color: white !important;
            font-weight: bold !important;
            transition: all 0.3s ease !important;
            border-radius: 20px !important;
            padding: 8px 16px !important;
            font-family: 'Tahoma', Arial, sans-serif !important;
        }

        .reset-today-btn:hover {
            background: linear-gradient(45deg, #146c43, #1ea085) !important;
            transform: scale(1.05) translateY(-1px) !important;
            box-shadow: 0 4px 12px rgba(25, 135, 84, 0.3) !important;
        }

        /* Clear button styling */
        .btn-clear {
            position: absolute !important;
            right: 5px !important;
            top: 50% !important;
            transform: translateY(-50%) !important;
            border: none !important;
            background: transparent !important;
            color: #6c757d !important;
            z-index: 10 !important;
            padding: 2px 6px !important;
            font-size: 12px !important;
            transition: all 0.2s ease !important;
            border-radius: 50% !important;
        }

        .btn-clear:hover {
            color: #dc3545 !important;
            background: #f8d7da !important;
            transform: translateY(-50%) scale(1.2) !important;
        }
    `;
    document.head.appendChild(style);

    console.log('✅ Enhanced Arabic Flatpickr with Today button initialized successfully');

    // Convert existing content to Arabic (Same as transaction form)
    function convertExistingContentToArabic() {
        $('span, div, p, td, th, label, small').each(function () {
            let $element = $(this);
            let text = $element.text();
            if ($element.find('input, select, textarea').length > 0 ||
                $element.hasClass('flatpickr-calendar') ||
                $element.closest('.flatpickr-calendar').length > 0 ||
                $element.attr('id') === 'FromDate' ||
                $element.attr('id') === 'ToDate' ||
                $element.find('#FromDate, #ToDate').length > 0) {
                return;
            }
            if (/[0-9]/.test(text)) {
                $element.text(toArabicDigits(text));
            }
        });

        $('input[placeholder]:not(#FromDate):not(#ToDate):not(.flatpickr-input), textarea[placeholder]').each(function () {
            let $input = $(this);
            let placeholder = $input.attr('placeholder');
            if (placeholder && /[0-9]/.test(placeholder)) {
                $input.attr('placeholder', toArabicDigits(placeholder));
            }
        });

        $('.badge, .alert, .card-text, .form-text').each(function () {
            let $element = $(this);
            let html = $element.html();
            if (html && /[0-9]/.test(html) &&
                !$element.closest('.flatpickr-calendar').length &&
                !$element.find('#FromDate, #ToDate').length) {
                $element.html(html.replace(/\b\d+(\.\d+)?\b/g, match => toArabicDigits(match)));
            }
        });

        console.log('✅ Converted existing content to Arabic digits');
    }

    // Enhanced Select2 configuration with Arabic support
    $('.form-select').select2({
        theme: 'bootstrap-5',
        width: '100%',
        dir: 'rtl',
        language: {
            noResults: () => "لا توجد نتائج",
            searching: () => "جاري البحث...",
            loadingMore: () => "جاري تحميل المزيد...",
            maximumSelected: () => "تم الوصول للحد الأقصى"
        },
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
            const normalizeArabic = (str) => {
                if (!str) return '';
                return str.replace(/[أإآ]/g, 'ا').replace(/ى/g, 'ي');
            };
            let searchTerm = toLatinDigits(params.term.toLowerCase().trim());
            let optionText = toLatinDigits(data.text.toLowerCase().trim());
            searchTerm = normalizeArabic(searchTerm);
            optionText = normalizeArabic(optionText);
            if (optionText.includes(searchTerm)) return data;
            return null;
        }
    });

    // Clear button functionality for text inputs
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

    // Enhanced form submission with proper date handling
    $('#searchForm').on('submit', function () {
        const fromLatin = $('#FromDate').attr('data-latin-date') || toLatinDigits($('#FromDate').val());
        const toLatin = $('#ToDate').attr('data-latin-date') || toLatinDigits($('#ToDate').val());

        // Temporarily set Latin values for submission
        $('#FromDate').val(fromLatin);
        $('#ToDate').val(toLatin);

        $('#InternalId, #ExternalId').each(function () {
            const latinValue = toLatinDigits($(this).val());
            $(this).val(latinValue);
        });
    });

    // Enhanced date range functions with notifications
    window.setToday = function () {
        const today = new Date().toISOString().split('T')[0];
        if (fromDateInput) {
            fromDateInput.setAttribute('data-latin-date', today);
            fromDateInput.value = toArabicDigits(today);
            if (fpFrom) fpFrom.setDate(today);
        }
        if (toDateInput) {
            toDateInput.setAttribute('data-latin-date', today);
            toDateInput.value = toArabicDigits(today);
            if (fpTo) fpTo.setDate(today);
        }
        showTodaySetNotification();
    };

    window.resetToToday = function () {
        const today = new Date().toISOString().split('T')[0];
        if (fromDateInput) {
            fromDateInput.setAttribute('data-latin-date', today);
            fromDateInput.value = toArabicDigits(today);
            if (fpFrom) fpFrom.setDate(today);
        }
        if (toDateInput) {
            toDateInput.setAttribute('data-latin-date', today);
            toDateInput.value = toArabicDigits(today);
            if (fpTo) fpTo.setDate(today);
        }
        showTodaySetNotification();
    };

    window.setThisWeek = function () {
        const today = new Date();
        const firstDay = new Date(today.setDate(today.getDate() - today.getDay()));
        const lastDay = new Date();
        const firstStr = firstDay.toISOString().split('T')[0];
        const lastStr = lastDay.toISOString().split('T')[0];

        if (fromDateInput) {
            fromDateInput.setAttribute('data-latin-date', firstStr);
            fromDateInput.value = toArabicDigits(firstStr);
            if (fpFrom) fpFrom.setDate(firstStr);
        }
        if (toDateInput) {
            toDateInput.setAttribute('data-latin-date', lastStr);
            toDateInput.value = toArabicDigits(lastStr);
            if (fpTo) fpTo.setDate(lastStr);
        }
        showTodaySetNotification();
    };

    window.setThisMonth = function () {
        const today = new Date();
        const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
        const lastDay = new Date();
        const firstStr = firstDay.toISOString().split('T')[0];
        const lastStr = lastDay.toISOString().split('T')[0];

        if (fromDateInput) {
            fromDateInput.setAttribute('data-latin-date', firstStr);
            fromDateInput.value = toArabicDigits(firstStr);
            if (fpFrom) fpFrom.setDate(firstStr);
        }
        if (toDateInput) {
            toDateInput.setAttribute('data-latin-date', lastStr);
            toDateInput.value = toArabicDigits(lastStr);
            if (fpTo) fpTo.setDate(lastStr);
        }
        showTodaySetNotification();
    };

    window.resetForm = function () {
        $('#searchForm')[0].reset();
        $('.form-select').val('').trigger('change');
        const today = new Date();
        const thirtyDaysAgo = new Date();
        thirtyDaysAgo.setDate(today.getDate() - 30);
        const todayStr = today.toISOString().split('T')[0];
        const thirtyStr = thirtyDaysAgo.toISOString().split('T')[0];

        if (fromDateInput) {
            fromDateInput.setAttribute('data-latin-date', thirtyStr);
            fromDateInput.value = toArabicDigits(thirtyStr);
            if (fpFrom) fpFrom.setDate(thirtyStr);
        }
        if (toDateInput) {
            toDateInput.setAttribute('data-latin-date', todayStr);
            toDateInput.value = toArabicDigits(todayStr);
            if (fpTo) fpTo.setDate(todayStr);
        }
        $('.btn-clear').hide();
        showTodaySetNotification();
    };

    // [Include all remaining table functionality from your original search code...]

    // Initialize everything
    convertExistingContentToArabic();

    // Set default date range if not already set
    const today = new Date();
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(today.getDate() - 30);
    const todayStr = today.toISOString().split('T')[0];
    const thirtyStr = thirtyDaysAgo.toISOString().split('T')[0];

    if (fromDateInput && !fromDateInput.value) {
        fromDateInput.setAttribute('data-latin-date', thirtyStr);
        fromDateInput.value = toArabicDigits(thirtyStr);
        if (fpFrom) fpFrom.setDate(thirtyStr);
    }
    if (toDateInput && !toDateInput.value) {
        toDateInput.setAttribute('data-latin-date', todayStr);
        toDateInput.value = toArabicDigits(todayStr);
        if (fpTo) fpTo.setDate(todayStr);
    }

    console.log('✅ Search form ready with transaction form style calendar!');
    console.log('📅 Features: Same styling as transaction form, Today/Clear buttons, #198754 green colors');
});