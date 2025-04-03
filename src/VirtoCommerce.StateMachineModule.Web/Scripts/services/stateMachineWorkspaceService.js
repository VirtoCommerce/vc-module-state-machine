angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineWorkspaceService', [
        '$timeout',
        'virtoCommerce.stateMachineModule.stateMachineStateService',
        'virtoCommerce.stateMachineModule.stateMachineTransitionService',
        function ($timeout,
            stateMachineStateService,
            stateMachineTransitionService
        ) {

            function updateWorkspaceSize(states, transitions, $scope, workspace) {
                const bladeContent = document.getElementById('visualEditorBlade');
                if (!workspace || !bladeContent) return;

                const bladeRect = bladeContent.getBoundingClientRect();
                let statesBounds = stateMachineStateService.calculateBounds(states, 10, false);

                workspace.style.width = '100%';
                workspace.style.position = 'absolute';
                workspace.style.top = '0';
                workspace.style.left = '0';
                workspace.style.right = '0';
                workspace.style.bottom = '0';
                workspace.style.overflow = 'auto';

                const svg = workspace.querySelector('svg');
                if (svg) {
                    // If we have negative coordinates, shift all states to positive space
                    if (statesBounds.minX < 0) {
                        const shift = Math.abs(statesBounds.minX);
                        states.forEach(state => {
                            state.position.x += shift;
                        });
                        statesBounds.maxX += shift;
                        statesBounds.minX = 0;
                    }

                    if (statesBounds.minY < 0) {
                        const shift = Math.abs(statesBounds.minY);
                        states.forEach(state => {
                            state.position.y += shift;
                        });
                        statesBounds.maxY += shift;
                        statesBounds.minY = 0;
                    }

                    svg.style.minWidth = Math.max(statesBounds.maxX, bladeRect.width) + 'px';
                    svg.style.minHeight = Math.max(statesBounds.maxY, bladeRect.height) + 'px';

                    stateMachineTransitionService.updateTransitionPaths(transitions, $scope, workspace);
                }
            }

            return {
                updateWorkspaceSize: updateWorkspaceSize
            };
        }
    ]);
