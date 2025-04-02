angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineTransitionService', [
        '$timeout',
        'virtoCommerce.stateMachineModule.stateMachineStateService',
        function ($timeout,
            stateMachineStateService
            ) {
            const stateWidth = 150;
            const stateHeight = 100;

            function updateTransitionPaths(transitions, $scope, workspace) {
                transitions.forEach(function (transition) {
                    updateTransitionPath(transition, transitions, $scope, workspace);
                });
            };

            function updateTransitionPath(transition, transitions, $scope, workspace) {
                const fromState = transition.fromState;
                const toState = transition.toState;

                if (!fromState.position || !toState.position) {
                    console.warn('States do not have valid positions:', fromState, toState);
                    return;
                }

                const startX = fromState.position.x + stateWidth / 2;
                const startY = fromState.position.y + stateHeight;
                const endX = toState.position.x + stateWidth / 2;
                const endY = toState.position.y;

                const deltaX = endX - startX;
                const deltaY = endY - startY;
                const distance = Math.sqrt(deltaX * deltaX + deltaY * deltaY);

                const curveIntensity = Math.min(Math.max(distance * 0.2, 30), 100);

                const cp1y = startY + curveIntensity;
                const cp2y = endY - curveIntensity;

                const t = 0.5;
                const labelX = startX * Math.pow(1 - t, 3) +
                    3 * startX * Math.pow(1 - t, 2) * t +
                    3 * endX * (1 - t) * Math.pow(t, 2) +
                    endX * Math.pow(t, 3);

                const labelY = startY * Math.pow(1 - t, 3) +
                    3 * cp1y * Math.pow(1 - t, 2) * t +
                    3 * cp2y * (1 - t) * Math.pow(t, 2) +
                    endY * Math.pow(t, 3);

                transition.path = `M ${startX} ${startY} C ${startX} ${cp1y}, ${endX} ${cp2y}, ${endX} ${endY}`;
                transition.labelPosition = { x: labelX, y: labelY };

                $scope.$evalAsync(() => {
                    const transitionIndex = transitions.indexOf(transition);
                    const transitionGroup = workspace.querySelector(`g[ng-repeat="transition in transitions"]:nth-child(${transitionIndex + 1})`);

                    if (transitionGroup) {
                        const pathEl = transitionGroup.querySelector('path');
                        if (pathEl) {
                            pathEl.setAttribute('d', transition.path);
                        }

                        const labelBgEl = transitionGroup.querySelector('rect');
                        if (labelBgEl) {
                            labelBgEl.setAttribute('x', labelX - 50);
                            labelBgEl.setAttribute('y', labelY - 10);
                            labelBgEl.setAttribute('width', '100');
                            labelBgEl.setAttribute('height', '20');
                        }

                        const labelEl = transitionGroup.querySelector('text');
                        if (labelEl) {
                            labelEl.setAttribute('x', labelX);
                            labelEl.setAttribute('y', labelY + 5);
                            labelEl.textContent = transition.trigger;
                        }
                    }
                });
            };

            function deleteTransition(states, transition, transitions) {
                const fromState = transition.fromState;

                const transitionIndex = transitions.indexOf(transition);
                if (transitionIndex > -1) {
                    transitions.splice(transitionIndex, 1);
                }

                const stateTransitionIndex = fromState.transitions.indexOf(transition);
                if (stateTransitionIndex > -1) {
                    fromState.transitions.splice(stateTransitionIndex, 1);
                }

                stateMachineStateService.updateStatesAttributes(states);
                stateMachineStateService.calculateStateLevels(states);
            };

            function removeTempLine($scope) {
                var workspace = document.getElementById('visualEditorWorkspace');
                var svg = workspace.querySelector('svg');
                if ($scope.tempLine) {
                    svg.removeChild($scope.tempLine);
                    $scope.tempLine = null;
                }
            };

            function generateUniqueId() {
                return 'transition_' + Math.random().toString(36).substr(2, 9);
            };

            return {
                updateTransitionPaths: updateTransitionPaths,
                updateTransitionPath: updateTransitionPath,
                deleteTransition: deleteTransition,
                removeTempLine: removeTempLine,
                generateUniqueId: generateUniqueId
            };
        }
    ]);
