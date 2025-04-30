angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineLayoutService', [
        '$timeout',
        'virtoCommerce.stateMachineModule.stateMachineWorkspaceService',
        'virtoCommerce.stateMachineModule.stateMachineStateService',
        function ($timeout,
            stateMachineWorkspaceService,
            stateMachineStateService
        ) {
            const stateWidth = 150;
            const stateHeight = 100;
            var horizontalSpacing = 200;
            const verticalSpacing = 150;

            function normalizeStateLayout(states, transitions, $scope, workspace) {
                var workspace = document.getElementById('visualEditorWorkspace');
                if (!workspace) return;

                // Update workspace size before normalization
                stateMachineWorkspaceService.updateWorkspaceSize(states, transitions, $scope, workspace);

                stateMachineStateService.calculateStateLevels(states);

                const statesByLevel = {};
                states.forEach(state => {
                    if (!statesByLevel[state.level]) {
                        statesByLevel[state.level] = [];
                    }
                    statesByLevel[state.level].push(state);
                });

                const sortedLevels = Object.keys(statesByLevel).sort((a, b) => a - b);

                if (statesByLevel['1']) {
                    const initialStates = statesByLevel['1'];
                    const totalInitialStates = initialStates.length;

                    const availableWidth = workspace.clientWidth;
                    const totalStateWidth = totalInitialStates * stateWidth;
                    const totalSpacingNeeded = availableWidth - totalStateWidth;
                    const spacingBetweenStates = Math.max(horizontalSpacing, totalSpacingNeeded / (totalInitialStates + 1));

                    initialStates.forEach((state, index) => {
                        state.position.x = spacingBetweenStates + index * (stateWidth + spacingBetweenStates);
                        state.position.y = 50;
                    });
                }

                let maxIterations = 3;
                let currentIteration = 0;
                let previousIntersections = Infinity;

                while (currentIteration < maxIterations) {
                    sortedLevels.forEach((level, levelIndex) => {
                        if (level === '1') return;

                        const statesInLevel = statesByLevel[level];
                        const levelY = 50 + levelIndex * (stateHeight + verticalSpacing);

                        statesInLevel.sort((stateA, stateB) => {
                            const getParentX = (state) => {
                                const parents = states.filter(s =>
                                    s.transitions.some(t => t.toState === state)
                                );
                                if (parents.length === 0) return 0;
                                return parents.reduce((sum, p) => sum + p.position.x, 0) / parents.length;
                            };

                            return getParentX(stateA) - getParentX(stateB);
                        });

                        const statesByParent = {};
                        statesInLevel.forEach(state => {
                            const parents = states.filter(s =>
                                s.transitions.some(t => t.toState === state)
                            );
                            const parentKey = parents.map(p => p.id).sort().join(',');
                            if (!statesByParent[parentKey]) {
                                statesByParent[parentKey] = [];
                            }
                            statesByParent[parentKey].push(state);
                        });

                        let currentX = 50;
                        Object.values(statesByParent).forEach(groupStates => {
                            const groupWidth = groupStates.length * stateWidth +
                                (groupStates.length - 1) * horizontalSpacing;
                            const parentX = groupStates.reduce((sum, state) => {
                                const parents = states.filter(s =>
                                    s.transitions.some(t => t.toState === state)
                                );
                                return sum + parents.reduce((pSum, p) => pSum + p.position.x, 0) /
                                    (parents.length || 1);
                            }, 0) / groupStates.length;

                            const groupStartX = Math.max(currentX, parentX - groupWidth / 2);
                            groupStates.forEach((state, index) => {
                                state.position.x = groupStartX + index * (stateWidth + horizontalSpacing);
                                state.position.y = levelY;
                            });

                            currentX = groupStartX + groupWidth + horizontalSpacing;
                        });
                    });

                    const currentIntersections = checkTransitionIntersections(states);

                    if (currentIntersections >= previousIntersections || currentIntersections === 0) {
                        break;
                    }

                    horizontalSpacing += 50;
                    previousIntersections = currentIntersections;
                    currentIteration++;
                }

                sortedLevels.forEach(level => {
                    const statesInLevel = statesByLevel[level];
                    statesInLevel.sort((a, b) => a.position.x - b.position.x);

                    for (let i = 1; i < statesInLevel.length; i++) {
                        const minSpace = stateWidth + horizontalSpacing;
                        const overlap = minSpace - (statesInLevel[i].position.x - statesInLevel[i - 1].position.x);
                        if (overlap > 0) {
                            statesInLevel[i].position.x += overlap;
                        }
                    }
                });

                let minX = Infinity, maxX = -Infinity;
                states.forEach(state => {
                    minX = Math.min(minX, state.position.x);
                    maxX = Math.max(maxX, state.position.x + stateWidth);
                });

                const graphWidth = maxX - minX;
                const workspaceWidth = workspace.clientWidth;

                if (graphWidth < workspaceWidth) {
                    const offset = (workspaceWidth - graphWidth) / 2 - minX;
                    states.forEach(state => {
                        state.position.x += offset;
                    });
                } else {
                    const rightMargin = 50;
                    workspace.style.width = (graphWidth + rightMargin) + 'px';
                }

                let maxY = -Infinity;
                states.forEach(state => {
                    maxY = Math.max(maxY, state.position.y + stateHeight);
                });

                workspace.style.width = '100%';
            }

            function doLinesIntersect(x1, y1, x2, y2, x3, y3, x4, y4) {
                const denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
                if (denom === 0) return false;

                const ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denom;
                const ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denom;

                return ua > 0 && ua < 1 && ub > 0 && ub < 1;
            }

            function transitionCrossesState(fromX, fromY, toX, toY, state) {
                if (state.position.x <= Math.max(fromX, toX) &&
                    state.position.x + stateWidth >= Math.min(fromX, toX) &&
                    state.position.y <= Math.max(fromY, toY) &&
                    state.position.y + stateHeight >= Math.min(fromY, toY)) {

                    const stateLeft = state.position.x;
                    const stateRight = state.position.x + stateWidth;
                    const stateTop = state.position.y;
                    const stateBottom = state.position.y + stateHeight;

                    return doLinesIntersect(fromX, fromY, toX, toY, stateLeft, stateTop, stateRight, stateTop) ||
                        doLinesIntersect(fromX, fromY, toX, toY, stateRight, stateTop, stateRight, stateBottom) ||
                        doLinesIntersect(fromX, fromY, toX, toY, stateRight, stateBottom, stateLeft, stateBottom) ||
                        doLinesIntersect(fromX, fromY, toX, toY, stateLeft, stateBottom, stateLeft, stateTop);
                }
                return false;
            }

            function checkTransitionIntersections(states) {
                let intersections = 0;
                let transitions = [];

                states.forEach(state => {
                    state.transitions.forEach(transition => {
                        transitions.push({
                            from: state,
                            to: transition.toState,
                            fromX: state.position.x + stateWidth / 2,
                            fromY: state.position.y + stateHeight,
                            toX: transition.toState.position.x + stateWidth / 2,
                            toY: transition.toState.position.y
                        });
                    });
                });

                for (let i = 0; i < transitions.length; i++) {
                    for (let j = i + 1; j < transitions.length; j++) {
                        if (doLinesIntersect(
                            transitions[i].fromX, transitions[i].fromY,
                            transitions[i].toX, transitions[i].toY,
                            transitions[j].fromX, transitions[j].fromY,
                            transitions[j].toX, transitions[j].toY
                        )) {
                            intersections++;
                        }
                    }

                    states.forEach(state => {
                        if (state !== transitions[i].from && state !== transitions[i].to) {
                            if (transitionCrossesState(
                                transitions[i].fromX, transitions[i].fromY,
                                transitions[i].toX, transitions[i].toY,
                                state
                            )) {
                                intersections++;
                            }
                        }
                    });
                }

                return intersections;
            }

            return {
                normalizeStateLayout: normalizeStateLayout
            };
        }
    ]);
