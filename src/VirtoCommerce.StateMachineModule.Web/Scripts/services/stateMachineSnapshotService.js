angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineSnapshotService', [
        '$timeout',
        'virtoCommerce.stateMachineModule.stateMachineStateService',
        function ($timeout,
            stateMachineStateService
        ) {
            async function makeSnapshot (states, workspace) {
                try {

                    const statesBounds = stateMachineStateService.calculateBounds(states, 20, true);

                    const totalWidth = statesBounds.maxX - statesBounds.minX;
                    const totalHeight = statesBounds.maxY - statesBounds.minY;

                    // Create a hidden clone of the workspace
                    const hiddenWorkspace = workspace.cloneNode(true);
                    hiddenWorkspace.id = 'hidden-workspace';
                    hiddenWorkspace.style.position = 'fixed';
                    hiddenWorkspace.style.left = '-9999px';
                    hiddenWorkspace.style.top = '-9999px';
                    hiddenWorkspace.style.background = 'white';
                    hiddenWorkspace.style.width = statesBounds.maxX + 'px';
                    hiddenWorkspace.style.height = statesBounds.maxY + 'px';
                    hiddenWorkspace.style.overflow = 'visible';

                    // Update SVG dimensions in the clone
                    const hiddenSvg = hiddenWorkspace.querySelector('svg');
                    if (hiddenSvg) {
                        hiddenSvg.style.width = statesBounds.maxX + 'px';
                        hiddenSvg.style.height = statesBounds.maxY + 'px';
                        hiddenSvg.setAttribute('width', statesBounds.maxX);
                        hiddenSvg.setAttribute('height', statesBounds.maxY);
                        hiddenSvg.setAttribute('xmlns:xlink', 'http://www.w3.org/1999/xlink');
                    }

                    // Add the hidden workspace to the document
                    document.body.appendChild(hiddenWorkspace);

                    const canvas = await html2canvas(hiddenWorkspace, {
                        backgroundColor: '#ffffff',
                        scale: 1,
                        logging: false,
                        useCORS: true,
                        allowTaint: true,
                        width: totalWidth,
                        height: totalHeight,
                        x: statesBounds.minX,
                        y: statesBounds.minY,
                        windowWidth: totalWidth,
                        windowHeight: totalHeight
                    });

                    const aspectRatio = canvas.height / canvas.width;
                    const targetWidth = 380;
                    const targetHeight = Math.round(targetWidth * aspectRatio);

                    const resizedCanvas = document.createElement('canvas');
                    resizedCanvas.width = targetWidth;
                    resizedCanvas.height = targetHeight;
                    const ctx = resizedCanvas.getContext('2d');

                    // Enable image smoothing for better quality
                    ctx.imageSmoothingEnabled = true;
                    ctx.imageSmoothingQuality = 'high';

                    // Draw the original canvas onto the resized one
                    ctx.drawImage(canvas, 0, 0, targetWidth, targetHeight);

                    const imageData = resizedCanvas.toDataURL('image/png', 1.0);

                    // Always remove the hidden workspace
                    if (hiddenWorkspace && hiddenWorkspace.parentNode) {
                        hiddenWorkspace.parentNode.removeChild(hiddenWorkspace);
                    }

                    return imageData;

                } catch (error) {
                    console.error('Error in makeSnaphot:', error);
                }
            };

            return {
                makeSnapshot: makeSnapshot
            };
        }
    ]);
