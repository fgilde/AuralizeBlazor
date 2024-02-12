class Auralizer {
    elementRef;
    dotnet;
    audioMotion;
    _featuresPaused = false;

    constructor(elementRef, dotNet, options) {
        this.elementRef = elementRef;
        this.dotnet = dotNet;
        this.options = options;
        this.createVisualizer(options);
    }

    createVisualizer(options) {
        this.audioMotion = new (window.AudioMotionAnalyzer || (window.AudioMotionAnalyzer = options.audioMotion.default))(
            this.visualizer = options.visualizer,
            this.options = this.prepareOptions(options)
        );
        this.visualizer.addEventListener('click', this.onVisualizerClick.bind(this));
        this.visualizer.addEventListener('dblclick', this.onVisualizerDblClick.bind(this));
        this.visualizer.addEventListener('contextmenu', this.onVisualizerCtxMenu.bind(this));
        this.reconnectInputs();
        this.dotnet.invokeMethodAsync('HandleOnCreated');

        this.clickTimeout = null;
        this.preventSingleClick = false;
    }

    async onVisualizerCtxMenu(e) {
        await this.handleAction(this.options.visualizerCtxMenuAction, e);
    }

    async onVisualizerDblClick(e) {
        this.preventSingleClick = true;
        if (this.clickTimeout) {
            clearTimeout(this.clickTimeout);
        }
        await this.handleAction(this.options.visualizerDblClickAction, e);
    }

    async onVisualizerClick(e) {
        this.clickTimeout = setTimeout(async () => {
            if (!this.preventSingleClick) {
                await this.handleAction(this.options.visualizerClickAction, e);
            }
            this.preventSingleClick = false;
        }, 300); 
    }


    async handleAction(action, e) {
        if (action === 0) return;
        if (e.preventDefault) {
            e.preventDefault();
        }
        switch (action) {
            case 1: // Pause/Resume
                if (!this.pauseAllActive()) {
                    if(!this.playAllActive(true)) {
                        this.playAllActive();
                    }
                }
                break;
            case 2: // Mute/Unmute
                // TODO: implement
                break;
            case 3: // Pause/Resume Features
                this._featuresPaused = !this._featuresPaused;
                break;
            case 4: // Picture-in-Picture
                this.togglePip();
                break;
            case 5: // Fullscreen
                this.toggleFullscreen();
                break;
            case 6: // Toggle Microphone
                this.connectToMic(!this.micStream);
                break;
            case 7: // Next preset
                await this.dotnet.invokeMethodAsync('NextPresetAsync');
                break;
            case 8: // Previous preset
                await this.dotnet.invokeMethodAsync('PreviousPresetAsync');
                break;
            default:
                break;
        }
    }

    invokeMethod(namespaceString, methodName, ...args) {
        const namespaceParts = namespaceString.split('.');
        let context = window;

        for (const part of namespaceParts.slice(0, -1)) {
            context = context[part];
            if (!context) return;
        }

        context = context[namespaceParts[namespaceParts.length - 1]];
        if (!context) return;
        const func = context[methodName];
        if (func && typeof func === 'function') {
            func.apply(context, [context].concat(args));
        }
    }


    prepareOptions(options) {
        options.fsElement = options.fsElement || options.visualizer;
        options.gradientLeft = options.gradientLeft || options.gradient;
        options.gradientRight = options.gradientRight || options.gradient;
        if (options.backgroundImage) {
            this.visualizer.style.backgroundImage = `url('${options.backgroundImage}')`;
            this.visualizer.style.backgroundSize = options.backgroundSize || 'cover';
            this.visualizer.style.backgroundRepeat = options.backgroundRepeat || 'no-repeat';
            this.visualizer.style.backgroundPosition = options.backgroundPosition || 'center';
        }

        if (options.features) { // TODO: Currently not helpful
            options.features.forEach(feature => this._ensureNamespace(feature.jsNamespace));
        }

        options.gradient = this.registerGradientIfRequired(options.gradient);
        options.gradientLeft = this.registerGradientIfRequired(options.gradientLeft);
        options.gradientRight = this.registerGradientIfRequired(options.gradientRight);

        options.onCanvasDraw = (instance, info) => {
            if (!this._featuresPaused && options.features) {
                options.features.forEach(feature => {
                    const methodName = feature.onCanvasDrawCallbackName;
                    const namespace = feature.jsNamespace;
                    this.invokeMethod(namespace, methodName, this, feature.options || {}, instance, info);
                });
            }
        };

        options.overlay = true;
        options.overlay = options.overlay || options.backgroundImage;

        return options;
    }

    registerGradientIfRequired(gradient) {
        if (!gradient) return 'classic';
        if (typeof gradient === 'string') return gradient;
        if (!gradient.name) return 'classic';
        const name = gradient.name.toLowerCase();
        if (this.audioMotion?._gradients?.[name] || !gradient.colorStops || gradient.isPredefined) return name;
        this.audioMotion.registerGradient(name, gradient);
        return name;
    }

    connectToMic(connect) {
        if (!connect && this.micStream) {
            this.audioMotion.disconnectInput(this.micStream, true); // disconnect mic stream and release audio track
            this.audioMotion.connectOutput();
            this.micStream = null;
        }
        else if (connect && !this.micStream) {
            navigator.mediaDevices.getUserMedia({ audio: true })
                .then(stream => {
                    this.micStream = this.audioMotion.audioCtx.createMediaStreamSource(stream);
                    this.audioMotion.disconnectOutput();
                    this.audioMotion.connectInput(this.micStream);
                })
                .catch(err => console.log('Error accessing user microphone.'));
        }
    }

    playAllActive(autoPausedOnly) {
        let hasPlayed = false;
        this.audioMotion?.connectedSources?.forEach(source => {
            if (source.mediaElement && source.mediaElement.paused) {
                if (autoPausedOnly && !source.mediaElement.__autoPaused) return;
                source.mediaElement.play();
                delete source.mediaElement.__autoPaused;
                hasPlayed = true;
            }
        });
        return hasPlayed;
    }

    pauseAllActive() {
        let hasPaused = false;
        this.audioMotion?.connectedSources?.forEach(source => {
            if (source.mediaElement && !source.mediaElement.paused) {
                source.mediaElement.__autoPaused = true;
                source.mediaElement.pause();
                hasPaused = true;
            }
        });
        return hasPaused;
    }

    disconnectInputs() {
        this.audioMotion?.connectedSources?.forEach(source => {
            this.audioMotion.disconnectInput(source.gain);
            // Assuming you have stored references to listeners in source object
            if (source.mediaElement && source.mediaElement.listeners) {
                Object.keys(source.mediaElement.listeners).forEach(event => {
                    source.mediaElement.removeEventListener(event, source.mediaElement.listeners[event]);
                });
                source.mediaElement.listeners = null;
                if (!source.mediaElement.paused) {
                    this.dotnet.invokeMethodAsync('HandleOnPause');
                }
                this.dotnet.invokeMethodAsync('HandleOnInputDisconnected');
            }
        });
    }


    reconnectInputs() {
        const audioCtx = this.audioMotion.audioCtx;
        const owner = this.options?.queryOwner?.querySelectorAll ? this.options.queryOwner : document;
        const audioElements = this.options.connectAll ? Array.from(owner.querySelectorAll('audio, video')) : this.options.audioElements;

        this.disconnectInputs();

        this.connectToMic(this.options.connectMicrophone);

        audioElements?.forEach(el => {
            if (el && (el['__internalId'] === undefined || el['__internalId'])) {
                if (!el.audioSourceNode) {
                    el.audioSourceNode = audioCtx.createMediaElementSource(el);
                    const gainNode = audioCtx.createGain();
                    el.audioSourceNode.connect(gainNode);
                    gainNode.connect(audioCtx.destination);
                }
                this.audioMotion.connectInput(el.audioSourceNode);
                if (!el.listeners) {
                    el.listeners = {
                        play: () => this.dotnet.invokeMethodAsync('HandleOnPlay'),
                        pause: () => this.dotnet.invokeMethodAsync('HandleOnPause'),
                        ended: () => this.dotnet.invokeMethodAsync('HandleOnEnded')
                    };
                    Object.keys(el.listeners).forEach(event => {
                        el.addEventListener(event, el.listeners[event]);
                    });
                }
                if (!el.paused) {
                    this.dotnet.invokeMethodAsync('HandleOnPlay');
                }
                this.dotnet.invokeMethodAsync('HandleOnInputConnected');
            }
        });
    }


    removeAllMediaElementListeners() {
        const owner = this.options?.queryOwner?.querySelectorAll ? this.options.queryOwner : document;
        const mediaElements = Array.from(owner.querySelectorAll('audio, video'));
        mediaElements.forEach(el => {
            if (el.listeners) {
                Object.keys(el.listeners).forEach(event => {
                    el.removeEventListener(event, el.listeners[event]);
                });
                el.listeners = null;
            }
        });
    }



    setOptions(options) {
        this.audioMotion.setOptions(this.options = this.prepareOptions(options));
        this.reconnectInputs();
    }

    async togglePip() {
        if (document.pictureInPictureEnabled) {
            if (!document.pictureInPictureElement) {
                this._tmpVideoElement = document.createElement('video');
                this._tmpVideoElement.style.display = 'none';
                document.body.appendChild(this._tmpVideoElement);

                const stream = this.audioMotion.canvas.captureStream();
                this._tmpVideoElement.srcObject = stream;

                await this._tmpVideoElement.play();
                await this._tmpVideoElement.requestPictureInPicture();
                return true;
            } else {
                if (this._tmpVideoElement) {
                    this._tmpVideoElement.remove();
                }
                await document.exitPictureInPicture();
                return false;
            }
        } else {
            console.warn('Picture-in-Picture-Mode not supported by browser');
            return false;
        }
    }

    _ensureNamespace(ns) {
        var parts = ns.split('.');
        var parent = window;
        for (var i = 0; i < parts.length; i++) {
            var part = parts[i];

            if (!parent[part] || typeof parent[part] === 'undefined') {
                parent[part] = {};
            }

            parent = parent[part];
        }
    }

    toggleFullscreen() {
        this.audioMotion.toggleFullscreen();
        return this.audioMotion.isFullscreen;
    }

    dispose() {
        this.visualizer.removeEventListener('click', this.onVisualizerClick.bind(this));
        this.disconnectInputs();
        this.audioMotion.destroy();
    }
}

window.Auralizer = Auralizer;
window.AuralizeBlazor = {
    features: {}
}

export function initializeAuralizer(elementRef, dotnet, options) {
    return new Auralizer(elementRef, dotnet, options);
}