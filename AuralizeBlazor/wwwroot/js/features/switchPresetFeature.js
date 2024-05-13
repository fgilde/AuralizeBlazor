window.AuralizeBlazor.features.switchPresetFeature = (() => {
    let lastChangeTime = 0;
    
    const onCanvasDraw = (scope, auralizer, featureOptions, instance, info) => {
        const currentTime = Date.now();
        if (currentTime - lastChangeTime > featureOptions.minDebounceTimeInMs) {
            let energy = instance.getEnergy('bass');
            //if (auralizer.options.reflexRatio > 0) {
            //    energy = energy / auralizer.options.reflexRatio;
            //}
            //if (auralizer.options.linearBoost > 1) {
            //    energy = energy * auralizer.options.linearBoost;
            //}
            if (energy > featureOptions.minEnergy) {
                auralizer.dotnet.invokeMethodAsync(featureOptions.pickRandom ? 'RandomPreset' : 'NextPresetAsync', 5);
                lastChangeTime = currentTime; 
            }
        }
    };

    return {
        onCanvasDraw: onCanvasDraw
    };
})();