class BlazorAudioVisualizer {
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
    }

    async onVisualizerCtxMenu(e) {
        await this.handleAction(this.options.visualizerCtxMenuAction, e);
    }

    async onVisualizerDblClick(e) {
        await this.handleAction(this.options.visualizerDblClickAction, e);
    }

    async onVisualizerClick(e) {
        await this.handleAction(this.options.visualizerClickAction, e);
    }

    async handleAction(action, e) {
        if (action === 0) return;
        if (e.preventDefault) {
            e.preventDefault();
        }
        switch (action) {
            case 1: // Pause/Resume
                this.audioMotion.audioCtx.state === 'running' ? await this.audioMotion.audioCtx.suspend() : await this.audioMotion.audioCtx.state === 'suspended' && this.audioMotion.audioCtx.resume();
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

        options.onCanvasDraw = (instance, info) => {
            if (!this._featuresPaused && options.features) {
                options.features.forEach(feature => {
                    const methodName = feature.onCanvasDrawCallbackName;
                    const namespace = feature.jsNamespace;
                    this.invokeMethod(namespace, methodName, this, feature.options || {}, instance, info);
                });
            }
        };

        options.overlay = options.overlay || options.backgroundImage; // TODO: check if this is correct

        return options;
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

    disconnectInputs() {
        this.audioMotion?.connectedSources?.forEach(source => {
            this.audioMotion.disconnectInput(source.gain);
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
                //this.audioMotion.connectInput(el);
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

window.BlazorAudioVisualizer = BlazorAudioVisualizer;
window.AuralizeBlazor = {
    features: {}
}

export function initializeBlazorAudioVisualizer(elementRef, dotnet, options) {
    return new BlazorAudioVisualizer(elementRef, dotnet, options);
}