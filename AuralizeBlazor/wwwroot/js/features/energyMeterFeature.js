window.AuralizeBlazor.features.energyMeter = {

    onCanvasDraw: (scope, auralizer, featureOptions, instance, info) => {
        const audioMotion = instance,
            canvas = audioMotion.canvas,
            ctx = audioMotion.canvasCtx,
            pixelRatio = audioMotion.pixelRatio, // for scaling the size of things drawn on canvas, on Hi-DPI screens or loRes mode
            baseSize = Math.max(20 * pixelRatio, canvas.height / 27 | 0),
            centerX = canvas.width >> 1,
            centerY = canvas.height >> 1,
            energy = audioMotion.getEnergy(),
            peakEnergy = audioMotion.getEnergy('peak'),
            width = 50 * pixelRatio,
            peakY = -canvas.height * (peakEnergy - 1);

        // overall energy peak
        if (featureOptions.showPeakEnergyBar === true) {

            //ctx.fillStyle = '#f008';
            ctx.fillRect(width, peakY, width, 2);

            ctx.font = `${16 * pixelRatio}px sans-serif`;
            ctx.textAlign = 'left';
            ctx.fillText(peakEnergy.toFixed(4), width, peakY - 4);

            // overall energy bar
            //ctx.fillStyle = '#fff8';
            ctx.fillRect(width, canvas.height, width, -canvas.height * energy);
        }

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
        if (featureOptions.bassText) {
            ctx.font = `bold ${baseSize + growSize * bassEnergy}px sans-serif`;
            ctx.fillText(featureOptions.bassText, canvas.width * .15, centerY);
        }
        if (featureOptions.showBassLight) {
            drawLight(canvas.width * .15, '#f00', bassEnergy);
        }

        if (featureOptions.showMidrangeLight) {
            drawLight(canvas.width * .325, '#f80', audioMotion.getEnergy('lowMid'));
        }

        const midEnergy = audioMotion.getEnergy('mid');
        if (featureOptions.midrangeText) {
            ctx.font = `bold ${baseSize + growSize * midEnergy}px sans-serif`;
            ctx.fillText(featureOptions.midrangeText, centerX, centerY);
        }
        if (featureOptions.showMidrangeLight) {
            drawLight(centerX, '#ff0', midEnergy);
            drawLight(canvas.width * .675, '#0f0', audioMotion.getEnergy('highMid'));
        }

        const trebleEnergy = audioMotion.getEnergy('treble');
        if (featureOptions.trebleText) {
            ctx.font = `bold ${baseSize + growSize * trebleEnergy}px sans-serif`;
            ctx.fillText(featureOptions.trebleText, canvas.width * .85, centerY);
        }
        if (featureOptions.showTrebleLight) {
            drawLight(canvas.width * .85, '#0ff', trebleEnergy);
        }
    }

}