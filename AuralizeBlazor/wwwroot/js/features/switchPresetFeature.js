window.AuralizeBlazor.features.switchPresetFeature = (() => {
    let lastChangeTime = 0;
    
    const onCanvasDraw = (scope, blazorAudioVisualizer, featureOptions, instance, info) => {
        const currentTime = Date.now();
        if (currentTime - lastChangeTime > featureOptions.minDebounceTimeInMs) {
            const energy = instance.getEnergy();
            if (energy > featureOptions.minEnergy) {
                blazorAudioVisualizer.dotnet.invokeMethodAsync('NextPresetAsync');
                lastChangeTime = currentTime; 
            }
        }
    };

    return {
        onCanvasDraw: onCanvasDraw
    };
})();