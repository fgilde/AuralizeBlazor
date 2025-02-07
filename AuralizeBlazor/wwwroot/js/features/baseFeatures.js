window.AuralizeBlazor.features.lineParticle = {
    onCanvasDraw: (scope, auralizer, featureOptions, instance, info) => {
        const canvas = instance.canvas,
            ctx = instance.canvasCtx,
            width = canvas.width,
            height = canvas.height,
            centerX = width / 2,
            centerY = height / 2;

        ctx.fillStyle = 'rgba(0, 0, 0, 0.1)';
        ctx.fillRect(0, 0, width, height);
        const energy = instance.getEnergy ? instance.getEnergy() : 0,
            bass = instance.getEnergy ? instance.getEnergy('bass') : 0,
            mid = instance.getEnergy ? instance.getEnergy('mid') : 0,
            treble = instance.getEnergy ? instance.getEnergy('treble') : 0,
            time = performance.now() / 1000; // Zeit in Sekunden

        if (!scope.lineParticles || scope.lineParticles.length !== featureOptions.particleCount) {
            scope.lineParticles = [];
            const particleCount = featureOptions.particleCount;
            for (let i = 0; i < particleCount; i++) {
                scope.lineParticles.push({
                    x: Math.random() * width,
                    y: Math.random() * height,
                    vx: (Math.random() - 0.5) * 1.5,
                    vy: (Math.random() - 0.5) * 1.5,
                    baseSize: 2 + Math.random() * 3,
                    sizeOsc: Math.random() * Math.PI * 2,
                    color: `hsl(${Math.random() * 360}, 80%, 60%)`
                });
            }
        }

        for (let particle of scope.lineParticles) {
            particle.x += particle.vx * (1 + energy * 2);
            particle.y += particle.vy * (1 + energy * 2);

            if (particle.x < 0) particle.x += width;
            if (particle.x > width) particle.x -= width;
            if (particle.y < 0) particle.y += height;
            if (particle.y > height) particle.y -= height;

            const osc = (Math.sin(time * 2 + particle.sizeOsc) + 1) / 2; // Wert zwischen 0 und 1
            const size = particle.baseSize * (0.5 + osc); // variiert von ca. 0.5x bis 1.5x der Basisgröße

            const grad = ctx.createRadialGradient(particle.x, particle.y, 0, particle.x, particle.y, size);
            grad.addColorStop(0, particle.color);
            grad.addColorStop(1, 'rgba(0, 0, 0, 0)');
            ctx.beginPath();
            ctx.arc(particle.x, particle.y, size, 0, Math.PI * 2);
            ctx.fillStyle = grad;
            ctx.fill();
        }

        const connectionThreshold = featureOptions.connectionThreshold || 50;
        for (let i = 0; i < scope.lineParticles.length; i++) {
            for (let j = i + 1; j < scope.lineParticles.length; j++) {
                const p1 = scope.lineParticles[i],
                    p2 = scope.lineParticles[j],
                    dx = p1.x - p2.x,
                    dy = p1.y - p2.y,
                    dist = Math.sqrt(dx * dx + dy * dy);
                if (dist < connectionThreshold) {
                    // Je näher die Partikel beieinander liegen, desto stärker (und dicker) die Linie
                    const alpha = (1 - dist / connectionThreshold) * (0.3 + energy * 0.7);
                    const lineWidth = 1 + energy * 2;
                    ctx.strokeStyle = `rgba(255, 255, 255, ${alpha})`;
                    ctx.lineWidth = lineWidth;
                    ctx.beginPath();
                    ctx.moveTo(p1.x, p1.y);
                    ctx.lineTo(p2.x, p2.y);
                    ctx.stroke();
                }
            }
        }

        ctx.save();
        ctx.translate(centerX, centerY);
        ctx.rotate(time * 0.2);
        const maxRadius = Math.min(width, height) / 3;
        const vortexGrad = ctx.createRadialGradient(0, 0, 0, 0, 0, maxRadius);
        vortexGrad.addColorStop(0, `rgba(255, 255, 255, ${0.2 + energy * 0.5})`);
        vortexGrad.addColorStop(1, 'rgba(0, 0, 0, 0)');
        ctx.fillStyle = vortexGrad;
        ctx.beginPath();
        const spiralPoints = 120;
        ctx.moveTo(0, 0);
        for (let i = 0; i < spiralPoints; i++) {
            const angle = (i / spiralPoints) * Math.PI * 8;
            const radius = (i / spiralPoints) * maxRadius * (1 + energy * 0.5);
            const x = radius * Math.cos(angle);
            const y = radius * Math.sin(angle);
            ctx.lineTo(x, y);
        }
        ctx.lineTo(0, 0);
        ctx.closePath();
        ctx.fill();
        ctx.restore();

        if (featureOptions.epicText) {
            ctx.font = `bold ${Math.floor(40 + energy * 50)}px sans-serif`;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillStyle = `rgba(255, 255, 255, ${0.7 + energy * 0.3})`;
            ctx.fillText(featureOptions.epicText, centerX, centerY);
        }
    }
};

window.AuralizeBlazor.features.vortexParticle = {
    onCanvasDraw: (scope, auralizer, featureOptions, instance, info) => {
        const canvas = instance.canvas,
            ctx = instance.canvasCtx,
            width = canvas.width,
            height = canvas.height,
            centerX = width / 2,
            centerY = height / 2;

        // Zeit in Sekunden
        const time = performance.now() / 1000;

        // Globaler Mittelpunkt, der sich über den Canvas bewegt
        const globalCenterX = centerX + Math.sin(time * 0.3) * (width / 4);
        const globalCenterY = centerY + Math.cos(time * 0.3) * (height / 4);

        // Hintergrund mit Trail-Effekt
        ctx.fillStyle = 'rgba(0, 0, 0, 0.1)';
        ctx.fillRect(0, 0, width, height);

        // Audio-Energie abrufen
        const energy = instance.getEnergy ? instance.getEnergy() : 0,
            bass = instance.getEnergy ? instance.getEnergy('bass') : 0,
            mid = instance.getEnergy ? instance.getEnergy('mid') : 0,
            treble = instance.getEnergy ? instance.getEnergy('treble') : 0;

        // Partikelsystem initialisieren (nur einmal)
        if (!scope.vortexParticles || scope.vortexParticles.length !== featureOptions.particleCount) {
            scope.vortexParticles = [];
            const particleCount = featureOptions.particleCount;
            const maxZ = 200; // maximale Tiefe (willkürlicher Wert)
            for (let i = 0; i < particleCount; i++) {
                const angle = Math.random() * Math.PI * 2;
                const radius = Math.random() * Math.min(width, height) / 2;
                // Z-Koordinate zufällig in [-maxZ/2, maxZ/2]
                const z = Math.random() * maxZ - maxZ / 2;
                // Zufällige Geschwindigkeit in Z-Richtung
                const zSpeed = (Math.random() - 0.5) * 0.5;
                scope.vortexParticles.push({
                    angle: angle,              // aktueller Winkel
                    radius: radius,            // aktueller Abstand vom globalen Mittelpunkt
                    speed: 0.005 + Math.random() * 0.01, // Drehgeschwindigkeit
                    size: 1 + Math.random() * 2,         // Basis-Partikelgröße
                    color: `hsl(${Math.random() * 360}, 80%, 60%)`,
                    z: z,                      // Z-Koordinate (Tiefe)
                    zSpeed: zSpeed             // Geschwindigkeit in Z-Richtung
                });
            }
        }

        // Fokallänge für die Perspektivprojektion (je kleiner der Wert, desto stärker der Effekt)
        const focalLength = 300;

        // Partikel aktualisieren und zeichnen
        scope.vortexParticles.forEach(particle => {
            // Aktualisiere Winkel und Radius – die Audioenergie verstärkt die Drehung
            particle.angle += particle.speed * (1 + energy * 5);
            particle.radius += Math.sin(time * 2 + particle.angle) * 0.5 * energy;

            // Aktualisiere die Z-Position
            particle.z += particle.zSpeed * (1 + energy);
            const maxZ = 200;
            if (particle.z > maxZ / 2) {
                particle.z = maxZ / 2;
                particle.zSpeed = -Math.abs(particle.zSpeed);
            }
            if (particle.z < -maxZ / 2) {
                particle.z = -maxZ / 2;
                particle.zSpeed = Math.abs(particle.zSpeed);
            }

            // Berechne den Perspektivfaktor
            const perspective = focalLength / (focalLength + particle.z);

            // Berechne die Partikelposition (mit Perspektive und relativ zum globalen Mittelpunkt)
            const x = globalCenterX + particle.radius * Math.cos(particle.angle) * perspective;
            const y = globalCenterY + particle.radius * Math.sin(particle.angle) * perspective;

            // Endgröße des Partikels: Basisgröße plus Energie, skaliert durch die Perspektive
            const finalSize = (particle.size + energy * 4) * perspective;

            // Zeichne das Partikel mit einem radialen Farbverlauf (Glow-Effekt)
            ctx.beginPath();
            ctx.arc(x, y, finalSize, 0, Math.PI * 2);
            const grad = ctx.createRadialGradient(x, y, 0, x, y, finalSize);
            grad.addColorStop(0, particle.color);
            grad.addColorStop(1, 'rgba(0, 0, 0, 0)');
            ctx.fillStyle = grad;
            ctx.fill();
        });

        // Zeichne den Vortex (spiralförmiger Hintergrund)
        ctx.save();
        ctx.translate(globalCenterX, globalCenterY);
        ctx.rotate(time * 0.2); // langsame Rotation
        const maxRadius = Math.min(width, height) / 3;
        const vortexGrad = ctx.createRadialGradient(0, 0, 0, 0, 0, maxRadius);
        vortexGrad.addColorStop(0, `rgba(255, 255, 255, ${0.2 + energy * 0.5})`);
        vortexGrad.addColorStop(1, 'rgba(0, 0, 0, 0)');
        ctx.fillStyle = vortexGrad;
        ctx.beginPath();
        const spiralPoints = 120;
        ctx.moveTo(0, 0);
        for (let i = 0; i < spiralPoints; i++) {
            const angle = (i / spiralPoints) * Math.PI * 8; // 4 vollständige Umdrehungen (8π)
            const radius = (i / spiralPoints) * maxRadius * (1 + energy * 0.5);
            const x = radius * Math.cos(angle);
            const y = radius * Math.sin(angle);
            ctx.lineTo(x, y);
        }
        ctx.lineTo(0, 0);
        ctx.closePath();
        ctx.fill();
        ctx.restore();

        // Optional: Epischer Text in der Mitte
        if (featureOptions.epicText) {
            ctx.font = `bold ${Math.floor(40 + energy * 50)}px sans-serif`;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillStyle = `rgba(255, 255, 255, ${0.7 + energy * 0.3})`;
            ctx.fillText(featureOptions.epicText, globalCenterX, globalCenterY);
        }
    }
};
