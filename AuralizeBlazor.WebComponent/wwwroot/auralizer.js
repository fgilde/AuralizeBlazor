/*
 * AuralizeBlazor web component loader.
 *
 * Include it with a classic script tag (not type="module") from wherever the published files are hosted:
 *   <script src="https://your-host/auralizer/auralizer.js"></script>
 * and use the custom element anywhere on the page:
 *   <auralize-blazor-auralizer connect-all-audio-sources="true"></auralize-blazor-auralizer>
 *
 * The loader boots the Blazor WebAssembly runtime from the folder this file lives in,
 * so the embedding page does not need to know anything about Blazor.
 */
(() => {
    if (window.AuralizeWebComponent) return;

    const self = document.currentScript || [...document.scripts].find(s => /\/auralizer\.js(\?|$)/.test(s.src));
    const baseUrl = new URL('./', self.src).href;

    window.AuralizeWebComponent = {
        baseUrl,
        tagName: 'auralize-blazor-auralizer',
        /** Resolves when the custom element is registered and ready to use. */
        ready: null
    };

    const css = document.createElement('link');
    css.rel = 'stylesheet';
    css.href = baseUrl + 'AuralizeBlazor.WebComponent.styles.css';
    document.head.appendChild(css);

    // Properties assigned before the element is defined (e.g. el.trackList = [...] in an inline script)
    // would be shadowed on upgrade. Take them off and re-apply them through the element's setters.
    const define = customElements.define;
    customElements.define = function (name, ctor, options) {
        if (name !== window.AuralizeWebComponent.tagName) return define.call(this, name, ctor, options);
        customElements.define = define;
        const pending = [...document.getElementsByTagName(name)].map(el => {
            const props = { ...el };
            Object.keys(props).forEach(key => delete el[key]);
            return [el, props];
        });
        define.call(this, name, ctor, options);
        pending.forEach(([el, props]) => Object.assign(el, props));
    };

    window.AuralizeWebComponent.ready = new Promise((resolve, reject) => {
        if (window.Blazor) {
            reject(new Error('AuralizeBlazor: this page already runs Blazor. Use the <Auralizer> component directly instead of the web component.'));
            return;
        }
        const blazor = document.createElement('script');
        blazor.src = baseUrl + '_framework/blazor.webassembly.js';
        blazor.setAttribute('autostart', 'false');
        blazor.onerror = reject;
        blazor.onload = () => Blazor.start({
            // Only dotnet.js is requested relative to the page, everything else is already resolved relative to dotnet.js.
            loadBootResource: (type, name, defaultUri) => new URL(defaultUri, baseUrl).href,
            // Blazor would import the library initializers (_content/*.lib.module.js) relative to the embedding page.
            // Passing initializers skips that; the .NET runtime still imports them itself relative to dotnet.js.
            initializers: { beforeStart: [], afterStarted: [] }
        }).then(() => customElements.whenDefined(window.AuralizeWebComponent.tagName)).then(resolve, reject);
        document.head.appendChild(blazor);
    });
})();
