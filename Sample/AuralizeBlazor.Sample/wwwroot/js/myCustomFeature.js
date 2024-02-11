window.myCustomFeature = (() => {
    let lastChangeTime = 0;
    let savedPositions = []; 
    let numDots = null; 

    const initSavedPositions = (canvas) => {
        savedPositions = [];
        for (let i = 0; i < numDots; i++) {
            let x = Math.random() * canvas.width;
            let y = Math.random() * canvas.height;
            savedPositions.push({ x, y }); 
        }
    };

    const onCanvasDraw = (scope, auralizer, featureOptions, instance, info) => {
        numDots = numDots || featureOptions.numDots || 5;
        const currentTime = Date.now();
        const ctx = instance.canvasCtx,
            canvas = ctx.canvas;
        const energy = instance.getEnergy('bass');

        if (savedPositions.length === 0) {
            initSavedPositions(canvas);
        }

        if (currentTime - lastChangeTime > 500 && energy > .35) {
            for (let i = 0; i < numDots; i++) {
                savedPositions[i].x = (Math.random() * canvas.width) * (0.5 + energy / 2);
                savedPositions[i].y = (Math.random() * canvas.height) * (0.5 + energy / 2);
            }
            lastChangeTime = currentTime; 
        }

        savedPositions.forEach(pos => {
            let radius = 10 + energy * 20;
            ctx.beginPath();
            ctx.arc(pos.x, pos.y, radius, 0, 2 * Math.PI, false);
            ctx.fill();
        });
    };

    return {
        onCanvasDraw: onCanvasDraw
    };
})();
