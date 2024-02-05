window.AuralizeBlazor.features.radialRadius = {

    onCanvasDraw: (scope, blazorAudioVisualizer, featureOptions, instance, info) => {
        instance.radius = blazorAudioVisualizer.options.radius + instance.getEnergy();
    }
}