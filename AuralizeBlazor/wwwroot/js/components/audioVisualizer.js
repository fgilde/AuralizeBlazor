class BlazorAudioVisualizer {
    elementRef;
    dotnet;
    audioMotion;
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

        this.reconnectInputs();
    }

    prepareOptions(options) {
        options.fsElement = options.fsElement || options.visualizer;
        options.gradientLeft = options.gradientLeft || options.gradient;
        options.gradientRight = options.gradientRight || options.gradient;
        options.backgroundImage = "https://www.pr-bild-award.de/site-prba/assets/files/1/prba-teaser.jpg";
        const container = this.visualizer;
        container.style.backgroundImage = `url('${options.backgroundImage}')`;
        container.style.backgroundSize = options.backgroundSize || 'cover';
        container.style.backgroundRepeat = options.backgroundRepeat || 'no-repeat';
        container.style.backgroundPosition = options.backgroundPosition || 'center';

        options.onCanvasDraw = (instance, info) => {
            const audioMotion = this.audioMotion;
            const features = {
                energyMeter: true,
                showLogo: true,
                songProgress: true

            };
            const canvas = audioMotion.canvas,
                ctx = audioMotion.canvasCtx,
                pixelRatio = audioMotion.pixelRatio, // for scaling the size of things drawn on canvas, on Hi-DPI screens or loRes mode
                baseSize = Math.max(20 * pixelRatio, canvas.height / 27 | 0),
                centerX = canvas.width >> 1,
                centerY = canvas.height >> 1;

            if (true) {
                if (!this.waveformNode) {
                    this.waveformNode = this.audioMotion.audioCtx.createAnalyser();
                    this.waveformNode.fftSize = 4096;

                    this.bufferLength = this.waveformNode.fftSize;
                    this.dataArray = new Uint8Array(this.bufferLength);
                }
                const bufferLength = this.bufferLength,
                    waveformNode = this.waveformNode,
                    dataArray = this.dataArray;
                const idxStep = Math.max(1, Math.floor(bufferLength / canvas.width)),
                    sliceWidth = canvas.width / bufferLength * idxStep,
                    baseY = canvas.height / 4;

                // obtain time-domain data from the waveform analyzer
                waveformNode.getByteTimeDomainData(dataArray);

                // draw the waveform
                ctx.lineWidth = 1;
                ctx.strokeStyle = info.canvasGradients[1],
                    ctx.beginPath();
                let x = 0;
                for (let i = 0; i < bufferLength - idxStep; i += idxStep) {
                    const y = dataArray[i] / 128 * baseY;
                    if (i == 0)
                        ctx.moveTo(x, y);
                    else
                        ctx.lineTo(x, y);
                    x += sliceWidth;
                }
                ctx.stroke();
            }
            if (features.energyMeter) {
                const energy = audioMotion.getEnergy(),
                    peakEnergy = audioMotion.getEnergy('peak');

                // overall energy peak
                const width = 50 * pixelRatio;
                const peakY = -canvas.height * (peakEnergy - 1);
                ctx.fillStyle = '#f008';
                ctx.fillRect(width, peakY, width, 2);

                ctx.font = `${16 * pixelRatio}px sans-serif`;
                ctx.textAlign = 'left';
                ctx.fillText(peakEnergy.toFixed(4), width, peakY - 4);

                // overall energy bar
                ctx.fillStyle = '#fff8';
                ctx.fillRect(width, canvas.height, width, -canvas.height * energy);

                // bass, midrange and treble meters

                const drawLight = (posX, color, alpha) => {
                    const halfWidth = width >> 1,
                        doubleWidth = width << 1;

                    const grad = ctx.createLinearGradient(0, 0, 0, canvas.height);
                    grad.addColorStop(0, color);
                    grad.addColorStop(.75, `${color}0`);

                    ctx.beginPath();
                    ctx.moveTo(posX - halfWidth, 0);
                    ctx.lineTo(posX - doubleWidth, canvas.height);
                    ctx.lineTo(posX + doubleWidth, canvas.height);
                    ctx.lineTo(posX + halfWidth, 0);

                    ctx.save();
                    ctx.fillStyle = grad;
                    ctx.shadowColor = color;
                    ctx.shadowBlur = 40;
                    ctx.globalCompositeOperation = 'screen';
                    ctx.globalAlpha = alpha;
                    ctx.fill();
                    ctx.restore();
                }

                ctx.textAlign = 'center';
                const growSize = baseSize * 4;

                const bassEnergy = audioMotion.getEnergy('bass');
                ctx.font = `bold ${baseSize + growSize * bassEnergy}px sans-serif`;
                ctx.fillText('BASS', canvas.width * .15, centerY);
                drawLight(canvas.width * .15, '#f00', bassEnergy);

                drawLight(canvas.width * .325, '#f80', audioMotion.getEnergy('lowMid'));

                const midEnergy = audioMotion.getEnergy('mid');
                ctx.font = `bold ${baseSize + growSize * midEnergy}px sans-serif`;
                ctx.fillText('MIDRANGE', centerX, centerY);
                drawLight(centerX, '#ff0', midEnergy);

                drawLight(canvas.width * .675, '#0f0', audioMotion.getEnergy('highMid'));

                const trebleEnergy = audioMotion.getEnergy('treble');
                ctx.font = `bold ${baseSize + growSize * trebleEnergy}px sans-serif`;
                ctx.fillText('TREBLE', canvas.width * .85, centerY);
                drawLight(canvas.width * .85, '#0ff', trebleEnergy);
            }

            if (features.showLogo) {
                // the overall energy provides a simple way to sync a pulsating text/image to the beat
                // it usually works best than specific frequency ranges, for a wider range of music styles
                ctx.font = `${baseSize + audioMotion.getEnergy() * 25 * pixelRatio}px Orbitron, sans-serif`;

                ctx.fillStyle = '#fff8';
                ctx.textAlign = 'center';
                ctx.fillText('AuralizeBlazor', canvas.width - baseSize * 8, baseSize * 2);
            }

            //if (features.songProgress) {
            //    const lineWidth = canvas.height / 40,
            //        posY = lineWidth >> 1;

            //    ctx.beginPath();
            //    ctx.moveTo(0, posY);
            //    ctx.lineTo(canvas.width * audioEl.currentTime / audioEl.duration, posY);
            //    ctx.lineCap = 'round';
            //    ctx.lineWidth = lineWidth;
            //    ctx.globalAlpha = audioMotion.getEnergy(); // use the song energy to control the bar opacity
            //    ctx.stroke();
            //}
        }

        options.overlay = options.overlay || options.backgroundImage;

        return options;
    }

    connectToMic(connect) {
        if (!connect && this.micStream) {
            this.audioMotion.disconnectInput(this.micStream, true); // disconnect mic stream and release audio track
            this.audioMotion.connectOutput();
            this.micStream = null;
        }
        else if(connect && !this.micStream) {
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
        this.disconnectInputs();
        this.audioMotion.destroy();
    }
}

window.BlazorAudioVisualizer = BlazorAudioVisualizer;

export function initializeBlazorAudioVisualizer(elementRef, dotnet, options) {
    return new BlazorAudioVisualizer(elementRef, dotnet, options);
}