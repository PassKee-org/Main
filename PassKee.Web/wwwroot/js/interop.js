window.PassKeeInterop = {
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
        _clickHandler: null,
        init: function (dotNetHelper) {
            this.dispose();
            this._dotNetHelper = dotNetHelper;
            this._clickHandler = () => {
                if (this._dotNetHelper) {
                    this._dotNetHelper.invokeMethodAsync('OnUserClickActivity');
                }
            };
            window.addEventListener('click', this._clickHandler, { capture: true, passive: true });
        },
        dispose: function () {
            if (this._clickHandler) {
                window.removeEventListener('click', this._clickHandler, { capture: true });
                this._clickHandler = null;
            }
            this._dotNetHelper = null;
        }
    }
};
