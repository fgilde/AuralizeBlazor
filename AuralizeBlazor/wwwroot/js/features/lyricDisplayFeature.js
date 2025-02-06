﻿window.AuralizeBlazor.features.lyricsDisplay = {
    onCanvasDraw: function (scope, auralizer, featureOptions, instance, info) {
        // Abort if no lyrics are provided
        if (!auralizer.options.lyrics || !auralizer.options.lyrics.lines) {
            return;
        }

        //#### Options #######
        // Text position: "top", "middle", or "bottom"
        var textPosition = featureOptions.textPosition || "middle";
        if (typeof textPosition === 'number') {
            if (textPosition === 0) textPosition = "top";
            else if (textPosition === 1) textPosition = "middle";
            else if (textPosition === 2) textPosition = "bottom";
        }
        if (textPosition !== "top" && textPosition !== "middle" && textPosition !== "bottom") {
            textPosition = "middle";
        }
        // Base font size (reference value)
        var baseFontSizeRef = featureOptions.baseFontSize || 40;
        // Line spacing (vertical distance between lines; reference value)
        var lineSpacingRef = featureOptions.lineSpacing || 50;
        // Gradient colors array (for the text)
        var gradientColors = featureOptions.gradientColors || auralizer.getCurrentColors();
        if (!gradientColors || gradientColors.length <= 0)
            gradientColors = ["#ff0088", "#ff8800", "#ffff00"];
        if (gradientColors.length === 1)
            gradientColors.push(gradientColors[0]);
        // Enable word-based animation for transitions (boolean)
        var enableWordAnimation = featureOptions.enableWordAnimation;
        // Maximum dispersion offsets for word animation (in pixels; reference values)
        var wordAnimationMaxXRef = featureOptions.wordAnimationMaxX || 50;
        var wordAnimationMaxYRef = featureOptions.wordAnimationMaxY || 30;
        //#######################

        var canvas = instance.canvas,
            ctx = instance.canvasCtx,
            width = canvas.width,
            height = canvas.height;

        // Calculate scale factor based on a reference resolution (e.g. 800x600)
        var scaleFactor = Math.min(width / 800, height / 600);

        // Scale the size-dependent options accordingly
        var baseFontSize = baseFontSizeRef * scaleFactor;
        var lineSpacing = lineSpacingRef * scaleFactor;
        var wordAnimationMaxX = wordAnimationMaxXRef * scaleFactor;
        var wordAnimationMaxY = wordAnimationMaxYRef * scaleFactor;

        // Set background with a slight trail effect
        ctx.fillStyle = 'rgba(0, 0, 0, 0.1)';
        ctx.fillRect(0, 0, width, height);

        // Determine current playback time (preferably from auralizer.getTime())
        var currentTime = 0;
        if (auralizer.getTime && typeof auralizer.getTime === "function") {
            currentTime = auralizer.getTime();
        } else {
            var audEls = AuralizeBlazor.instance.getAudioElements();
            if (audEls && audEls.length > 0) {
                currentTime = audEls[0].currentTime;
            }
        }

        // Parse lyric timestamps (in seconds) if not already done
        if (!auralizer.options.lyrics._parsed) {
            auralizer.options.lyrics.lines.forEach(function (line) {
                if (!line.timeSeconds) {
                    // Expected format e.g. "00:00:18.4510000"
                    var parts = line.timeStamp.split(":");
                    if (parts.length === 3) {
                        line.timeSeconds = parseInt(parts[0], 10) * 3600 +
                            parseInt(parts[1], 10) * 60 +
                            parseFloat(parts[2]);
                    } else {
                        line.timeSeconds = 0;
                    }
                }
            });
            // Sort lines by time
            auralizer.options.lyrics.lines.sort(function (a, b) {
                return a.timeSeconds - b.timeSeconds;
            });
            auralizer.options.lyrics._parsed = true;
        }

        var lines = auralizer.options.lyrics.lines;

        // Find index of the current active line:
        // Search for the last line with timeSeconds <= currentTime.
        var currentIndex = -1;
        for (var i = 0; i < lines.length; i++) {
            if (lines[i].timeSeconds <= currentTime) {
                currentIndex = i;
            } else {
                break;
            }
        }
        if (currentIndex === -1) {
            currentIndex = 0;
        }

        // Determine which lines to display: previous (if exists), current, and next (if exists)
        var displayIndices = [];
        if (currentIndex > 0) {
            displayIndices.push(currentIndex - 1);
        }
        displayIndices.push(currentIndex);
        if (currentIndex < lines.length - 1) {
            displayIndices.push(currentIndex + 1);
        }

        // Determine base Y position based on textPosition option
        var baseY;
        if (textPosition === "top") {
            baseY = height * 0.2;
        } else if (textPosition === "bottom") {
            baseY = height * 0.8;
        } else {
            baseY = height / 2;
        }

        // Get audio energy for modulation
        var energy = instance.getEnergy ? instance.getEnergy() : 0;

        // Calculate "progress" for the current line transition (if next line exists)
        var progress = 0;
        if (currentIndex < lines.length - 1) {
            var cur = lines[currentIndex],
                nxt = lines[currentIndex + 1];
            progress = (currentTime - cur.timeSeconds) / (nxt.timeSeconds - cur.timeSeconds);
            progress = Math.min(1, Math.max(0, progress));
        }

        // Easing function (easeOutQuad) for smooth dispersion
        function easeOutQuad(t) {
            return t * (2 - t);
        }

        // Helper function to calculate alpha based on line difference
        // diff === 0 (current line) alpha = 1; diff === 1 (next line) alpha = 0.7;
        // diff === -1 (previous line) fades out with an easing based on progress.
        function calcAlpha(diff) {
            if (diff === 0) return 1;
            if (diff === 1) return 0.7;
            if (diff === -1) {
                var baseAlpha = 0.7;
                var mod = (progress - 0.5) * 2; // for progress from 0.5 to 1: mod 0 to 1
                mod = Math.min(1, Math.max(0, mod));
                return baseAlpha * (1 - mod);
            }
            return 0.5;
        }

        // Helper function to create a gradient for the text.
        // For the current line, modify the gradient based on energy.
        function createGradient(xCenter, textWidth, isCurrent) {
            var grad = ctx.createLinearGradient(xCenter - textWidth / 2, 0, xCenter + textWidth / 2, 0);
            try {
                if (isCurrent && energy > 0.5) {
                    grad.addColorStop(0, gradientColors[0]);
                    grad.addColorStop(0.5, "#ffffff"); // middle stop set to white
                    grad.addColorStop(1, gradientColors[gradientColors.length - 1]);
                } else {
                    gradientColors.forEach(function (col, index) {
                        grad.addColorStop(index / (gradientColors.length - 1), col);
                    });
                }
            }catch(e) {}
            return grad;
        }

        // Render the selected lines
        displayIndices.forEach(function (index) {
            var line = lines[index];
            var diff = index - currentIndex; // -1: previous, 0: current, 1: next

            // Base vertical offset: diff * lineSpacing.
            var offset = diff * lineSpacing;
            // For the current line, shift upward gradually based on progress.
            if (diff === 0) {
                offset -= progress * lineSpacing;
            }
            // For the previous line, add additional upward shift based on progress.
            if (diff === -1) {
                offset -= progress * lineSpacing;
            }

            var alpha = calcAlpha(diff);

            // Calculate font size; current line is slightly larger.
            var fontSize = baseFontSize;
            if (diff === 0) {
                fontSize = baseFontSize * 1.2;
            }
            // Modulate font size slightly with audio energy
            fontSize *= 1 + energy * 0.1;

            // Set font properties (bold for current line)
            var fontWeight = (diff === 0) ? "bold" : "normal";
            ctx.font = fontWeight + " " + fontSize + "px sans-serif";
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';

            var xCenter = width / 2;

            // If word-based animation is enabled, render the line word-by-word for smooth transition
            if (enableWordAnimation) {
                // Split the line into words
                var words = line.text.split(" ");
                // Measure the widths of all words (including spaces) to center the entire line
                var wordWidths = [];
                var totalTextWidth = 0;
                words.forEach(function (word, i) {
                    var wordWithSpace = word + (i < words.length - 1 ? " " : "");
                    var w = ctx.measureText(wordWithSpace).width;
                    wordWidths.push(w);
                    totalTextWidth += w;
                });
                var startX = xCenter - totalTextWidth / 2;
                var currentX = startX;
                // Compute dispersion factor using easing for smooth transition
                var dispersion = easeOutQuad(progress);
                // Render each word individually
                words.forEach(function (word, i) {
                    var wordWithSpace = word + (i < words.length - 1 ? " " : "");
                    var wordWidth = wordWidths[i];
                    var wordCenter = currentX + wordWidth / 2;
                    currentX += wordWidth;
                    // For smooth transition, apply dispersion offsets regardless of diff,
                    // but adjust intensity based on diff (for previous line, effect is stronger)
                    var offsetX = 0, offsetY = 0;
                    if (diff === -1) {
                        var angle = ((i / words.length) - 0.5) * Math.PI; // from -pi/2 to pi/2
                        offsetX = Math.cos(angle) * wordAnimationMaxX * dispersion;
                        offsetY = -Math.sin(angle) * wordAnimationMaxY * dispersion;
                    }
                    // Final word position
                    var wordX = wordCenter + offsetX;
                    var wordY = baseY + offset + offsetY;
                    // Create gradient for the word
                    var wordGrad = createGradient(wordCenter, wordWidth, diff === 0);
                    ctx.fillStyle = wordGrad;
                    ctx.globalAlpha = alpha;
                    ctx.fillText(word, wordCenter + offsetX, wordY);
                });
            } else {
                // Otherwise, render the full line as a block.
                var textWidth = ctx.measureText(line.text).width;
                var grad = createGradient(xCenter, textWidth, diff === 0);
                ctx.fillStyle = grad;
                ctx.globalAlpha = alpha;
                ctx.fillText(line.text, xCenter, baseY + offset);
            }
        });

        // Reset global alpha
        ctx.globalAlpha = 1;
    }
};
