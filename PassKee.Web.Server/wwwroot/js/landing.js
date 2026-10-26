(function () {
    "use strict";

    function initVaultPreview() {
        var previewRoot = document.querySelector("[data-vault-preview]");
        if (!previewRoot) return;

        var tabs = previewRoot.querySelectorAll("[data-preview-tab]");
        var panels = previewRoot.querySelectorAll("[data-preview-panel]");

        tabs.forEach(function (tab) {
            tab.addEventListener("click", function () {
                var target = tab.getAttribute("data-preview-tab");
                tabs.forEach(function (t) {
                    var active = t.getAttribute("data-preview-tab") === target;
                    t.setAttribute("aria-selected", active ? "true" : "false");
                    if (active) {
                        t.classList.add("border-blue-200", "bg-blue-50/70");
                        t.classList.remove("border-slate-200/80", "bg-white");
                    } else {
                        t.classList.remove("border-blue-200", "bg-blue-50/70");
                        t.classList.add("border-slate-200/80", "bg-white");
                    }
                });

                panels.forEach(function (panel) {
                    if (panel.getAttribute("data-preview-panel") === target) {
                        panel.classList.remove("hidden");
                    } else {
                        panel.classList.add("hidden");
                    }
                });
            });
        });
    }

    function initPasswordGenerator() {
        var root = document.querySelector("[data-password-generator]");
        if (!root) return;

        var outputEl = root.querySelector("[data-pw-output]");
        var lengthInput = root.querySelector("[data-pw-length]");
        var lengthValEl = root.querySelector("[data-pw-length-value]");
        var upperInput = root.querySelector("[data-pw-upper]");
        var lowerInput = root.querySelector("[data-pw-lower]");
        var digitsInput = root.querySelector("[data-pw-digits]");
        var symbolsInput = root.querySelector("[data-pw-symbols]");
        var regenBtn = root.querySelector("[data-pw-regenerate]");
        var copyBtn = root.querySelector("[data-pw-copy]");
        var copyLabel = root.querySelector("[data-pw-copy-label]");
        var strengthText = root.querySelector("[data-pw-strength-text]");
        var crackInline = root.querySelector("[data-pw-crack-inline]");
        var meter = root.querySelector("[data-pw-meter]");

        var CHARSETS = {
            upper: "ABCDEFGHJKLMNPQRSTUVWXYZ",
            lower: "abcdefghijkmnopqrstuvwxyz",
            digits: "23456789",
            symbols: "!@#$%^&*()-_=+?"
        };

        function randomIndex(max) {
            var arr = new Uint32Array(1);
            window.crypto.getRandomValues(arr);
            return arr[0] % max;
        }

        function formatCrackTime(bits) {
            if (bits < 45) return "Minutes to hours";
            if (bits < 65) return "Several years";
            if (bits < 85) return "Millions of years";
            return "> 500 billion years";
        }

        function generatePassword() {
            var length = parseInt(lengthInput.value, 10) || 20;
            if (lengthValEl) lengthValEl.textContent = String(length);

            var pool = "";
            var requiredChars = [];

            if (upperInput && upperInput.checked) {
                pool += CHARSETS.upper;
                requiredChars.push(CHARSETS.upper[randomIndex(CHARSETS.upper.length)]);
            }
            if (lowerInput && lowerInput.checked) {
                pool += CHARSETS.lower;
                requiredChars.push(CHARSETS.lower[randomIndex(CHARSETS.lower.length)]);
            }
            if (digitsInput && digitsInput.checked) {
                pool += CHARSETS.digits;
                requiredChars.push(CHARSETS.digits[randomIndex(CHARSETS.digits.length)]);
            }
            if (symbolsInput && symbolsInput.checked) {
                pool += CHARSETS.symbols;
                requiredChars.push(CHARSETS.symbols[randomIndex(CHARSETS.symbols.length)]);
            }

            if (!pool) {
                if (lowerInput) lowerInput.checked = true;
                pool = CHARSETS.lower;
                requiredChars.push(CHARSETS.lower[randomIndex(CHARSETS.lower.length)]);
            }

            var chars = requiredChars.slice();
            while (chars.length < length) {
                chars.push(pool[randomIndex(pool.length)]);
            }

            for (var i = chars.length - 1; i > 0; i--) {
                var j = randomIndex(i + 1);
                var tmp = chars[i];
                chars[i] = chars[j];
                chars[j] = tmp;
            }

            var password = chars.join("");
            if (outputEl) outputEl.textContent = password;

            var entropy = Math.round(length * Math.log2(pool.length));
            var label = "Bulletproof";
            var widthPct = "100%";
            var colorClass = "bg-emerald-500";

            if (entropy < 60) {
                label = "Moderate";
                widthPct = "45%";
                colorClass = "bg-amber-500";
            } else if (entropy < 90) {
                label = "Strong";
                widthPct = "78%";
                colorClass = "bg-blue-600";
            }

            if (strengthText) {
                strengthText.textContent = label + " • " + entropy + " bits of entropy";
            }
            if (crackInline) {
                crackInline.textContent = "Crack time: " + formatCrackTime(entropy);
            }
            if (meter) {
                meter.style.width = widthPct;
                meter.className = "h-full rounded-full transition-all duration-300 " + colorClass;
            }
        }

        if (lengthInput) lengthInput.addEventListener("input", generatePassword);
        [upperInput, lowerInput, digitsInput, symbolsInput].forEach(function (el) {
            if (el) el.addEventListener("change", generatePassword);
        });
        if (regenBtn) regenBtn.addEventListener("click", generatePassword);

        if (copyBtn && outputEl) {
            copyBtn.addEventListener("click", function () {
                var text = outputEl.textContent || "";
                if (navigator.clipboard && navigator.clipboard.writeText) {
                    navigator.clipboard.writeText(text).then(function () {
                        if (copyLabel) {
                            var prev = copyLabel.textContent;
                            copyLabel.textContent = "Copied!";
                            setTimeout(function () {
                                copyLabel.textContent = prev;
                            }, 1800);
                        }
                    });
                }
            });
        }
    }

    function initHelpSearch() {
        var container = document.querySelector("[data-faq-container]");
        if (!container) return;

        var searchForm = document.querySelector("[data-help-search-form]");
        var searchInput = document.querySelector("[data-help-search-input]");
        var categoryLinks = document.querySelectorAll("[data-help-category]");
        var items = container.querySelectorAll("[data-faq-item]");
        var emptyState = document.querySelector("[data-faq-empty-state]");
        var resetBtn = document.querySelector("[data-faq-reset]");
        var countStatus = document.querySelector("[data-faq-count-status]");

        var supportsUntilFound = "onbeforematch" in HTMLElement.prototype;
        var activeCategory = "all";

        var urlParams = new URLSearchParams(window.location.search);
        if (urlParams.get("category")) {
            activeCategory = urlParams.get("category").toLowerCase();
        }

        function setHiddenState(el, shouldHide) {
            if (!shouldHide) {
                el.removeAttribute("hidden");
                el.classList.remove("hidden");
            } else if (supportsUntilFound) {
                el.setAttribute("hidden", "until-found");
            } else {
                el.removeAttribute("hidden");
            }
        }

        function updateCategoryPills() {
            categoryLinks.forEach(function (link) {
                var cat = link.getAttribute("data-help-category");
                var isActive = cat === activeCategory;
                if (isActive) {
                    link.className = "inline-flex items-center gap-2 rounded-xl border border-blue-600 bg-blue-600 px-3.5 py-2 text-xs sm:text-sm font-semibold text-white shadow-sm transition-colors cursor-pointer";
                } else {
                    link.className = "inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3.5 py-2 text-xs sm:text-sm font-medium text-slate-700 hover:border-blue-300 hover:bg-blue-50/50 transition-colors cursor-pointer";
                }
            });
        }

        function applyFilter() {
            var q = searchInput ? searchInput.value.trim().toLowerCase() : "";
            var visibleCount = 0;

            items.forEach(function (item) {
                var cat = item.getAttribute("data-faq-category") || "";
                var searchBlob = item.getAttribute("data-faq-search") || "";
                var matchesCat = activeCategory === "all" || cat === activeCategory;
                var matchesQuery = !q || searchBlob.indexOf(q) !== -1;

                if (matchesCat && matchesQuery) {
                    setHiddenState(item, false);
                    if (q) item.open = true;
                    visibleCount++;
                } else {
                    setHiddenState(item, true);
                }
            });

            if (emptyState) {
                if (visibleCount === 0) {
                    emptyState.classList.remove("hidden");
                } else {
                    emptyState.classList.add("hidden");
                }
            }

            if (countStatus) {
                countStatus.textContent = "Showing " + visibleCount + " of " + items.length + " practical answers";
            }
        }

        container.addEventListener("beforematch", function (e) {
            var matchedDetails = e.target.closest ? e.target.closest("details[data-faq-item]") : null;
            if (matchedDetails) {
                activeCategory = "all";
                if (searchInput) searchInput.value = "";
                updateCategoryPills();
                applyFilter();
                matchedDetails.open = true;
            }
        });

        if (searchForm) {
            searchForm.addEventListener("submit", function (e) {
                e.preventDefault();
                applyFilter();
            });
        }

        if (searchInput) {
            searchInput.addEventListener("input", applyFilter);
        }

        categoryLinks.forEach(function (link) {
            link.addEventListener("click", function (e) {
                e.preventDefault();
                activeCategory = link.getAttribute("data-help-category") || "all";
                updateCategoryPills();
                applyFilter();
            });
        });

        if (resetBtn) {
            resetBtn.addEventListener("click", function (e) {
                e.preventDefault();
                activeCategory = "all";
                if (searchInput) searchInput.value = "";
                updateCategoryPills();
                applyFilter();
            });
        }

        if (!supportsUntilFound) {
            document.querySelectorAll('[hidden="until-found"]').forEach(function (el) {
                el.removeAttribute("hidden");
            });
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", function () {
            initVaultPreview();
            initPasswordGenerator();
            initHelpSearch();
        });
    } else {
        initVaultPreview();
        initPasswordGenerator();
        initHelpSearch();
    }
})();

