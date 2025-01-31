window.AuralizeBlazor.features.switchPresetFeature = (() => {
    let lastChangeTime = 0;
    let lastColorChangeTime = 0;
    
    const onCanvasDraw = (scope, auralizer, featureOptions, instance, info) => {
        let minColorEnergy = featureOptions.minEnergyForColor ?? featureOptions.minEnergy;
        let minDebounceForColorTimeInMs = featureOptions.minDebounceForColorTimeInMs ?? featureOptions.minDebounceTimeInMs;
        let currentTime = Date.now();

        if (featureOptions.overridePresetColorsWithRandoms && currentTime - lastColorChangeTime > minDebounceForColorTimeInMs) {
            let energy = instance.getEnergy('bass');

            if (energy > minColorEnergy) {
                auralizer.dotnet.invokeMethodAsync('RandomColor');
                lastColorChangeTime = currentTime;
            }
        }


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