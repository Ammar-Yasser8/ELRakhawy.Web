/**
 * Number Formatting Utilities for RakhawyTex
 * Handles automatic conversion between Latin and Arabic/Indian numerals based on context
 */

class NumberFormatter {
    constructor() {
        this.arabicDigits = '٠١٢٣٤٥٦٧٨٩';
        this.latinDigits = '0123456789';
        this.arabicRegex = /[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFF]/;
        this.numberRegex = /\d/g;
        this.arabicNumberRegex = /[٠١٢٣٤٥٦٧٨٩]/g;
        
        this.init();
    }
    
    init() {
        // Format numbers when DOM is ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.formatPageNumbers());
        } else {
            this.formatPageNumbers();
        }
        
        // Set up mutation observer for dynamic content
        this.setupMutationObserver();
        
        // Make utility functions globally available
        window.NumberFormatter = this;
        window.toArabicDigits = (str) => this.toArabicDigits(str);
        window.toLatinDigits = (str) => this.toLatinDigits(str);
        window.formatElementNumbers = (element) => this.formatElementNumbers(element);
    }
    
    /**
     * Convert Latin digits to Arabic/Indian digits
     */
    toArabicDigits(str) {
        if (typeof str !== 'string') return str;
        return str.replace(this.numberRegex, d => this.arabicDigits[d]);
    }
    
    /**
     * Convert Arabic/Indian digits to Latin digits
     */
    toLatinDigits(str) {
        if (typeof str !== 'string') return str;
        return str.replace(this.arabicNumberRegex, d => this.arabicDigits.indexOf(d));
    }
    
    /**
     * Check if text contains Arabic characters
     */
    isArabicText(text) {
        if (typeof text !== 'string') return false;
        return this.arabicRegex.test(text);
    }
    
    /**
     * Get surrounding text context for a text node
     */
    getSurroundingText(textNode) {
        const parent = textNode.parentElement;
        if (!parent) return '';
        
        let surroundingText = parent.textContent || '';
        
        // Check previous and next siblings
        const prevSibling = parent.previousElementSibling;
        const nextSibling = parent.nextElementSibling;
        
        if (prevSibling) {
            surroundingText = (prevSibling.textContent || '') + ' ' + surroundingText;
        }
        if (nextSibling) {
            surroundingText = surroundingText + ' ' + (nextSibling.textContent || '');
        }
        
        return surroundingText;
    }
    
    /**
     * Determine if numbers should be Arabic based on context
     */
    shouldBeArabic(textNode) {
        const parentElement = textNode.parentElement;
        
        // Check explicit language attributes
        const lang = parentElement.getAttribute('lang') || 
                    parentElement.closest('[lang]')?.getAttribute('lang') || 
                    document.documentElement.getAttribute('lang');
        
        if (lang === 'ar' || lang === 'arabic') {
            return true;
        } else if (lang === 'en' || lang === 'english') {
            return false;
        }
        
        // Auto-detect based on surrounding text
        const surroundingText = this.getSurroundingText(textNode);
        return this.isArabicText(surroundingText);
    }
    
    /**
     * Format numbers in a text node based on context
     */
    formatTextNode(textNode) {
        const text = textNode.textContent;
        
        // Skip if no numbers
        if (!this.numberRegex.test(text) && !this.arabicNumberRegex.test(text)) {
            return;
        }
        
        const shouldBeArabic = this.shouldBeArabic(textNode);
        let formattedText = text;
        
        if (shouldBeArabic) {
            formattedText = this.toArabicDigits(text);
        } else {
            formattedText = this.toLatinDigits(text);
        }
        
        // Only update if there's a change
        if (formattedText !== text) {
            textNode.textContent = formattedText;
        }
    }
    
    /**
     * Format all numbers in the document
     */
    formatPageNumbers() {
        const walker = document.createTreeWalker(
            document.body,
            NodeFilter.SHOW_TEXT,
            {
                acceptNode: (node) => {
                    // Skip script and style tags
                    if (node.parentElement.tagName === 'SCRIPT' || 
                        node.parentElement.tagName === 'STYLE') {
                        return NodeFilter.FILTER_REJECT;
                    }
                    // Only process nodes that contain numbers
                    if (this.numberRegex.test(node.textContent) || 
                        this.arabicNumberRegex.test(node.textContent)) {
                        return NodeFilter.FILTER_ACCEPT;
                    }
                    return NodeFilter.FILTER_REJECT;
                }
            }
        );
        
        const textNodes = [];
        let node;
        while (node = walker.nextNode()) {
            textNodes.push(node);
        }
        
        textNodes.forEach(textNode => this.formatTextNode(textNode));
    }
    
    /**
     * Format numbers in a specific element
     */
    formatElementNumbers(element) {
        if (!element) return;
        
        const walker = document.createTreeWalker(
            element,
            NodeFilter.SHOW_TEXT,
            {
                acceptNode: (node) => {
                    if (this.numberRegex.test(node.textContent) || 
                        this.arabicNumberRegex.test(node.textContent)) {
                        return NodeFilter.FILTER_ACCEPT;
                    }
                    return NodeFilter.FILTER_REJECT;
                }
            }
        );
        
        const textNodes = [];
        let node;
        while (node = walker.nextNode()) {
            textNodes.push(node);
        }
        
        textNodes.forEach(textNode => this.formatTextNode(textNode));
    }
    
    /**
     * Set up mutation observer for dynamic content
     */
    setupMutationObserver() {
        const observer = new MutationObserver((mutations) => {
            let shouldReformat = false;
            
            mutations.forEach((mutation) => {
                if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                    // Check if any added nodes contain numbers
                    mutation.addedNodes.forEach((node) => {
                        if (node.nodeType === Node.TEXT_NODE) {
                            if (this.numberRegex.test(node.textContent) || 
                                this.arabicNumberRegex.test(node.textContent)) {
                                shouldReformat = true;
                            }
                        } else if (node.nodeType === Node.ELEMENT_NODE) {
                            if (this.numberRegex.test(node.textContent) || 
                                this.arabicNumberRegex.test(node.textContent)) {
                                shouldReformat = true;
                            }
                        }
                    });
                }
            });
            
            if (shouldReformat) {
                setTimeout(() => this.formatPageNumbers(), 100);
            }
        });
        
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }
    
    /**
     * Format a specific number with proper locale
     */
    formatNumber(number, locale = 'ar-EG') {
        if (typeof number !== 'number') return number;
        
        try {
            return new Intl.NumberFormat(locale).format(number);
        } catch (e) {
            return number.toString();
        }
    }
    
    /**
     * Format currency with proper locale
     */
    formatCurrency(amount, currency = 'EGP', locale = 'ar-EG') {
        if (typeof amount !== 'number') return amount;
        
        try {
            return new Intl.NumberFormat(locale, {
                style: 'currency',
                currency: currency
            }).format(amount);
        } catch (e) {
            return amount.toString();
        }
    }
    
    /**
     * Force format numbers in an element to Arabic
     */
    forceArabic(element) {
        if (!element) return;
        
        const walker = document.createTreeWalker(
            element,
            NodeFilter.SHOW_TEXT,
            {
                acceptNode: (node) => {
                    if (this.numberRegex.test(node.textContent)) {
                        return NodeFilter.FILTER_ACCEPT;
                    }
                    return NodeFilter.FILTER_REJECT;
                }
            }
        );
        
        let node;
        while (node = walker.nextNode()) {
            const text = node.textContent;
            const formattedText = this.toArabicDigits(text);
            if (formattedText !== text) {
                node.textContent = formattedText;
            }
        }
    }
    
    /**
     * Force format numbers in an element to Latin
     */
    forceLatin(element) {
        if (!element) return;
        
        const walker = document.createTreeWalker(
            element,
            NodeFilter.SHOW_TEXT,
            {
                acceptNode: (node) => {
                    if (this.arabicNumberRegex.test(node.textContent)) {
                        return NodeFilter.FILTER_ACCEPT;
                    }
                    return NodeFilter.FILTER_REJECT;
                }
            }
        );
        
        let node;
        while (node = walker.nextNode()) {
            const text = node.textContent;
            const formattedText = this.toLatinDigits(text);
            if (formattedText !== text) {
                node.textContent = formattedText;
            }
        }
    }
}

// Initialize the number formatter
const numberFormatter = new NumberFormatter();

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = NumberFormatter;
}
