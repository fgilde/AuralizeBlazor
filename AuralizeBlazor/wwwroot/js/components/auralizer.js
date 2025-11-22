class Auralizer {
    elementRef;
    dotnet;
    audioMotion;
    isSimulating = false;
    _featuresPaused = false;
    _createdAudioElements = [];
    connectedStreams = [];
    visualizerAction;
    constructor(elementRef, dotNet, options, visualizerAction) {
        this.elementRef = elementRef;
        this.dotnet = dotNet;
        this.options = options;
        this.visualizerAction = visualizerAction;
        this.createVisualizer(options);
        window['AuralizeBlazor'].instance = this;
    }

    createVisualizer(options) {
        this.audioMotion = new (window.AudioMotionAnalyzer || (window.AudioMotionAnalyzer = options.audioMotion.default))(
            this.visualizer = options.visualizer,
            this.options = this.prepareOptions(options)
        );
        if (this.visualizer) {
            this.visualizer.addEventListener('click', this.onVisualizerClick.bind(this));
            this.visualizer.addEventListener('dblclick', this.onVisualizerDblClick.bind(this));
            this.visualizer.addEventListener('contextmenu', this.onVisualizerCtxMenu.bind(this));
            this.visualizer.addEventListener('mousemove', this.onVisualizerMouseMove.bind(this));
        }
        this.reconnectInputs();
        this.dotnet.invokeMethodAsync('HandleOnCreated');

        this.clickTimeout = null;
        this.preventSingleClick = false;
        this.analyzer = this.audioMotion._analyzer[0];
        this.originalGetFloatFrequencyData = this.analyzer.getFloatFrequencyData.bind(this.analyzer);
        if (options.initialRender > 0) {
            this.renderOneTimeStatic();
        }
    }
    
    record({ duration = 5000, fps = 30 } = {}) {
        const audioCtx = this.audioMotion.audioCtx;
        const canvas = this.audioMotion.canvas;

        if (audioCtx.state === "suspended") {
            audioCtx.resume();
        }

        const canvasStream = canvas.captureStream(fps);
        const dest = audioCtx.createMediaStreamDestination();
        const masterGain = audioCtx.createGain();
        masterGain.gain.value = 1.0;
        masterGain.connect(dest);

        let hasAudio = false;

        const tryConnect = (node, name) => {
            try {
                node.connect(masterGain);
                console.log(`Audioquelle verbunden: ${name}`);
                hasAudio = true;
            } catch (e) {
                console.warn(`Fehler beim Verbinden von ${name}:`, e);
            }
        };

        if (this.connectedStreams && this.connectedStreams.length > 0) {
            this.connectedStreams.forEach((node, i) => {
                tryConnect(node, `connectedStreams[${i}]`);
            });
        }

        if (this.audioMotion.connectedSources && this.audioMotion.connectedSources.length > 0) {
            this.audioMotion.connectedSources.forEach((source, i) => {
                if (source.audioSourceNode) {
                    tryConnect(source.audioSourceNode, `connectedSources[${i}]`);
                }
            });
        }

        if (this.micStream) {s
            tryConnect(this.micStream, "micStream");
        }

        if (!hasAudio) {
            const mediaElements = this.getAudioElements();
            if (mediaElements && mediaElements.length > 0) {
                mediaElements.forEach((el, i) => {
                    // Wir verbinden nur Elemente, die gerade wiedergegeben werden (nicht pausiert)
                    if (!el.paused) {
                        try {
                            let audioNode;
                            // Verhindere, dass für dasselbe Element mehrfach ein MediaElementAudioSourceNode erstellt wird.
                            if (!el.__auralizerAudioSource) {
                                audioNode = audioCtx.createMediaElementSource(el);
                                el.__auralizerAudioSource = audioNode;
                            } else {
                                audioNode = el.__auralizerAudioSource;
                            }
                            tryConnect(audioNode, `getAudioElements[${i}]`);
                        } catch (e) {
                            console.warn(`Fehler beim Verbinden von MediaElement ${i}:`, e);
                        }
                    }
                });
            }
        }

        // Debug: Ausgabe der Audio-Tracks im Destination-Stream
        setTimeout(() => {
            console.log("Audio Destination Tracks:", dest.stream.getAudioTracks());
        }, 100);

        const combinedStream = new MediaStream([
            ...canvasStream.getVideoTracks(),
            ...dest.stream.getAudioTracks()
        ]);

        const options = {
            mimeType: "video/webm; codecs=vp9",
            videoBitsPerSecond: 8_000_000, // 8 Mbit/s
            audioBitsPerSecond: 320_000    // 320 kbit/s
        };

        const recordedChunks = [];
        let mediaRecorder;

        try {
            mediaRecorder = new MediaRecorder(combinedStream, options);
        } catch (e) {
            console.error("MediaRecorder konnte nicht erstellt werden:", e);
            return;
        }

        mediaRecorder.ondataavailable = (e) => {
            if (e.data && e.data.size > 0) {
                recordedChunks.push(e.data);
            }
        };

        mediaRecorder.onstop = () => {
            const blob = new Blob(recordedChunks, { type: options.mimeType });
            console.log("Aufgezeichneter Blob:", blob, "Größe:", blob.size);
            if (blob.size === 0) {
                console.warn("Der Blob ist leer – es wurde kein Audio oder Video aufgezeichnet.");
            }
            const url = URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = "recording.webm";
            document.body.appendChild(a);
            a.click();
            setTimeout(() => {
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
            }, 100);
        };

        mediaRecorder.start();

        setTimeout(() => {
            mediaRecorder.stop();
            console.log("Aufnahme gestoppt.");
        }, duration);
    }




    async renderOneTimeStatic() {
        const audioElements = this.getAudioElements();        
        //render enum 2 means real data and 3 full spectrum
        if (this.options.initialRender === 1 || this.options.initialRender === 6) {
            const frequencyData = this.generateRandomAudioFrequencyData();
            this.setAudioData(frequencyData);
        }
        if ((this.options.initialRender === 2 || this.options.initialRender === 3 || this.options.initialRender === 6) && audioElements.length > 0) {
            const frequencyDataArray = await Promise.all(audioElements.map(async (audioElement) => {
                return await (this.options.initialRender === 2 ? this.getRealAudioFrequencyData(audioElement) : this.getFullAudioSpectrumFrequencyData(audioElement, true));
            }));
            const combinedFrequencyData = this.combineFrequencyData(frequencyDataArray);
            this.setAudioData(combinedFrequencyData);
        }
        if (this.options.initialRender === 4) {
            this.simulateRandomAudioSpectrum();
        }
        if (this.options.initialRender === 5) {
            this.simulateFullAudioSpectrum(audioElements[0], 1, true);
        }
    }

    disableOutput() {
        this.audioMotion.volume = 0; // AT least important for stream connections
    }

    _captureStream = null;
    _isSelecting = false;
    async connectToCapture(preferCurrentTab = false) {
        if (this._isSelecting)
            return;
        try {
            if (!this._captureStream) {
                this._isSelecting = true;
                const constraints = {
                    audio: true, // oder { systemAudio: 'include' } in Chrome
                    displaySurface: 'browser',
                    selfBrowserSurface: 'include',
                    surfaceSwitching: 'include',
                    logicalSurface: true,
                    systemAudio: 'include',
                    preferCurrentTab: preferCurrentTab
                };
                this._captureStream = await navigator.mediaDevices.getDisplayMedia(constraints);
            }
            const source = this.audioMotion.audioCtx.createMediaStreamSource(this._captureStream);
            source.stream = this._captureStream;

            const onInactiveHandler = () => {
                if (this._captureStream) {
                    this._captureStream.removeEventListener('inactive', onInactiveHandler);
                }
                const idx = this.connectedStreams.indexOf(source);
                if (idx >= 0) {
                    this.connectedStreams.splice(idx, 1);
                }
                this._captureStream = null;
                this.reconnectInputs();
            };
            this._captureStream.addEventListener('inactive', onInactiveHandler);
            this.connectedStreams.push(source);
            this.audioMotion.connectInput(source);
            this.disableOutput();
            if (!this.audioMotion.isOn) {
                this.audioMotion.toggleAnalyzer(true);
            }

        } catch (err) {
            console.error('getDisplayMedia failed or cancelled:', err);
        } finally {
            this._isSelecting = false;
        }
    }


    //#region Frequency Data helpers
    // TODO: Move to separate class

    setAudioData(frequencyData, oneTime = true) {
        this.analyzer.getFloatFrequencyData = this._tmpGetFloatFrequencyData = function (array) {
            array.set(frequencyData);
        };
        if (oneTime) {
            this.audioMotion.toggleAnalyzer(true);
        }
        setTimeout(() => {
            if (oneTime) {
                this.audioMotion.toggleAnalyzer(false);
                this.analyzer.getFloatFrequencyData = this.originalGetFloatFrequencyData;
            }
        }, 100); // Delay in milliseconds (adjust as needed)
    }


    combineFrequencyData(frequencyDataArray) {
        const length = frequencyDataArray[0].length;
        const combinedData = new Float32Array(length);

        frequencyDataArray.forEach(data => {
            for (let i = 0; i < length; i++) {
                combinedData[i] += data[i];
            }
        });

        // Normalize the combined data
        for (let i = 0; i < length; i++) {
            combinedData[i] /= frequencyDataArray.length;
        }

        return combinedData;
    }

    generateRandomAudioFrequencyData() {
        const smoothingFactor = 0.6; // Adjust this value to control smoothing level
        let previousValue = Math.random() * 20 - 80; // Initial random value in a lower range

        const frequencyData = new Float32Array(this.audioMotion._analyzer[0].frequencyBinCount);
        for (let i = 0; i < frequencyData.length; i++) {
            const randomValue = Math.random() * 30 - 80; // Random value between -80 and -50 dB
            const smoothedValue = previousValue * smoothingFactor + randomValue * (1 - smoothingFactor);
            frequencyData[i] = smoothedValue;
            previousValue = smoothedValue;
        }
        return frequencyData;
    }


    async getRealAudioFrequencyData(audioElementOrFile) {
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const fileUrl = typeof audioElementOrFile === 'string' ? audioElementOrFile : audioElementOrFile.src;
        const response = await fetch(fileUrl);
        const arrayBuffer = await response.arrayBuffer();
        const audioBuffer = await audioContext.decodeAudioData(arrayBuffer);

        const offlineContext = new OfflineAudioContext(1, audioBuffer.length, audioContext.sampleRate);
        const source = offlineContext.createBufferSource();
        source.buffer = audioBuffer;

        const analyzer = offlineContext.createAnalyser();
        analyzer.fftSize = this.options.fftSize;
        source.connect(analyzer);
        analyzer.connect(offlineContext.destination);

        source.start();
        await offlineContext.startRendering();

        const frequencyData = new Float32Array(analyzer.frequencyBinCount);
        analyzer.getFloatFrequencyData(frequencyData);

        return frequencyData;
    }

    async getFullAudioSpectrumFrequencyData(audioElementOrFile, aggregate = false) {
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const fileUrl = typeof audioElementOrFile === 'string' ? audioElementOrFile : audioElementOrFile.src;
        const response = await fetch(fileUrl);
        const arrayBuffer = await response.arrayBuffer();
        const audioBuffer = await audioContext.decodeAudioData(arrayBuffer);

        const frameSize = this.options.fftSize || 2048;
        const frameStep = frameSize / 4; // Overlap frames by 25%
        const frames = Math.ceil(audioBuffer.length / frameStep);
        const frequencyDataArray = [];

        let aggregatedFrequencyData;
        const timestamps = [];

        if (aggregate) {
            aggregatedFrequencyData = new Float32Array(frameSize / 2);
        }

        for (let i = 0; i < frames; i++) {
            const frameStart = i * frameStep;
            const frameEnd = frameStart + frameSize;

            // Calculate the timestamp for this frame
            const timestamp = (frameStart / audioBuffer.sampleRate) * 1000; // Convert to milliseconds
            timestamps.push(timestamp);

            // Create a temporary buffer to hold the current frame
            const tempBuffer = audioBuffer.getChannelData(0).slice(frameStart, frameEnd);
            const offlineContext = new OfflineAudioContext(1, tempBuffer.length, audioContext.sampleRate);
            const tempAudioBuffer = offlineContext.createBuffer(1, tempBuffer.length, offlineContext.sampleRate);
            tempAudioBuffer.copyToChannel(tempBuffer, 0);

            // Create a temporary source for the current frame
            const tempSource = offlineContext.createBufferSource();
            tempSource.buffer = tempAudioBuffer;

            const analyzer = offlineContext.createAnalyser();
            analyzer.fftSize = frameSize;
            tempSource.connect(analyzer);
            analyzer.connect(offlineContext.destination);

            tempSource.start();
            await offlineContext.startRendering();

            const frequencyData = new Float32Array(analyzer.frequencyBinCount);
            analyzer.getFloatFrequencyData(frequencyData);

            if (aggregate) {
                // Aggregate frequency data
                for (let j = 0; j < frequencyData.length; j++) {
                    aggregatedFrequencyData[j] += frequencyData[j];
                }
            } else {
                frequencyDataArray.push(frequencyData);
            }
        }

        if (aggregate) {
            // Normalize the aggregated frequency data
            for (let j = 0; j < aggregatedFrequencyData.length; j++) {
                aggregatedFrequencyData[j] /= frames;
            }
            //return { frequencyDataArray: [aggregatedFrequencyData], timestamps: [0] };
            return aggregatedFrequencyData;
        }

        return { frequencyDataArray, timestamps };
    }

    isSimulationRunning() {
        return this.isSimulating;
    }

    stopSimulation() {
        if (!this.isSimulating)
            return;
        this.isSimulating = false;
        this._simulationStopped = true;
        if (this._simulateId) {
            clearInterval(this._simulateId);
        }
        setTimeout(() => this.audioMotion.toggleAnalyzer(true), 100);
    }

    simulateRandomAudioSpectrum() {
        if (this.isSimulating)
            return;
        this.isSimulating = true;
        this._simulateId = setInterval(() => {
            if (!this.simulationIsPaused()) {
                requestAnimationFrame(() => this.setAudioData(this.generateRandomAudioFrequencyData(), false));
            }
        }, 10);
    }

    simulateRandomAudioSpectrumContinueWithFullSpectrum(audioElementOrFileOrData, speedFactor = 1, endless = false) {
        if (!audioElementOrFileOrData)
            audioElementOrFileOrData = this.getAudioElements()[0];
        if (audioElementOrFileOrData) {
            const task = 'frequencyDataArray' in audioElementOrFileOrData
                ? Promise.resolve(audioElementOrFileOrData)
                : this.getFullAudioSpectrumFrequencyData(audioElementOrFileOrData, false);

            task.then(data => {
                //this.stopSimulation(); // Stop the random simulation
                clearInterval(this._simulateId);
                this.simulateFullAudioSpectrum(data, speedFactor, endless); // Start the full spectrum simulation
            });
        }
        this.simulateRandomAudioSpectrum(); // Start the random simulation
    }



    async simulateFullAudioSpectrum(audioElementOrFileOrData, speedFactor = 1, endless = false) {
        if (!audioElementOrFileOrData)
            audioElementOrFileOrData = this.getAudioElements()[0];
        if (!audioElementOrFileOrData)
            return Promise.resolve();
        const { frequencyDataArray, timestamps } = 'frequencyDataArray' in audioElementOrFileOrData ? audioElementOrFileOrData : await this.getFullAudioSpectrumFrequencyData(audioElementOrFileOrData, false);
        const analyzer = this.analyzer;
        const originalGetFloatFrequencyData = this.originalGetFloatFrequencyData;

        this.isSimulating = true;
        const totalFrames = frequencyDataArray.length;
        let currentFrame = 0;
        let accumulatedPauseTime = 0;
        let lastPauseTime = 0;
        let startTime = performance.now();

        const updateFrame = () => {
            if (this.simulationIsPaused()) {
                if (lastPauseTime === 0) {
                    lastPauseTime = performance.now();
                }
                requestAnimationFrame(updateFrame);
                return;
            }

            if (lastPauseTime !== 0) {
                accumulatedPauseTime += performance.now() - lastPauseTime;
                lastPauseTime = 0;
            }

            const currentTime = performance.now() - startTime - accumulatedPauseTime;
            while (currentFrame < totalFrames && timestamps[currentFrame] <= currentTime * speedFactor) {
                currentFrame++;
            }

            requestAnimationFrame(updateFrame);
        };

        return new Promise((resolve) => {
            analyzer.getFloatFrequencyData = this._tmpGetFloatFrequencyData = function (array) {
                originalGetFloatFrequencyData(array);
                if (currentFrame < totalFrames) {
                    array.set(frequencyDataArray[currentFrame]);
                } else {
                    array.fill(-Infinity); // Fill with silence after last frame
                }
            };

            this.audioMotion.toggleAnalyzer(true);
            requestAnimationFrame(updateFrame);

            const checkCompletion = () => {
                if (currentFrame >= totalFrames || this._simulationStopped) {
                    if (endless && !this._simulationStopped) {
                        currentFrame = 0;
                        startTime = performance.now();
                        accumulatedPauseTime = 0;
                        lastPauseTime = 0;
                        setTimeout(checkCompletion, 100); // Restart after 100ms
                        return;
                    }
                    this._simulationStopped = false;
                    analyzer.getFloatFrequencyData = originalGetFloatFrequencyData;
                    this._tmpGetFloatFrequencyData = null;
                    this.isSimulating = false;
                    resolve();
                } else {
                    setTimeout(checkCompletion, 100); // Check completion every 100ms
                }
            };
            checkCompletion();
        });
    }

    simulationIsPaused() {
        return this._simulationPaused;
    }

    pauseSimulation(keepState) {
        if (this.isSimulating) {
            this._simulationPaused = true;
            this.analyzer.getFloatFrequencyData = this.originalGetFloatFrequencyData;
            if (!keepState) {
                this.audioMotion.toggleAnalyzer(false);
            }
        }
    }

    resumeSimulation() {
        if (this.isSimulating) {
            if (this._tmpGetFloatFrequencyData) {
                this.analyzer.getFloatFrequencyData = this._tmpGetFloatFrequencyData;
            }
            this._simulationPaused = false;
            if (!this.audioMotion.isOn) {
                this.audioMotion.toggleAnalyzer(true);
            }
        }
    }


    //#endregion

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
        if (options.backgroundImageToApply) {
            this.visualizer.style.backgroundImage = `url('${options.backgroundImageToApply}')`;
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

        //options.overlay = true;
        options.overlay = options.overlay || options.backgroundImageToApply;

        return options;
    }

    getCurrentColors() {
        return [...new Set(this.getColorsFromGradient(this.audioMotion.gradient)
            .concat(this.getColorsFromGradient(this.audioMotion.gradientLeft))
            .concat(this.getColorsFromGradient(this.audioMotion.gradientRight)))];
    }

    getCurrentColorStops() {
        return [...new Set(this.getColorStopsFromGradient(this.audioMotion.gradient)
            .concat(this.getColorStopsFromGradient(this.audioMotion.gradientLeft))
            .concat(this.getColorStopsFromGradient(this.audioMotion.gradientRight)))];
    }

    getColorsFromGradient(gradientName) {
        return this.getColorStopsFromGradient(gradientName).map(c => c.color);
    }

    getColorStopsFromGradient(gradientName) {
        if (!gradientName)
            return [];
        var colors = this.audioMotion._gradients[gradientName]?.colorStops;
        if (!colors)
            return [];
        return colors;
    }

    updateGradient(gradient, gradientLeft, gradientRight) {
        this.audioMotion.setOptions({
            gradient: this.registerGradientIfRequired(gradient),
            gradientLeft: this.registerGradientIfRequired(gradientLeft),
            gradientRight: this.registerGradientIfRequired(gradientRight)
        });
        if (this.audioMotion?.redraw)
            this.audioMotion.redraw();
    }

    registerGradientIfRequired(gradient) {
        if (!gradient) return 'classic';
        if (typeof gradient === 'string') return gradient;
        if (!this.audioMotion) return 'classic';
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
        let elements = this.audioMotion?.connectedSources?.filter(s => s.mediaElement).map(s => s.mediaElement);
        if (!elements || elements.length <= 0) {
            elements = this.getAudioElements();
        }
        elements?.forEach(mediaElement => {
            if (mediaElement && mediaElement.paused) {
                if (autoPausedOnly && !mediaElement.__autoPaused) return;
                mediaElement.play();
                delete mediaElement.__autoPaused;
                hasPlayed = true;
            }
        });
        return hasPlayed;
    }

    pauseAllActive() {
        let hasPaused = false;
        let elements = this.audioMotion?.connectedSources?.filter(s => s.mediaElement).map(s => s.mediaElement);
        if (!elements || elements.length <= 0) {            
            elements = this.getAudioElements();
        }
        elements?.forEach(mediaElement => {
            if (mediaElement && !mediaElement.paused) {
                mediaElement.__autoPaused = true;
                mediaElement.pause();
                hasPaused = true;
            }
        });
        return hasPaused;
    }

    disconnectInputs() {
        if (this.connectedStreams.length > 0) {
            for (const stream of this.connectedStreams) {
                try {
                    this.audioMotion.disconnectInput(stream);
                } catch (e) {
                    console.error('Error disconnecting stream from audioMotion:', e);
                }
            }
            this.connectedStreams = [];
        }

        this.audioMotion?.connectedSources?.forEach(source => {
            this.audioMotion.disconnectInput(source.gain);            
            if (source.mediaElement && source.mediaElement.listeners) {
                Object.keys(source.mediaElement.listeners).forEach(event => {
                    source.mediaElement.removeEventListener(event, source.mediaElement.listeners[event]);
                });
                source.mediaElement.listeners = null;
                if (!source.mediaElement.paused) {
                    this.dotnet.invokeMethodAsync('HandleOnPause');
                }                
            }
        });
        this.connectToMic(false);
    }

    _connectToStream(el) {
        if (!el.stream) {
            el.stream = el.captureStream();
        }
        const audioTracks = el.stream.getAudioTracks();
        if (audioTracks.length <= 0) {            
            const onAddTrackHandler = () => {                
                el?.stream?.removeEventListener('addtrack', onAddTrackHandler);
                this._connectToStream(el);
            };
            el.stream.addEventListener('addtrack', onAddTrackHandler);
            return;
        }
        var s = this.audioMotion.audioCtx.createMediaStreamSource(el.stream);
        
        const onInactiveHandler = () => {            
            el?.stream?.removeEventListener('inactive', onInactiveHandler);
            el.stream = null;            
            this.reconnectInputs();
        };
        el.stream.addEventListener('inactive', onInactiveHandler);

        this.connectedStreams.push(s);
        this.audioMotion.connectInput(s);
        this.disableOutput();
    }


    _createGainNode(el) {
        const audioCtx = this.audioMotion.audioCtx;
        el.audioSourceNode = audioCtx.createMediaElementSource(el);
        const gainNode = audioCtx.createGain();
        el.audioSourceNode.connect(gainNode);
        gainNode.connect(audioCtx.destination);
        return el.audioSourceNode;
    }

    _connectTo(el) {
        if (this.options.connectionMode === 1) {
            this._connectToStream(el);
        } else {
            if (!el.audioSourceNode && this.options.connectionMode === 0) {
                el.audioSourceNode = this._createGainNode(el);
            }
            try {
                this.audioMotion.connectInput(el.audioSourceNode ?? el);
            } catch (e) {
                console.error('Error connecting audio source to audioMotion:', e);
            }
        }
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

        if (this.options.connectToCapture && this.options.connectToCapture !== 0) {
            this.connectToCapture(this.options.connectToCapture === 2); // 2 = CaptureConnection.ConnectCurrentTab
        }

        this.connectToMic(this.options.connectMicrophone);

        for (const el of audioElements) {
            if (el && (el['__internalId'] === undefined || el['__internalId'])) {
                this._connectTo(el);

                if (!el.listeners) {
                    el.listeners = {                 
                        play: () => {
                            if (this.options.connectionMode === 1 && !el.stream) {                                
                                this._connectToStream(el);                                
                            }
                            if (audioCtx.state === 'suspended') {
                                audioCtx.resume();
                            }
                            if (this.options.keepState || (this.options.initialRender > 0)) {
                                if (!this.audioMotion.isOn) {
                                    this.audioMotion.toggleAnalyzer(true);
                                }
                            }
                            if (this.options.keepState) {
                                this.pauseSimulation(true);
                            } else {
                                this.stopSimulation();
                            }
                            this.dotnet.invokeMethodAsync('HandleOnPlay');
                        },
                        pause: () => {
                            if (this.options.keepState) {
                                this.audioMotion.toggleAnalyzer(false);
                                this.resumeSimulation();
                            }
                            this.dotnet.invokeMethodAsync('HandleOnPause');
                        },
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
        }

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
            mediaEl.stream = null; // TODO: Better to do this general on src change
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

    isPlaying() {
        return this.getAudioElements().some(el => !el.paused || el.autoplay);
    }

    setOptions(options) {
        this.audioMotion.setOptions(this.options = this.prepareOptions(options));
        this.reconnectInputs();
    }

    renderOneTimeStaticIf(force) {
        if (force || (!this.isSimulationRunning() && this.options.initialRender > 0 && !this.isPlaying())) {
            this.renderOneTimeStatic();
        }
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


    async readBlobAsByteArray(blobUrl) {
        console.log('READ THE BLOB BYTES');
        const response = await fetch(blobUrl);
        const blob = await response.blob();
        const arrayBuffer = await blob.arrayBuffer();
        console.log('ARRAY BUFFER', arrayBuffer);
        return Array.from(new Uint8Array(arrayBuffer));
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