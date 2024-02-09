window.AuralizeBlazor.features.radialRadius = {

    onCanvasDraw: (scope, auralizer, featureOptions, instance, info) => {
        instance.radius = auralizer.options.radius + instance.getEnergy();
    }
}