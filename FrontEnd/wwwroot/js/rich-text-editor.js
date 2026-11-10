window.richTextEditor = (() => {
    const editors = Object.create(null);

    async function loadPersianLanguage() {
        try {
            const url = new URL('lib/jodit/fa.js', document.baseURI);
            const module = await import(url.href);
            return module.default || {};
        } catch (error) {
            console.warn('Jodit Persian language pack could not be loaded.', error);
            return {};
        }
    }

    async function init(id, dotnet, html) {
        const element = document.getElementById(id);
        if (!element || editors[id]) return;

        const api = window.Jodit && (window.Jodit.Jodit || window.Jodit);
        if (!api || typeof api.make !== 'function') {
            throw new Error('Jodit 4.19.10 is not loaded.');
        }

        const persian = await loadPersianLanguage();

        if (Object.keys(persian).length > 0 && api.lang) {
            api.lang.fa = persian;
        }

        const editor = api.make(element, {
            language: 'fa',
            direction: 'rtl',
            placeholder: element.dataset.placeholder || 'متن خود را اینجا بنویسید...',

            toolbar: true,
            toolbarAdaptive: true,
            toolbarButtonSize: 'middle',

            buttons: [
                'bold', 'italic', 'underline', 'strikethrough', '|',
                'ul', 'ol', '|',
                'font', 'fontsize', 'brush', 'paragraph', '|',
                'left', 'center', 'right', 'justify', '|',
                'link', 'image', 'table', '|',
                'undo', 'redo', 'eraser', '|',
                'hr', 'fullsize', 'source'
            ],

            askBeforePasteFromWord: false,
            processPasteFromWord: true,
            askBeforePasteHTML: true,
            defaultActionOnPaste: 'insert_as_html',

            showCharsCounter: false,
            showWordsCounter: false,
            showXPathInStatusbar: false,
            hidePoweredByJodit: true,

            spellcheck: true,
            autofocus: false,
            minHeight: 260,
            height: 360,
            enter: 'p',
uploader: {
                insertImageAsBase64URI: false
            }
        });

        editors[id] = { editor, dotnet };
        editor.value = html || '';

        editor.events.on('change', () => {
            const item = editors[id];
            if (!item) return;

            item.dotnet.invokeMethodAsync(
                'OnEditorInput',
                item.editor.value || ''
            ).catch(error => console.error('Editor update failed.', error));
        });
    }

    function setContent(id, html) {
        const item = editors[id];
        if (!item) return;

        const value = html || '';
        if (item.editor.value !== value) {
            item.editor.value = value;
        }
    }

    function dispose(id) {
        const item = editors[id];
        if (!item) return;

        item.editor.destruct();
        delete editors[id];
    }

    return { init, setContent, dispose };
})();