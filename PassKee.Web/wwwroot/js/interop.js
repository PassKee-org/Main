window.PassKeeInterop = {
    getLocationHash: function () {
        return window.location.hash || '';
    },
    copyToClipboard: async function (text, clearAfterSeconds = 60) {
        if (!navigator.clipboard || !text) {
            return false;
        }
        await navigator.clipboard.writeText(text);
        if (clearAfterSeconds > 0) {
            setTimeout(async () => {
                try {
                    const currentText = await navigator.clipboard.readText();
                    if (currentText === text) {
                        await navigator.clipboard.writeText('');
                    }
                } catch {
                    // Clipboard read may be blocked if tab is inactive; safe to ignore
                }
            }, clearAfterSeconds * 1000);
        }
        return true;
    },
    downloadFileFromStream: async (fileName, contentStreamReference) => {
        const arrayBuffer = await contentStreamReference.arrayBuffer();
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName ?? '';
        anchorElement.click();
        anchorElement.remove();
        URL.revokeObjectURL(url);
    },
    inactivityTracker: {
        _dotNetHelper: null,
        _activityHandler: null,
        _visibilityHandler: null,
        init: function (dotNetHelper) {
            this.dispose();
            this._dotNetHelper = dotNetHelper;
            this._activityHandler = () => {
                if (this._dotNetHelper) {
                    this._dotNetHelper.invokeMethodAsync('OnUserClickActivity');
                }
            };
            this._visibilityHandler = () => {
                if (this._dotNetHelper) {
                    if (document.visibilityState === 'hidden') {
                        // When user leaves tab, notify inactivity service
                        this._dotNetHelper.invokeMethodAsync('OnTabHiddenActivity');
                    } else if (document.visibilityState === 'visible') {
                        // When user returns to tab, check if session exceeded timeout
                        this._dotNetHelper.invokeMethodAsync('OnTabVisibleActivity');
                    }
                }
            };

            window.addEventListener('click', this._activityHandler, { capture: true, passive: true });
            window.addEventListener('keydown', this._activityHandler, { capture: true, passive: true });
            window.addEventListener('touchstart', this._activityHandler, { capture: true, passive: true });
            document.addEventListener('visibilitychange', this._visibilityHandler, { passive: true });
        },
        dispose: function () {
            if (this._activityHandler) {
                window.removeEventListener('click', this._activityHandler, { capture: true });
                window.removeEventListener('keydown', this._activityHandler, { capture: true });
                window.removeEventListener('touchstart', this._activityHandler, { capture: true });
                this._activityHandler = null;
            }
            if (this._visibilityHandler) {
                document.removeEventListener('visibilitychange', this._visibilityHandler);
                this._visibilityHandler = null;
            }
            this._dotNetHelper = null;
        }
    },
    viewportTracker: {
        _dotNetHelper: null,
        _mediaQuery: null,
        _changeHandler: null,
        init: function (dotNetHelper) {
            this.dispose();
            this._dotNetHelper = dotNetHelper;
            this._mediaQuery = window.matchMedia('(max-width: 768px)');
            this._changeHandler = (e) => {
                if (this._dotNetHelper) {
                    this._dotNetHelper.invokeMethodAsync('OnBrowserResize', e.matches);
                }
            };
            if (this._mediaQuery.addEventListener) {
                this._mediaQuery.addEventListener('change', this._changeHandler);
            } else if (this._mediaQuery.addListener) {
                this._mediaQuery.addListener(this._changeHandler);
            }
            return this._mediaQuery.matches;
        },
        dispose: function () {
            if (this._mediaQuery && this._changeHandler) {
                if (this._mediaQuery.removeEventListener) {
                    this._mediaQuery.removeEventListener('change', this._changeHandler);
                } else if (this._mediaQuery.removeListener) {
                    this._mediaQuery.removeListener(this._changeHandler);
                }
            }
            this._changeHandler = null;
            this._mediaQuery = null;
            this._dotNetHelper = null;
        }
    },
    initViewportListener: function (dotNetHelper) {
        return this.viewportTracker.init(dotNetHelper);
    },
    disposeViewportListener: function () {
        this.viewportTracker.dispose();
    }
};
