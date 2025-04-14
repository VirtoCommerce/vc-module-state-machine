angular.module('virtoCommerce.stateMachineModule')
    .controller('virtoCommerce.stateMachineModule.stateMachineVisualEditorController', [
        '$scope', '$element', '$timeout',
        'virtoCommerce.stateMachineModule.webApi',
        'virtoCommerce.stateMachineModule.stateMachineStateService',
        'virtoCommerce.stateMachineModule.stateMachineTransitionService',
        'virtoCommerce.stateMachineModule.stateMachineSnapshotService',
        'virtoCommerce.stateMachineModule.stateMachineJsonService',
        'virtoCommerce.stateMachineModule.stateMachineModalService',
        'virtoCommerce.stateMachineModule.stateMachineLayoutService',
        'virtoCommerce.stateMachineModule.stateMachineWorkspaceService',
        function ($scope, $element, $timeout,
            stateMachineApi,
            stateMachineStateService,
            stateMachineTransitionService,
            stateMachineSnapshotService,
            stateMachineJsonService,
            stateMachineModalService,
            stateMachineLayoutService,
            stateMachineWorkspaceService
        ) {
            var blade = $scope.blade;
            blade.scope = $scope;
            blade.headIcon = 'fas fa-project-diagram';
            blade.title = 'statemachine.blades.state-machine-visual-editor.title';
            blade.isInVisualMode = true;
            blade.isInJsonMode = false;
            blade.allLanguages = [];

            // State machine data
            blade.machineData = { states: [], transitions: [] };

            const stateHeight = 100;

            var oldStates = null;

            function initializeStateMachine() {
                if (!blade.currentEntity) return;

                stateMachineApi.getStateMachineSettings({}, function (data) {
                    blade.allLanguages = data.languages;
                });

                try {
                    const bladeContent = document.getElementById('visualEditorBlade');
                    var workspace = document.getElementById('visualEditorWorkspace');
                    if (!workspace) return;

                    if (bladeContent) {
                        const resizeObserver = new ResizeObserver(() => {
                            stateMachineWorkspaceService.updateWorkspaceSize(blade.machineData.states, blade.machineData.transitions, $scope, workspace);
                            stateMachineTransitionService.updateTransitionPaths(blade.machineData.transitions, $scope, workspace);
                        });
                        resizeObserver.observe(bladeContent);
                    }

                    const statesData = typeof blade.currentEntity === 'string'
                        ? JSON.parse(blade.currentEntity)
                        : blade.currentEntity;

                    blade.machineData.states = [];
                    blade.machineData.transitions = [];

                    // First pass: Create all states
                    const stateMap = new Map();
                    let needsNormalization = false;

                    statesData.forEach((stateData, index) => {
                        const hasValidPosition = stateData.position &&
                            typeof stateData.position.x === 'number' &&
                            typeof stateData.position.y === 'number' &&
                            stateData.position.x >= 0 &&
                            stateData.position.y >= 0;

                        if (!hasValidPosition) {
                            needsNormalization = true;
                        }

                        const state = {
                            id: stateData.id || stateData.name, // Fallback to name if id is not present
                            name: stateData.name,
                            description: stateData.description || '',
                            isInitial: stateData.isInitial || false,
                            isFinal: stateData.isFinal || false,
                            isSuccess: stateData.isSuccess || false,
                            isFailed: stateData.isFailed || false,
                            togglePosition: stateData.isSuccess ? 'left' : (stateData.isFailed ? 'right' : 'center'),
                            transitions: [], // Will be populated in second pass
                            position: hasValidPosition ? stateData.position : { x: 0, y: 0 } // Temporary position if invalid
                        };
                        stateMap.set(state.id, state);
                        blade.machineData.states.push(state);
                    });

                    // Second pass: Create transitions
                    statesData.forEach(stateData => {
                        const fromState = stateMap.get(stateData.id || stateData.name);
                        if (!fromState) return;

                        if (Array.isArray(stateData.transitions)) {
                            stateData.transitions.forEach(transitionData => {
                                const toStateId = typeof transitionData.toState === 'string'
                                    ? transitionData.toState
                                    : transitionData.toState.id || transitionData.toState.name;

                                const toState = stateMap.get(toStateId);
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

                    stateMachineStateService.updateStatesAttributes(blade.machineData.states);

                    stateMachineStateService.calculateStateLevels(blade.machineData.states);

                    $timeout(() => {
                        if (needsNormalization) {
                            stateMachineLayoutService.normalizeStateLayout(blade.machineData.states, blade.machineData.transitions, $scope, workspace);
                        }

                        $timeout(() => {
                            stateMachineWorkspaceService.updateWorkspaceSize(blade.machineData.states, blade.machineData.transitions, $scope, workspace);

                            $timeout(() => {
                                stateMachineTransitionService.updateTransitionPaths(blade.machineData.transitions, $scope, workspace);
                            }, 50);
                        }, 100);
                    }, 100);

                } catch (error) {
                    console.error('Error initializing state machine:', error);
                    console.error('Data:', blade.currentEntity);
                }
            }

            blade.refresh = function () {
                blade.isLoading = false;
                initializeStateMachine();

                const jsonEditor = document.getElementById('jsonEditor');
                if (!jsonEditor) return;
                stateMachineJsonService.initializeJsonEditor(jsonEditor, $scope, blade);
                blade.origEntity = angular.copy(blade.currentEntity);
            };

            blade.toolbarCommands = [
                {
                    name: "statemachine.blades.state-machine-visual-editor.commands.add-state",
                    icon: 'fas fa-plus',
                    executeMethod: function () {
                        $scope.addState();
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "platform.commands.reset",
                    icon: 'fa fa-undo',
                    executeMethod: function () {
                        const origData = typeof blade.origEntity === 'string' ? blade.origEntity : JSON.stringify(blade.origEntity);
                        blade.currentEntity = origData;
                        initializeStateMachine();
                    },
                    canExecuteMethod: isDirty
                },
                {
                    name: "statemachine.blades.state-machine-visual-editor.commands.toggle-to-visual-mode",
                    icon: 'fas fa-project-diagram',
                    executeMethod: toggleToVisualMode,
                    canExecuteMethod: function () {
                        return blade.isInJsonMode;
                    }
                },
                {
                    name: "statemachine.blades.state-machine-visual-editor.commands.toggle-to-json-mode",
                    icon: 'fas fa-code',
                    executeMethod: toggleToJsonMode,
                    canExecuteMethod: function () {
                        return blade.isInVisualMode;
                    }
                //},
                //{
                //    name: "Snapshot",
                //    icon: 'fa fa-camera',
                //    executeMethod: makeSnaphot,
                //    canExecuteMethod: function () {
                //        return true;
                //    }
                }
            ];

            $scope.setForm = function (form) {
                $scope.formScope = form;
            }

            blade.onClose = async function (closeCallback) {
                if (isDirty()) {
                    if (!blade.isInVisualMode) {
                        toggleToVisualMode();
                    }
                    blade.recalculateStatePositions();
                    await blade.makeSnapshot();
                }
                closeCallback();
            };

            function isDirty() {
                if (!blade.isLoading
                    && blade.machineData.states.length > 0
                    && blade.machineData.states !== oldStates) {
                    updateCurrentEntity();
                    oldStates = angular.copy(blade.machineData.states);
                };
                return !angular.equals(blade.currentEntity, blade.origEntity);
            }

            function toggleToVisualMode() {
                blade.isInJsonMode = false;
                blade.isInVisualMode = true;
                blade.headIcon = 'fas fa-project-diagram';

                const jsonEditorContainer = document.getElementById('jsonEditorContainer');
                const visualEditor = document.getElementById('visualEditorWorkspace');

                if (jsonEditorContainer) {
                    jsonEditorContainer.style.display = 'none';
                }
                if (visualEditor) {
                    visualEditor.style.display = 'block';
                    initializeStateMachine();
                }
            }

            function toggleToJsonMode() {
                blade.isInJsonMode = true;
                blade.isInVisualMode = false;
                blade.headIcon = 'fas fa-code';

                const jsonEditorContainer = document.getElementById('jsonEditorContainer');
                const jsonEditor = document.getElementById('jsonEditor');
                const visualEditor = document.getElementById('visualEditorWorkspace');

                if (!jsonEditorContainer) {
                    stateMachineJsonService.initializeJsonEditor(jsonEditor, $scope, blade);
                } else {
                    jsonEditorContainer.style.display = 'block';
                    if (jsonEditor) {
                        const formattedJson = JSON.stringify(JSON.parse(blade.currentEntity), null, 2);
                        jsonEditor.value = formattedJson;

                        jsonEditor.addEventListener('input', () => {
                            stateMachineJsonService.refreshJsonEditor(jsonEditor, $scope, blade);
                        });

                        jsonEditor.addEventListener('blur', () => {
                            stateMachineJsonService.refreshJsonEditor(jsonEditor, $scope, blade);
                        });
                    }
                }

                if (visualEditor) {
                    visualEditor.style.display = 'none';
                }
            }

            blade.recalculateStatePositions = function () {
                if (!isDirty()) {
                    return;
                }
                let minX = Infinity, minY = Infinity;
                const margin = 50;
                blade.machineData.states.forEach(state => {
                    minX = Math.min(minX, state.position.x);
                    minY = Math.min(minY, state.position.y);
                });
                blade.machineData.states.forEach(state => {
                    state.position.x -= minX - margin;
                    state.position.y -= minY - margin;
                });
                updateCurrentEntity();
            }

            blade.makeSnapshot = async function () {
                if (!isDirty()) {
                    return;
                }

                blade.scope.contextMenuData = null;
                stateMachineTransitionService.removeTempLine($scope);

                blade.isLoading = true;

                const workspace = document.getElementById('visualEditorWorkspace');
                if (!workspace) {
                    console.error('Workspace not found');
                    return;
                }

                const imageData = await stateMachineSnapshotService.makeSnapshot(blade.machineData.states, workspace);
                if (blade.parentBlade.updateStateMachineSnapshot) {
                    blade.parentBlade.updateStateMachineSnapshot(imageData);
                }

                blade.isLoading = false;
            }

            $scope.addState = function () {
                let newX = 100;
                let newY = 50;

                if (blade.machineData.states.length > 0) {
                    const lastState = blade.machineData.states[blade.machineData.states.length - 1];
                    newX = lastState.position.x;
                    newY = lastState.position.y + 100 + stateHeight;
                }

                stateMachineModalService.editStateModal($scope, $element, newX, newY, null, null, blade.machineData.states, blades.machineData.transitions);
            };

            function updateCurrentEntity() {
                const serializedStates = blade.machineData.states.map(state => ({
                    id: state.id,
                    name: state.name,
                    description: state.description || '',
                    isInitial: state.isInitial,
                    isFinal: state.isFinal,
                    isSuccess: state.isSuccess,
                    isFailed: state.isFailed,
                    position: state.position,
                    transitions: state.transitions.map(t => ({
                        id: t.id,
                        trigger: t.trigger,
                        icon: t.icon || '',
                        description: t.description || '',
                        toState: t.toState.id
                    }))
                }));

                blade.currentEntity = JSON.stringify(serializedStates);
                blade.parentBlade.updateStateMachineData(blade.currentEntity);
            }

            blade.getCurrentTranslations = async function (item) {
                var itemText = item.name || item.trigger;
                var searchCriteria = {
                    definitionId: blade.stateMachineDefinitionId,
                    item: itemText
                };
                var localizationSearchResult = await stateMachineApi.searchStateMachineLocalization(searchCriteria).$promise;
                var currentTranslations = localizationSearchResult ? localizationSearchResult.results : [];

                return currentTranslations;
            }

            blade.saveCurrentTranslations = function (localizations) {
                if (localizations) {
                    localizations = localizations.filter(x => x.value !== undefined && x.value !== null && x.value !== '');
                    localizations.forEach(x => {
                        x.definitionId = blade.stateMachineDefinitionId;
                    });
                    stateMachineApi.updateStateMachineLocalization({ localizations: localizations });
                }
            }

            blade.refresh();
        }
    ]);
