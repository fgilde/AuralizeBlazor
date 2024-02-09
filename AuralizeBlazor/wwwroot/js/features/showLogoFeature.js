window.AuralizeBlazor.features.showLogo = {

    _getCoordinates: function (featureOptions, baseSize, centerX, centerY, canvas) {
        const getPosition = (position) => {
            const positions = {
                'top-left': 0, '0': 0,
                'top-center': 1, '1': 1,
                'top-right': 2, '2': 2,
                'center-left': 3, '3': 3,
                'center-center': 4, '4': 4,
                'center-right': 5, '5': 5,
                'bottom-left': 6, '6': 6,
                'bottom-center': 7, '7': 7,
                'bottom-right': 8, '8': 8,
            };
            return positions[position.toString().toLowerCase()] || positions['0'];
        };

        let pos = getPosition(featureOptions?.position);

        let x, y, textAlign;
        switch (pos) {
        case 0: // 'top-left'
            x = baseSize;
            y = baseSize * 2;
            textAlign = 'left';
            break;
        case 1: // 'top-center'
            x = centerX;
            y = baseSize * 2;
            break;
        case 2: // 'top-right'
            x = canvas.width - baseSize;
            y = baseSize * 2;
            textAlign = 'right';
            break;
        case 3: // 'center-left'
            x = baseSize;
            y = centerY;
            textAlign = 'left';
            break;
        case 4: // 'center-center'
            x = centerX;
            y = centerY;
            break;
        case 5: // 'center-right'
            x = canvas.width - baseSize;
            y = centerY;
            textAlign = 'right';
            break;
        case 6: // 'bottom-left'
            x = baseSize;
            y = canvas.height - baseSize;
            textAlign = 'left';
            break;
        case 7: // 'bottom-center'
            x = centerX;
            y = canvas.height - baseSize;
            break;
        case 8: // 'bottom-right'
            x = canvas.width - baseSize;
            y = canvas.height - baseSize;
            textAlign = 'right';
            break;
        }
        return {
            x,
            y,
            textAlign
        }
    },

    onCanvasDraw: (scope, auralizer, featureOptions, instance, info) => {
        const audioMotion = auralizer.audioMotion;
        const canvas = audioMotion.canvas,
            ctx = audioMotion.canvasCtx,
            pixelRatio = audioMotion.pixelRatio,
            baseSize = Math.max(20 * pixelRatio, canvas.height / 27 | 0),
            centerX = canvas.width >> 1,
            centerY = canvas.height >> 1,
            pos = scope._getCoordinates(featureOptions, baseSize, centerX, centerY, canvas);

        ctx.font = `${baseSize + audioMotion.getEnergy() * 25 * pixelRatio}px Orbitron, sans-serif`;
        ctx.textAlign = pos.textAlign || 'center';

        if (!featureOptions?.labelColor) {
            ctx.fillStyle = featureOptions.labelColor;
        } 

        ctx.fillText(featureOptions?.label || 'AuralizeBlazor', pos.x, pos.y);
    }



}