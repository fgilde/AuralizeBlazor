window.AuralizeBlazor.features.waveNode = {

    onCanvasDraw: (scope, auralizer, featureOptions, instance, info) => {
        if (!instance.audioCtx) {
            return;
        }
        if (!scope.waveformNode) {
            scope.waveformNode = instance.audioCtx.createAnalyser();
            scope.waveformNode.fftSize = 4096;
            
            // connect audioMotion output to our waveform analyzer
            instance.connectOutput(scope.waveformNode);
        }

        const ctx = instance.canvasCtx,
            canvas = ctx.canvas,
            bufferLength = scope.waveformNode.fftSize,
            dataArray = new Uint8Array(bufferLength),
            idxStep = Math.max(1, Math.floor(bufferLength / canvas.width)),
            sliceWidth = canvas.width / bufferLength * idxStep,
            baseY = canvas.height / 4;

        // obtain time-domain data from the waveform analyzer
        scope.waveformNode.getByteTimeDomainData(dataArray);

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

}