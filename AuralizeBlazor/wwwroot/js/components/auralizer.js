﻿class Auralizer {
    elementRef;
    dotnet;
    audioMotion;
    _featuresPaused = false;
    _createdAudioElements = [];
    visualizerAction;
    constructor(elementRef, dotNet, options, visualizerAction) {
        this.elementRef = elementRef;
        this.dotnet = dotNet;
        this.options = options;
        this.visualizerAction = visualizerAction;
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
        this.visualizer.addEventListener('mousemove', this.onVisualizerMouseMove.bind(this));
        this.reconnectInputs();
        this.dotnet.invokeMethodAsync('HandleOnCreated');

        this.clickTimeout = null;
        this.preventSingleClick = false;
    }

    async onVisualizerMouseMove(e) {
        
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
                if (!(await this.dotnet.invokeMethodAsync('ClickInAlreadyHandled'))) {
                    await this.handleAction(this.options.visualizerClickAction, e);
                }
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
            case this.visualizerAction.TogglePlayPause: // Pause/Resume
                if (!this.pauseAllActive()) {
                    if (!this.playAllActive(true)) {
                        this.playAllActive();
                    }
                }
                break;
            case 2: // Mute/Unmute
                // TODO: implement
                break;
            case this.visualizerAction.ToggleAllFeatures: // Pause/Resume Features
                this._featuresPaused = !this._featuresPaused;
                break;
            case this.visualizerAction.TogglePictureInPicture: // Picture-in-Picture
                this.togglePip();
                break;
            case this.visualizerAction.ToggleFullscreen: // Fullscreen
                this.toggleFullscreen();
                break;
            case this.visualizerAction.ToggleMicrophone: // Toggle Microphone
                this.connectToMic(!this.micStream);
                break;
            case this.visualizerAction.NextPreset: // Next preset
                await this.dotnet.invokeMethodAsync('NextPresetAsync', 3);
                break;
            case this.visualizerAction.PreviousPreset: // Previous preset
                await this.dotnet.invokeMethodAsync('PreviousPresetAsync', 3);
                break;
            case this.visualizerAction.NextTrack: // Next track
                await this.dotnet.invokeMethodAsync('NextTrackAsync', this.currentTrack());
                break;
            case this.visualizerAction.PreviousTrack: // Previous track
                await this.dotnet.invokeMethodAsync('PreviousTrackAsync', this.currentTrack());
                break;
            case this.visualizerAction.ToggleFullPage: // full page
                await this.dotnet.invokeMethodAsync('ToggleFullPage');
                break;
            case this.visualizerAction.DisplayActionMenu: // action menu
                await this.dotnet.invokeMethodAsync('ToggleActionMenu');
                break;
            case this.visualizerAction.DisplayTrackList:
                await this.dotnet.invokeMethodAsync('ToggleTrackList');
                break;
            case this.visualizerAction.DisplayPresetList:
                await this.dotnet.invokeMethodAsync('TogglePresetList');
                break;
            default:
                break;
        }
    }

    currentTrack() {
        const mediaEl = (this.audioMotion?.connectedSources ?? [])[0]?.mediaElement ?? (this.getAudioElements() ?? [])[0];
        return mediaEl?.src;
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
            const deviceId = this.options.microphoneDeviceId;
            console.log(deviceId);
            const audioOptions = deviceId ? { audio: { deviceId: { exact: deviceId } } } : { audio: true };
            navigator.mediaDevices.getUserMedia(audioOptions)
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

    getOwner() {
        return this.options?.queryOwner?.querySelectorAll ? this.options.queryOwner : document;
    }

    getAudioElements() {
        const result = (this.options.connectAll ? Array.from(this.getOwner().querySelectorAll('audio, video')) : this.options.audioElements) ?? [];
        return [...result, ...(this._createdAudioElements ?? [])];
    }

    reconnectInputs() {
        const audioCtx = this.audioMotion.audioCtx;
        const audioElements = this.getAudioElements();

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
        this.dotnet.invokeMethodAsync('UpdateCurrentTrack', this.currentTrack());
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

    playTrack(url) {
        const mediaEl = (this.audioMotion?.connectedSources ?? [])[0]?.mediaElement ?? (this.getAudioElements() ?? [])[0];
        if (mediaEl) {
            mediaEl.src = url;
            mediaEl.play();
        } else {
            const mediaEl = document.createElement('audio');
            this._createdAudioElements.push(mediaEl);
            this.reconnectInputs();
            mediaEl.src = url;
            mediaEl.play();
        }
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
        this.visualizer.removeEventListener('dblclick', this.onVisualizerDblClick.bind(this));
        this.visualizer.removeEventListener('contextmenu', this.onVisualizerCtxMenu.bind(this));
        this.visualizer.removeEventListener('mousemove', this.onVisualizerMouseMove.bind(this));

        this.disconnectInputs();
        this.audioMotion.destroy();
    }
}

window.Auralizer = Auralizer;
window.AuralizeBlazor = {
    features: {}
}

export function initializeAuralizer(elementRef, dotnet, options, visualizerActions) {
    return new Auralizer(elementRef, dotnet, options, visualizerActions);
}