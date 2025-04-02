angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineStateService', [
        '$timeout',
        function ($timeout) {
            const stateWidth = 150;
            const stateHeight = 100;

            function calculateBounds(states, margin, needSetToZero) {
                let minX = Infinity, minY = Infinity;
                let maxX = -Infinity, maxY = -Infinity;

                states.forEach(state => {
                    minX = Math.min(minX, state.position.x);
                    minY = Math.min(minY, state.position.y);
                    maxX = Math.max(maxX, state.position.x + stateWidth);
                    maxY = Math.max(maxY, state.position.y + stateHeight);
                });

                minX = needSetToZero ? Math.max(0, minX - margin) : minX - margin;
                minY = needSetToZero ? Math.max(0, minY - margin) : minY - margin;
                maxX = maxX + margin;
                maxY = maxY + margin;

                return { minX: minX, minY: minY, maxX: maxX, maxY: maxY };
            };

            function calculateStateLevels(states) {
                states.forEach(state => state.level = 0);

                states.filter(s => s.isInitial).forEach(s => s.level = 1);

                let changed = true;
                while (changed) {
                    changed = false;
                    states.forEach(state => {
                        const incomingStates = states.filter(s =>
                            s.transitions.some(t => t.toState.id === state.id)
                        );

                        if (incomingStates.length > 0) {
                            const maxIncomingLevel = Math.max(...incomingStates.map(s => s.level));
                            const newLevel = maxIncomingLevel + 1;

                            if (newLevel > state.level) {
                                state.level = newLevel;
                                changed = true;
                            }
                        }
                    });
                }
            };

            function updateStatesAttributes(states) {
                states.forEach(state => {
                    updateStateAttributes(state, states)
                });
            };

            function updateStateAttributes(state, states) {
                const hasIncoming = states.some(s =>
                    s.transitions.some(t => t.toState.id === state.id)
                );

                const hasOutgoing = state.transitions.length > 0;

                state.isInitial = !hasIncoming;
                state.isFinal = !hasOutgoing;

                // Only allow success/fail states for final states
                if (!state.isFinal) {
                    state.isSuccess = false;
                    state.isFailed = false;
                }
            };

            function buildStateFromJSON(statesData) { //TODO: use in initial load
                blade.machineData.states = [];
                blade.machineData.transitions = [];

                // First pass: Create all states
                const stateMap = new Map();

                statesData.forEach((stateData) => {
                    const state = {
                        id: stateData.id,
                        name: stateData.name,
                        description: stateData.description || '',
                        isInitial: stateData.isInitial,
                        isFinal: stateData.isFinal,
                        isSuccess: stateData.isSuccess,
                        isFailed: stateData.isFailed,
                        position: stateData.position,
                        transitions: [],
                        togglePosition: stateData.isSuccess ? 'left' : (stateData.isFailed ? 'right' : 'center')
                    };
                    stateMap.set(state.id, state);
                    blade.machineData.states.push(state);
                });

                // Second pass: Create transitions
                statesData.forEach(stateData => {
                    const fromState = stateMap.get(stateData.id);
                    if (!fromState) return;

                    // Handle transitions array
                    if (Array.isArray(stateData.transitions)) {
                        stateData.transitions.forEach(transitionData => {
                            const toState = stateMap.get(transitionData.toState);
                            if (!toState) return;

                            const transition = {
                                id: transitionData.id || stateMachineTransitionService.generateUniqueId(),
                                trigger: transitionData.trigger,
                                icon: transitionData.icon || '',
                                description: transitionData.description || '',
                                fromState: fromState,
                                toState: toState,
                                path: '', // Will be calculated by updateTransitionPath
                                labelPosition: {} // Will be calculated by updateTransitionPath
                            };

                            blade.machineData.transitions.push(transition);
                            fromState.transitions.push(transition);
                        });
                    }
                });

                // Update states attributes
                stateMachineStateService.updateStatesAttributes(blade.machineData.states);
                stateMachineStateService.calculateStateLevels(blade.machineData.states);
            }

            return {
                calculateBounds: calculateBounds,
                calculateStateLevels: calculateStateLevels,
                updateStatesAttributes: updateStatesAttributes,
                updateStateAttributes: updateStateAttributes,
                buildStateFromJSON: buildStateFromJSON
            };
        }
    ]);
