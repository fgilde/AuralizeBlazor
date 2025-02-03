window.AuralizeBlazor.features.showLogo = {

    _getCoordinates: function (position, baseSize, centerX, centerY, canvas, energy, instance, key) {
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
            'random': 9, '9': 9
        };
        const getPosition = (position) => {
            return positions[position.toString().toLowerCase()] ?? positions['0'];
        };

        let pos = getPosition(position.position);
        
        let x, y, textAlign = 'center';

        if (pos === positions['random']) { 
            if (!instance.__randomCoords) {
                instance.__randomCoords = {};
            }
            const RANDOM_THRESHOLD = position.randomMinEnergy;

            const minTime = position.randomChangeDebounceInMs.min;
            const maxTime = position.randomChangeDebounceInMs.max;

            const now = Date.now();
            if (!instance.__randomCoords[key]) {
                const waitTime = minTime + Math.random() * (maxTime - minTime);
                x = baseSize + Math.random() * (canvas.width - 2 * baseSize);
                y = baseSize + Math.random() * (canvas.height - 2 * baseSize);
                textAlign = 'center';
                instance.__randomCoords[key] = { x, y, textAlign, lastUpdate: now, waitTime };
            } else {
                if (energy > RANDOM_THRESHOLD &&
                    (now - instance.__randomCoords[key].lastUpdate >= instance.__randomCoords[key].waitTime)) {
                    const waitTime = 200 + Math.random() * (1000 - 200);
                    x = baseSize + Math.random() * (canvas.width - 2 * baseSize);
                    y = baseSize + Math.random() * (canvas.height - 2 * baseSize);
                    textAlign = 'center';
                    instance.__randomCoords[key] = { x, y, textAlign, lastUpdate: now, waitTime };
                } else {
                    x = instance.__randomCoords[key].x;
                    y = instance.__randomCoords[key].y;
                    textAlign = instance.__randomCoords[key].textAlign;
                }
            }
            return { x, y, textAlign };
        }

        switch (pos) {
            case positions['top-left']: // top-left
                x = baseSize;
                y = baseSize * 2;
                textAlign = 'left';
                break;
            case positions['top-center']: // top-center
                x = centerX;
                y = baseSize * 2;
                textAlign = 'center';
                break;
            case positions['top-right']: // top-right
                x = canvas.width - baseSize;
                y = baseSize * 2;
                textAlign = 'right';
                break;
            case positions['center-left']: // center-left
                x = baseSize;
                y = centerY;
                textAlign = 'left';
                break;
            case positions['center-center']: // center-center
                x = centerX;
                y = centerY;
                textAlign = 'center';
                break;
            case positions['center-right']: // center-right
                x = canvas.width - baseSize;
                y = centerY;
                textAlign = 'right';
                break;
            case positions['bottom-left']: // bottom-left
                x = baseSize;
                y = canvas.height - baseSize;
                textAlign = 'left';
                break;
            case positions['bottom-center']: // bottom-center
                x = centerX;
                y = canvas.height - baseSize;
                textAlign = 'center';
                break;
            case positions['bottom-right']: // bottom-right
                x = canvas.width - baseSize;
                y = canvas.height - baseSize;
                textAlign = 'right';
                break;
        }
        x += position.margin.left;
        x -= position.margin.right;
        y += position.margin.top;
        y -= position.margin.bottom;

        return { x, y, textAlign };
    },

    onCanvasDraw: (scope, auralizer, featureOptions, instance, info) => {
        const audioMotion = auralizer.audioMotion,
            canvas = audioMotion.canvas,
            ctx = audioMotion.canvasCtx,
            pixelRatio = audioMotion.pixelRatio,
            baseSize = Math.max(20 * pixelRatio, canvas.height / 27 | 0) * (featureOptions?.textScale ?? 1),
            centerX = canvas.width >> 1,
            centerY = canvas.height >> 1,
            energy = audioMotion.getEnergy();

        const pos = scope._getCoordinates(featureOptions?.labelPosition, baseSize, centerX, centerY, canvas, energy, instance, 'label');
        ctx.font = `${baseSize + energy * 25 * pixelRatio}px Orbitron, sans-serif`;
        ctx.textAlign = pos.textAlign || 'center';
        if (featureOptions?.labelColor) {
            ctx.fillStyle = featureOptions.labelColor;
        }
        ctx.fillText(featureOptions?.label || '', pos.x, pos.y);

        if (featureOptions?.image) {
            let img = featureOptions.image;

            if (typeof img === 'string') {
                if (!instance.__cachedImages) {
                    instance.__cachedImages = {};
                }
                if (!instance.__cachedImages[img]) {
                    const newImg = new Image();
                    newImg.src = img;
                    instance.__cachedImages[img] = newImg;
                }
                img = instance.__cachedImages[img];
            }

            
            if (!img.complete) {
                return;
            }

            const baseImageSize = img.naturalWidth * (featureOptions?.imageScale ?? 1); 
            const posImage = scope._getCoordinates(featureOptions?.imagePosition, baseImageSize, centerX, centerY, canvas, energy, instance, 'image');
            const imgSize = baseImageSize + energy * 35 * pixelRatio;
            let imgX = posImage.x;
            if (posImage.textAlign === 'right') {
                imgX = posImage.x - imgSize;
            } else if (posImage.textAlign === 'center') {
                imgX = posImage.x - imgSize / 2;
            }
            const imgY = posImage.y - imgSize / 2;
            ctx.drawImage(img, imgX, imgY, imgSize, imgSize);
        }
    }
};
