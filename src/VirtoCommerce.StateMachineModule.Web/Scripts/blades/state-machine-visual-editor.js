angular.module('virtoCommerce.stateMachineModule')
    .directive('ngRightClick', ['$parse', function($parse) {
        return {
            restrict: 'A',
            link: function(scope, element, attrs) {
                var fn = $parse(attrs.ngRightClick);
                element.on('contextmenu', function(event) {
                    event.preventDefault();
                    scope.$apply(function() {
                        fn(scope, {$event: event});
                    });
                    return false;
                });
            }
        };
    }])
    .directive('threePositionToggle', function() {
        return {
            restrict: 'E',
            scope: {
                toggleId: '@',
                leftLabel: '@',
                rightLabel: '@',
                ngModel: '=',
                onChange: '&'
            },
            template: `
                <div class="toggle-container">
                    <span class="toggle-side-label left">{{leftLabel}}</span>
                    <div class="three-position-toggle" ng-class="ngModel || 'center'" ng-click="togglePosition($event)">
                        <div class="toggle-option left"><i class="fas fa-check"></i></div>
                        <div class="toggle-option center"></div>
                        <div class="toggle-option right"><i class="fas fa-ban"></i></div>
                    </div>
                    <span class="toggle-side-label right">{{rightLabel}}</span>
                </div>
            `,
            link: function(scope) {
                scope.togglePosition = function(event) {
                    const toggle = event.currentTarget;
                    const rect = toggle.getBoundingClientRect();
                    const clickX = event.clientX - rect.left;
                    const width = rect.width;

                    // Calculate which part was clicked
                    if (clickX < width / 3) {
                        scope.ngModel = 'left';
                    } else if (clickX > (width * 2 / 3)) {
                        scope.ngModel = 'right';
                    } else {
                        scope.ngModel = 'center';
                    }

                    scope.onChange({ $position: scope.ngModel });
                };
            }
        };
    })
    .controller('virtoCommerce.stateMachineModule.stateMachineVisualEditorController', [
        '$scope', '$element', '$timeout',
        'platformWebApp.assets.api',
        function ($scope, $element, $timeout,
            assetsApi) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-project-diagram';
            blade.title = 'statemachine.blades.state-machine-visual-editor.title';
            blade.isInVisualMode = true;
            blade.isInJsonMode = false;

            // State machine data
            $scope.states = [];
            $scope.transitions = [];

            // Constants for layout
            const stateWidth = 150;
            const stateHeight = 100;
            const horizontalSpacing = 200;
            const verticalSpacing = 150;

            function initializeJsonEditor() {
                const jsonEditor = document.getElementById('jsonEditor');
                if (!jsonEditor) return;

                // Set initial JSON content
                const formattedJson = JSON.stringify(JSON.parse(blade.currentEntity), null, 2);
                jsonEditor.value = formattedJson;

                jsonEditor.addEventListener('input', () => {
                    refreshJsonEditor();
                });

                jsonEditor.addEventListener('blur', () => {
                    refreshJsonEditor();
                });

            }

            function refreshJsonEditor() {
                const jsonEditor = document.getElementById('jsonEditor');
                if (!jsonEditor) return;

                try {
                    const jsonContent = jsonEditor.value;
                    const parsedJson = JSON.parse(jsonContent);

                    // Update blade.currentEntity
                    $scope.$apply(() => {
                        blade.currentEntity = JSON.stringify(parsedJson);
                        blade.parentBlade.updateStateMachineData(blade.currentEntity);

                        // If we're in visual mode, reinitialize the state machine
                        if (blade.isInVisualMode) {
                            initializeStateMachine();
                        }
                    });

                    // Remove error styling if present
                    jsonEditor.style.border = '1px solid #ccc';
                } catch (error) {
                    // Add error styling to indicate invalid JSON
                    jsonEditor.style.border = '1px solid #ff0000';
                }
            }

            function initializeStateMachine() {
                if (!blade.currentEntity) return;

                try {
                    // Add resize observer to handle blade size changes
                    const bladeContent = document.querySelector('.blade-content');
                    if (bladeContent) {
                        const resizeObserver = new ResizeObserver(() => {
                            updateWorkspaceSize();
                            updateTransitionPaths();
                        });
                        resizeObserver.observe(bladeContent);
                    }

                    // Initialize workspace context menu
                    initializeWorkspaceContextMenu();

                    // Parse the JSON string if it's a string
                    const statesData = typeof blade.currentEntity === 'string'
                        ? JSON.parse(blade.currentEntity)
                        : blade.currentEntity;

                    // Clear existing states and transitions
                    $scope.states = [];
                    $scope.transitions = [];

                    // First pass: Create all states
                    const stateMap = new Map();
                    let needsNormalization = false;

                    statesData.forEach((stateData, index) => {
                        // Check if position is valid
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
                        $scope.states.push(state);
                    });

                    // Second pass: Create transitions
                    statesData.forEach(stateData => {
                        const fromState = stateMap.get(stateData.id || stateData.name);
                        if (!fromState) return;

                        // Handle transitions array
                        if (Array.isArray(stateData.transitions)) {
                            stateData.transitions.forEach(transitionData => {
                                const toStateId = typeof transitionData.toState === 'string'
                                    ? transitionData.toState
                                    : transitionData.toState.id || transitionData.toState.name;

                                const toState = stateMap.get(toStateId);
                                if (!toState) return;

                                const transition = {
                                    id: transitionData.id || generateUniqueId(),
                                    trigger: transitionData.trigger,
                                    icon: transitionData.icon || '',
                                    description: transitionData.description || '',
                                    fromState: fromState,
                                    toState: toState,
                                    path: '', // Will be calculated by updateTransitionPath
                                    labelPosition: {} // Will be calculated by updateTransitionPath
                                };

                                $scope.transitions.push(transition);
                                fromState.transitions.push(transition);
                            });
                        }
                    });

                    // Update states attributes
                    updateStatesAttributes();

                    // Calculate state levels
                    calculateStateLevels();

                    // Use $timeout to ensure DOM is ready and trigger a digest cycle
                    $timeout(() => {
                        // If any state had invalid position, normalize the layout
                        if (needsNormalization) {
                            normalizeStateLayout();
                        }

                        // Wait for DOM update after normalization
                        $timeout(() => {
                            // Update workspace size first
                            updateWorkspaceSize();

                            // Then update transition paths
                            $timeout(() => {
                                updateTransitionPaths();
                            }, 50);
                        }, 100);
                    }, 0);
                } catch (error) {
                    console.error('Error initializing state machine:', error);
                    console.error('Data:', blade.currentEntity);
                }
            }

            blade.refresh = function () {
                initializeStateMachine();
                initializeJsonEditor(); // Create JSON editor during initialization
                blade.origEntity = angular.copy(blade.currentEntity);
                blade.isLoading = false;
            };

            // Add toggle change handler
            $scope.onStateToggleChange = function(state, newPosition) {
                saveCurrentState();

                // Update state flags based on position
                if (newPosition === 'left') {
                    state.isSuccess = true;
                    state.isFailed = false;
                } else if (newPosition === 'right') {
                    state.isSuccess = false;
                    state.isFailed = true;
                } else {
                    state.isSuccess = false;
                    state.isFailed = false;
                }

                // Update state's classes
                const stateEl = document.getElementById('state' + state.id);
                if (stateEl) {
                    stateEl.className = `state-node ${state.isInitial ? 'is-initial' : ''} ${state.isFinal ? 'is-final' : ''} ${state.isSuccess ? 'is-success' : ''} ${state.isFailed ? 'is-failed' : ''}`;
                }

                // Update state attributes and current entity
                updateStateAttributes(state);
                updateCurrentEntity();
            };

            // Update IsInitial and IsFinal attributes
            function updateStateAttributes(state) {
                const hasIncoming = $scope.states.some(s =>
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
            }

            function updateStatesAttributes() {
                $scope.states.forEach(state => {
                    updateStateAttributes(state)
                });
            }

            // Add editState handler
            $scope.editState = function(e, state) {
                if (e) {
                    e.preventDefault();
                    e.stopPropagation();
                }

                showStateModal(state.position.x, state.position.y, null, state);
            };

            // Add editTransition handler
            $scope.editTransition = function(e, transition) {
                if (e) {
                    e.preventDefault();
                    e.stopPropagation();
                }

                showTransitionModal(transition, (transitionData) => {
                    $scope.$apply(() => {
                        transition.trigger = transitionData.trigger;
                        transition.icon = transitionData.icon;
                        transition.description = transitionData.description;
                    });
                });
            };

            $scope.onStateHover = function(state, isHovered) {
                const stateEl = document.getElementById('state' + state.id);
                if (stateEl) {
                    if (isHovered) {
                        stateEl.style.border = '2px solid #242424';
                        stateEl.style.boxShadow = '5px 5px 6px rgba(0, 0, 0, 0.2)';
                    } else {
                        stateEl.style.border = '';
                        stateEl.style.boxShadow = '2px 2px 5px rgba(0, 0, 0, 0.2)';
                    }
                }
                const connectedTransitions = $scope.transitions.filter(x => x.fromState.id === state.id || x.toState.id === state.id);
                connectedTransitions.forEach(transition =>{
                    const transitionEl = document.getElementById(transition.id);
                    if (transitionEl) {
                        if (isHovered) {
                            transitionEl.classList.add('hovered');
                        } else {
                            transitionEl.classList.remove('hovered');
                        }
                    }
                });
            };

            // Add hover handler
            $scope.onTransitionHover = function(transition, isHovered) {
                const workspace = document.getElementById('visualEditorWorkspace');
                if (!workspace) return;

                // Find the transition group and update its hover state
                const transitionGroup = workspace.querySelector(`g[ng-repeat="transition in transitions"]:nth-child(${$scope.transitions.indexOf(transition) + 1})`);
                if (transitionGroup) {
                    if (isHovered) {
                        transitionGroup.classList.add('hovered');
                    } else {
                        transitionGroup.classList.remove('hovered');
                    }
                }

                const fromStateEl = document.getElementById('state' + transition.fromState.id);
                const toStateEl = document.getElementById('state' + transition.toState.id);

                if (fromStateEl) {
                    if (isHovered) {
                        fromStateEl.classList.add('hovered');
                        fromStateEl.style.border = '2px solid #242424';
                        fromStateEl.style.boxShadow = '5px 5px 6px rgba(0, 0, 0, 0.2)';
                    } else {
                        fromStateEl.classList.remove('hovered');
                        fromStateEl.style.border = '';
                        fromStateEl.style.boxShadow = '2px 2px 5px rgba(0, 0, 0, 0.2)';
                    }
                }

                if (toStateEl) {
                    if (isHovered) {
                        toStateEl.classList.add('hovered');
                        toStateEl.style.border = '2px solid #242424';
                        toStateEl.style.boxShadow = '5px 5px 6px rgba(0, 0, 0, 0.2)';
                    } else {
                        toStateEl.classList.remove('hovered');
                        toStateEl.style.border = '';
                        toStateEl.style.boxShadow = '2px 2px 5px rgba(0, 0, 0, 0.2)';
                    }
                }
            };

            // Expose showTransitionContextMenu to the scope
            $scope.showTransitionContextMenu = function(e, transition) {
                // Ensure browser's context menu is prevented
                if (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    e.returnValue = false;
                }

                // Remove any existing context menu
                removeContextMenu();

                // Get the workspace element and its position
                const workspace = document.getElementById('visualEditorWorkspace');
                const rect = workspace.getBoundingClientRect();

                const menu = document.createElement('div');
                menu.className = 'context-menu';
                menu.style.position = 'absolute';

                // Calculate position relative to the workspace
                const x = e.clientX - rect.left + workspace.scrollLeft;
                const y = e.clientY - rect.top + workspace.scrollTop;

                menu.style.left = `${x}px`;
                menu.style.top = `${y}px`;
                menu.style.zIndex = '1000';

                const editItem = document.createElement('div');
                editItem.className = 'context-menu-item';
                editItem.innerHTML = '<i class="fas fa-edit"></i>Edit transition';
                editItem.onclick = () => {
                    removeContextMenu();
                    showTransitionModal(transition, (transitionData) => {
                        $scope.$apply(() => {
                            transition.trigger = transitionData.trigger;
                            transition.icon = transitionData.icon;
                            transition.description = transitionData.description;
                        });
                    });
                };

                const deleteItem = document.createElement('div');
                deleteItem.className = 'context-menu-item';
                deleteItem.innerHTML = '<i class="fas fa-trash"></i>Delete transition';
                deleteItem.onclick = () => {
                    removeContextMenu();
                    deleteTransition(transition);
                };

                const insertStateItem = document.createElement('div');
                insertStateItem.className = 'context-menu-item';
                insertStateItem.innerHTML = '<i class="fas fa-plus"></i>Insert state here';
                insertStateItem.onclick = () => {
                    removeContextMenu();
                    // Calculate position between source and target states
                    const sourceState = transition.fromState;
                    const targetState = transition.toState;
                    const newX = (sourceState.position.x + targetState.position.x) / 2;
                    const newY = (sourceState.position.y + targetState.position.y) / 2;

                    showStateModal(newX, newY, null, null, (newState) => {
                        // After new state is created, show transition modal
                        showTransitionModal(null, (transitionData) => {
                            // Create new transition from new state to target state
                            const newTransition = {
                                id: generateUniqueId(),
                                trigger: transitionData.trigger,
                                icon: transitionData.icon || '',
                                description: transitionData.description || '',
                                fromState: newState,
                                toState: targetState
                            };

                            // Update the original transition to point to the new state
                            transition.toState = newState;

                            $scope.$apply(() => {
                                // Add new transition
                                $scope.transitions.push(newTransition);
                                newState.transitions.push(newTransition);

                                // Update states attributes and recalculate levels
                                updateStatesAttributes();
                                calculateStateLevels();

                                // Update blade.currentEntity
                                updateCurrentEntity();

                                // Update transition paths
                                updateTransitionPath(transition);
                                updateTransitionPath(newTransition);
                            });
                        });
                    });
                };

                menu.appendChild(editItem);
                menu.appendChild(deleteItem);
                menu.appendChild(insertStateItem);
                workspace.appendChild(menu);

                // Add click handler to document to close menu
                const closeMenu = (event) => {
                    if (!menu.contains(event.target)) {
                        removeContextMenu();
                        document.removeEventListener('click', closeMenu);
                    }
                };

                // Delay adding the click listener to prevent immediate closure
                setTimeout(() => {
                    document.addEventListener('click', closeMenu);
                }, 0);

                return false;
            };

            let isDragging = false;
            let isTransitioning = false;
            let transitionStartState = null;
            let tempLine = null;
            let lastSavedState = null;

            // Dragging state
            var dragState = null;
            var dragOffset = { x: 0, y: 0 };

            // Transition creation state
            var transitionStart = null;
            var transitionPreview = null;

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
                        // Handle string-based entities by parsing and stringifying
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
                await blade.makeSnaphot();
                closeCallback();
            };

            function isDirty() {
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
                    $timeout(() => {
                        initializeStateMachine();
                    });
                }
            }

            function toggleToJsonMode() {
                blade.isInJsonMode = true;
                blade.isInVisualMode = false;
                blade.headIcon = 'fas fa-code';

                const jsonEditorContainer = document.getElementById('jsonEditorContainer');
                const visualEditor = document.getElementById('visualEditorWorkspace');

                if (!jsonEditorContainer) {
                    initializeJsonEditor();
                } else {
                    jsonEditorContainer.style.display = 'block';
                    const jsonEditor = document.getElementById('jsonEditor');
                    if (jsonEditor) {
                        const formattedJson = JSON.stringify(JSON.parse(blade.currentEntity), null, 2);
                        jsonEditor.value = formattedJson;

                        jsonEditor.addEventListener('input', () => {
                            refreshJsonEditor();
                        });

                        jsonEditor.addEventListener('blur', () => {
                            refreshJsonEditor();
                        });
                    }
                }

                if (visualEditor) {
                    visualEditor.style.display = 'none';
                }
            }

            blade.makeSnaphot = async function () {
                if (!isDirty()) {
                    return;
                }

                removeContextMenu();
                removeTempLine();

                blade.isLoading = true;
                try {
                    const workspace = document.getElementById('visualEditorWorkspace');
                    if (!workspace) {
                        console.error('Workspace not found');
                        return;
                    }

                    // Calculate the bounds of all content
                    let minX = Infinity, minY = Infinity;
                    let maxX = -Infinity, maxY = -Infinity;

                    // Check all states
                    $scope.states.forEach(state => {
                        minX = Math.min(minX, state.position.x);
                        minY = Math.min(minY, state.position.y);
                        maxX = Math.max(maxX, state.position.x + stateWidth);
                        maxY = Math.max(maxY, state.position.y + stateHeight);
                    });

                    // Add 20px margin
                    const margin = 20;
                    minX = Math.max(0, minX - margin);
                    minY = Math.max(0, minY - margin);
                    maxX = maxX + margin;
                    maxY = maxY + margin;

                    const totalWidth = maxX - minX;
                    const totalHeight = maxY - minY;

                    // Create a hidden clone of the workspace
                    const hiddenWorkspace = workspace.cloneNode(true);
                    hiddenWorkspace.id = 'hidden-workspace';
                    hiddenWorkspace.style.position = 'fixed';
                    hiddenWorkspace.style.left = '-9999px';
                    hiddenWorkspace.style.top = '-9999px';
                    hiddenWorkspace.style.background = 'white';
                    hiddenWorkspace.style.width = totalWidth + 'px';
                    hiddenWorkspace.style.height = totalHeight + 'px';
                    hiddenWorkspace.style.overflow = 'visible';

                    // Update SVG dimensions in the clone
                    const hiddenSvg = hiddenWorkspace.querySelector('svg');
                    if (hiddenSvg) {
                        //hiddenSvg.style.left = (totalWidth - 9999) + 'px';
                        hiddenSvg.style.width = totalWidth + 'px';
                        //hiddenSvg.style.top = (totalHeight - 9999) + 'px';                        hiddenSvg.style.width = totalWidth + 'px';
                        hiddenSvg.style.height = totalHeight + 'px';
                        //hiddenSvg.setAttribute('left', totalWidth-9999);
                        hiddenSvg.setAttribute('width', totalWidth);
                        //hiddenSvg.setAttribute('top', totalHeight-9999);
                        hiddenSvg.setAttribute('height', totalHeight);
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
                        x: minX,
                        y: minY,
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
                        if (blade.parentBlade.updateStateMachineSnapshot) {
                            blade.parentBlade.updateStateMachineSnapshot(imageData);
                        }

                        // Always remove the hidden workspace
                        if (hiddenWorkspace && hiddenWorkspace.parentNode) {
                            hiddenWorkspace.parentNode.removeChild(hiddenWorkspace);
                        }

                } catch (error) {
                    console.error('Error in makeSnaphot:', error);
                }
                blade.isLoading = false;
            }

            // Add new state
            $scope.addState = function () {
                let newX = 100;
                let newY = 50;

                if ($scope.states.length > 0) {
                    const lastState = $scope.states[$scope.states.length - 1];
                    newX = lastState.position.x;
                    newY = lastState.position.y + 100 + stateHeight;
                }

                showStateModal(newX, newY);
            };

            // Add state context menu handler
            $scope.showStateContextMenu = function(e, state) {
                // Ensure browser's context menu is prevented
                if (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    e.returnValue = false;
                }

                // Remove any existing context menu
                removeContextMenu();

                // Get the workspace element and its position
                const workspace = document.getElementById('visualEditorWorkspace');
                const rect = workspace.getBoundingClientRect();

                const menu = document.createElement('div');
                menu.className = 'context-menu';
                menu.style.position = 'absolute';

                // Calculate position relative to the workspace
                const x = e.clientX - rect.left + workspace.scrollLeft;
                const y = e.clientY - rect.top + workspace.scrollTop;

                menu.style.left = `${x}px`;
                menu.style.top = `${y}px`;
                menu.style.zIndex = '1000';

                const editItem = document.createElement('div');
                editItem.className = 'context-menu-item';
                editItem.innerHTML = '<i class="fas fa-edit"></i>Edit state';
                editItem.onclick = () => {
                    removeContextMenu();
                    showStateModal(state.position.x, state.position.y, null, state);
                };

                const deleteItem = document.createElement('div');
                deleteItem.className = 'context-menu-item';
                deleteItem.innerHTML = '<i class="fas fa-trash"></i>Delete state';
                deleteItem.onclick = () => {
                    removeContextMenu();
                    deleteState(state);
                };

                menu.appendChild(editItem);
                menu.appendChild(deleteItem);
                workspace.appendChild(menu);

                // Add click handler to document to close menu
                const closeMenu = (event) => {
                    if (!menu.contains(event.target)) {
                        removeContextMenu();
                        document.removeEventListener('click', closeMenu);
                    }
                };

                // Delay adding the click listener to prevent immediate closure
                setTimeout(() => {
                    document.addEventListener('click', closeMenu);
                }, 0);

                return false;
            };

            function deleteState(state) {
                saveCurrentState();
                // Remove all inbound transitions to this state from other states
                $scope.states.forEach(s => {
                    s.transitions = s.transitions.filter(t => t.toState !== state);
                });

                // Remove all transitions that have this state as source or target
                $scope.transitions = $scope.transitions.filter(t =>
                    t.fromState !== state && t.toState !== state
                );

                // Remove the state itself
                const stateIndex = $scope.states.indexOf(state);
                if (stateIndex > -1) {
                    $scope.states.splice(stateIndex, 1);
                }

                // Update states attributes and recalculate levels
                updateStatesAttributes();
                calculateStateLevels();

                // Update blade.currentEntity
                updateCurrentEntity();

                // Redraw the state machine
                $scope.$apply(() => {
                    updateTransitionPaths();
                });
            }

            // Update showStateModal to handle editing existing states
            function showStateModal(x, y, startState = null, existingState = null, callback = null) {
                const modal = document.createElement('div');
                modal.className = 'modal';

                modal.innerHTML = `
                <div class="modal-content">
                    <div class="modal-title">${existingState ? 'Edit State' : 'Add New State'}</div>
                    <div class="form-group">
                        <label for="stateName">Name:</label>
                        <input type="text" id="stateName" value="${existingState ? existingState.name : ''}">
                    </div>
                    <div class="form-group">
                        <label for="stateDescription">Description:</label>
                        <textarea id="stateDescription">${existingState ? existingState.description : ''}</textarea>
                    </div>
                    <div class="modal-buttons">
                        <button class="modal-button modal-button-primary" id="saveButton">OK</button>
                        <button class="modal-button modal-button-secondary" id="cancelButton">Cancel</button>
                    </div>
                </div>
            `;

                // Append to the blade element using $element service
                $element.append(modal);

                const saveButton = modal.querySelector('#saveButton');
                const cancelButton = modal.querySelector('#cancelButton');
                const nameInput = modal.querySelector('#stateName');
                const descriptionInput = modal.querySelector('#stateDescription');

                saveButton.onclick = () => {
                    const stateName = nameInput.value.trim();
                    const description = descriptionInput.value.trim();

                    if (!stateName) {
                        alert('State name cannot be empty');
                        return;
                    }

                    if (existingState) {
                        // Save current state before updating
                        saveCurrentState();
                        // Update existing state
                        $scope.$apply(() => {
                            // If name changed, update ID and connected transitions
                            if (existingState.name !== stateName) {
                                const oldId = existingState.id;
                                existingState.id = stateName;
                                existingState.name = stateName;

                                // Update all transitions that reference this state
                                $scope.states.forEach(state => {
                                    state.transitions.forEach(transition => {
                                        if (transition.toState.id === oldId) {
                                            transition.toState.id = stateName;
                                            transition.toState.name = stateName;
                                        }
                                    });
                                });
                            } else {
                                existingState.name = stateName;
                            }
                            existingState.description = description;
                            updateCurrentEntity();
                        });
                        modal.remove();
                    } else {
                        // Save current state before adding new state
                        saveCurrentState();
                        // Create new state
                        const newState = {
                            id: stateName,
                            name: stateName,
                            description: description,
                            transitions: [],
                            isInitial: $scope.states.length === 0,
                            isFinal: true,
                            isSuccess: false,
                            isFailed: false,
                            togglePosition: 'center',
                            position: { x: x, y: y }
                        };

                        $scope.$apply(() => {
                            $scope.states.push(newState);
                            updateStatesAttributes();
                            updateCurrentEntity();

                            // If this is being created from a transition, show the transition modal
                            if (startState) {
                                modal.remove();
                                showTransitionModal(null, (transitionData) => {
                                    const newTransition = {
                                        id: generateUniqueId(),
                                        trigger: transitionData.trigger,
                                        icon: transitionData.icon || '',
                                        description: transitionData.description || '',
                                        toState: newState,
                                        fromState: startState
                                    };

                                    // Add to transitions array
                                    $scope.transitions.push(newTransition);
                                    // Update state's transitions
                                    startState.transitions.push(newTransition);
                                    startState.isFinal = false;
                                    // Calculate path
                                    updateTransitionPath(newTransition);
                                    // Recalculate levels
                                    calculateStateLevels();
                                    // Update blade.currentEntity
                                    updateCurrentEntity();
                                    // Remove temporary line
                                    removeTempLine();
                                });
                            } else if (callback) {
                                modal.remove();
                                callback(newState);
                            } else {
                                modal.remove();
                            }
                        });
                    }
                };

                cancelButton.onclick = () => {
                    modal.remove();
                    if (startState) {
                        removeTempLine();
                    }
                };

                // Focus on name input
                nameInput.focus();
            }

            function saveCurrentState() {
                // Create a deep copy of the current state before any modifications
                const currentState = $scope.states.map(state => ({
                    id: state.id,
                    name: state.name,
                    description: state.description || '',
                    isInitial: state.isInitial,
                    isFinal: state.isFinal,
                    isSuccess: state.isSuccess,
                    isFailed: state.isFailed,
                    position: { ...state.position },
                    transitions: state.transitions.map(t => ({
                        id: t.id,
                        trigger: t.trigger,
                        icon: t.icon || '',
                        description: t.description || '',
                        toState: t.toState.id
                    }))
                }));
                lastSavedState = currentState;
            }

            function restoreLastState() {
                if (lastSavedState) {
                    const stateToRestore = lastSavedState;
                    buildStateFromJSON(stateToRestore);
                    updateTransitionPaths();
                    updateCurrentEntity();
                }
            }

            function buildStateFromJSON(statesData) {
                // Clear existing states and transitions
                $scope.states = [];
                $scope.transitions = [];

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
                    $scope.states.push(state);
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
                                id: transitionData.id || generateUniqueId(),
                                trigger: transitionData.trigger,
                                icon: transitionData.icon || '',
                                description: transitionData.description || '',
                                fromState: fromState,
                                toState: toState,
                                path: '', // Will be calculated by updateTransitionPath
                                labelPosition: {} // Will be calculated by updateTransitionPath
                            };

                            $scope.transitions.push(transition);
                            fromState.transitions.push(transition);
                        });
                    }
                });

                // Update states attributes
                updateStatesAttributes();
                calculateStateLevels();
            }

            function calculateStateLevels() {
                $scope.states.forEach(state => state.level = 0);

                $scope.states.filter(s => s.isInitial).forEach(s => s.level = 1);

                let changed = true;
                while (changed) {
                    changed = false;
                    $scope.states.forEach(state => {
                        const incomingStates = $scope.states.filter(s =>
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
            }

            // Start dragging a state
            $scope.startDrag = function(event, state) {
                var workspace = document.getElementById('visualEditorWorkspace');

                saveCurrentState();
                let startX = event.clientX;
                let startY = event.clientY;

                isDragging = true;

                function updateStateAndTransitions(dx, dy) {
                    state.position.x += dx;
                    state.position.y += dy;

                    // Find all transitions connected to this state
                    const affectedTransitions = $scope.transitions.filter(transition =>
                        transition.fromState === state || transition.toState === state
                    );

                    // Update all affected transitions
                    affectedTransitions.forEach(transition => {
                        updateTransitionPath(transition);
                    });

                    // Update workspace size to accommodate new state position
                    updateWorkspaceSize();
                }

                workspace.onmousemove = (e) => {
                    if (isDragging) {
                        let dx = e.clientX - startX;
                        let dy = e.clientY - startY;

                        $scope.$apply(() => {
                            updateStateAndTransitions(dx, dy);
                        });

                        startX = e.clientX;
                        startY = e.clientY;
                    }
                };

                workspace.onmouseup = () => {
                    isDragging = false;
                    workspace.onmousemove = null;
                    workspace.onmouseup = null;

                    // Final update of all transitions and currentEntity
                    $scope.$apply(() => {
                        updateTransitionPaths();
                        updateCurrentEntity();
                    });
                };
            };

            // Start creating a transition
            $scope.startTransition = function(event, state) {
                isTransitioning = true;
                transitionStartState = state;
                var workspace = document.getElementById('visualEditorWorkspace');
                var svg = workspace.querySelector('svg');

                tempLine = document.createElementNS("http://www.w3.org/2000/svg", "path");
                tempLine.setAttribute("class", "transition");
                svg.appendChild(tempLine);

                workspace.onmousemove = (e) => {
                    if (isTransitioning && tempLine) {
                        const rect = workspace.getBoundingClientRect();
                        const fromX = transitionStartState.position.x + stateWidth/2;
                        const fromY = transitionStartState.position.y + stateHeight;
                        const toX = e.clientX - rect.left + workspace.scrollLeft;
                        const toY = e.clientY - rect.top + workspace.scrollTop;

                        // Calculate control points for preview curve
                        const deltaX = toX - fromX;
                        const deltaY = toY - fromY;
                        const distance = Math.sqrt(deltaX * deltaX + deltaY * deltaY);
                        const curveIntensity = Math.min(Math.max(distance * 0.2, 30), 100);

                        const cp1y = fromY + curveIntensity;
                        const cp2y = toY - curveIntensity;

                        tempLine.setAttribute("d", `M ${fromX} ${fromY} C ${fromX} ${cp1y}, ${toX} ${cp2y}, ${toX} ${toY}`);
                    }
                };

                workspace.onmouseup = (event) => {
                    if (!isTransitioning) return;

                    const rect = workspace.getBoundingClientRect();
                    const mouseX = event.pageX - rect.left + workspace.scrollLeft;
                    const mouseY = event.pageY - rect.top + workspace.scrollTop;

                    const targetState = $scope.states.find(s =>
                        mouseX >= s.position.x &&
                        mouseX <= s.position.x + stateWidth &&
                        mouseY >= s.position.y &&
                        mouseY <= s.position.y + stateHeight
                    );

                    if (targetState) {
                        finishTransition(targetState);
                    } else {
                        showNewStateContextMenu(event, transitionStartState, mouseX, mouseY);
                    }

                    isTransitioning = false;
                    workspace.onmousemove = null;
                    workspace.onmouseup = null;
                };
            };

            function finishTransition(targetState) {
                if (!isTransitioning || transitionStartState.id === targetState.id) {
                    removeTempLine();
                    return;
                }

                saveCurrentState();
                showTransitionModal(null, (transitionData) => {
                    const newTransition = {
                        id: generateUniqueId(),
                        trigger: transitionData.trigger,
                        icon: transitionData.icon || '',
                        description: transitionData.description || '',
                        toState: targetState,
                        fromState: transitionStartState
                    };

                    $scope.$apply(() => {
                        // Add to transitions array
                        $scope.transitions.push(newTransition);
                        // Update state's transitions
                        transitionStartState.transitions.push(newTransition);
                        transitionStartState.isFinal = false;
                        // Calculate path
                        updateTransitionPath(newTransition);
                        // Recalculate levels
                        calculateStateLevels();
                        // Update blade.currentEntity
                        updateCurrentEntity();
                    });

                    isTransitioning = false;
                    removeTempLine();
                });
            }

            function showNewStateContextMenu(e, startState, mouseX, mouseY) {
                e.preventDefault();
                e.stopPropagation();

                removeContextMenu();

                const menu = document.createElement('div');
                menu.className = 'context-menu';
                menu.style.left = `${e.pageX}px`;
                menu.style.top = `${e.pageY}px`;

                const addStateItem = document.createElement('div');
                addStateItem.className = 'context-menu-item';
                addStateItem.innerHTML = '<i class="fas fa-plus"></i>Add new State';
                addStateItem.onclick = () => {
                    removeContextMenu();
                    showStateModal(mouseX - stateWidth / 2, mouseY, startState);
                };

                menu.appendChild(addStateItem);
                document.body.appendChild(menu);

                // Add click handler to document to close menu and clean up
                const handleClickOutside = (event) => {
                    if (!menu.contains(event.target)) {
                        removeContextMenu();
                        removeTempLine();
                        isTransitioning = false;
                        document.removeEventListener('click', handleClickOutside);
                    }
                };

                // Delay adding the click listener to prevent immediate closure
                setTimeout(() => {
                    document.addEventListener('click', handleClickOutside);
                }, 0);
            }

            function removeContextMenu() {
                const existingMenu = document.querySelector('.context-menu');
                if (existingMenu) {
                    existingMenu.remove();
                }
                document.removeEventListener('click', removeContextMenu);
            }

            function updateTransitionPath(transition) {
                var workspace = document.getElementById('visualEditorWorkspace');
                if (!workspace) return;

                const fromState = transition.fromState;
                const toState = transition.toState;

                // Check if states have valid positions
                if (!fromState.position || !toState.position) {
                    console.warn('States do not have valid positions:', fromState, toState);
                    return;
                }

                // Calculate path points
                const startX = fromState.position.x + stateWidth/2;
                const startY = fromState.position.y + stateHeight;
                const endX = toState.position.x + stateWidth/2;
                const endY = toState.position.y;

                // Calculate control points for a more elegant curve
                const deltaX = endX - startX;
                const deltaY = endY - startY;
                const distance = Math.sqrt(deltaX * deltaX + deltaY * deltaY);

                // Adjust curve intensity based on distance
                const curveIntensity = Math.min(Math.max(distance * 0.2, 30), 100);

                // Calculate control points
                const cp1y = startY + curveIntensity;
                const cp2y = endY - curveIntensity;

                // Calculate label position - place it at the middle of the curve
                const t = 0.5; // Parameter for middle point
                const labelX = startX * Math.pow(1-t, 3) +
                              3 * startX * Math.pow(1-t, 2) * t +
                              3 * endX * (1-t) * Math.pow(t, 2) +
                              endX * Math.pow(t, 3);

                const labelY = startY * Math.pow(1-t, 3) +
                              3 * cp1y * Math.pow(1-t, 2) * t +
                              3 * cp2y * (1-t) * Math.pow(t, 2) +
                              endY * Math.pow(t, 3);

                // Update path and label position
                transition.path = `M ${startX} ${startY} C ${startX} ${cp1y}, ${endX} ${cp2y}, ${endX} ${endY}`;
                transition.labelPosition = { x: labelX, y: labelY };

                // Force immediate update of SVG elements
                $scope.$evalAsync(() => {
                    const transitionIndex = $scope.transitions.indexOf(transition);
                    const transitionGroup = workspace.querySelector(`g[ng-repeat="transition in transitions"]:nth-child(${transitionIndex + 1})`);

                    if (transitionGroup) {
                        // Update path
                        const pathEl = transitionGroup.querySelector('path');
                        if (pathEl) {
                            pathEl.setAttribute('d', transition.path);
                        }

                        // Update label background
                        const labelBgEl = transitionGroup.querySelector('rect');
                        if (labelBgEl) {
                            labelBgEl.setAttribute('x', labelX - 50);
                            labelBgEl.setAttribute('y', labelY - 10);
                            labelBgEl.setAttribute('width', '100');
                            labelBgEl.setAttribute('height', '20');
                        }

                        // Update label text
                        const labelEl = transitionGroup.querySelector('text');
                        if (labelEl) {
                            labelEl.setAttribute('x', labelX);
                            labelEl.setAttribute('y', labelY + 5);
                            labelEl.textContent = transition.trigger;
                        }
                    }
                });
            }

            // Update all transition paths
            function updateTransitionPaths() {
                var workspace = document.getElementById('visualEditorWorkspace');
                if (!workspace) return;

                $scope.transitions.forEach(function(transition) {
                    updateTransitionPath(transition);
                });
            }

            function deleteTransition(transition) {
                saveCurrentState();
                const fromState = transition.fromState;

                // Remove from transitions array
                const transitionIndex = $scope.transitions.indexOf(transition);
                if (transitionIndex > -1) {
                    $scope.$apply(() => {
                        $scope.transitions.splice(transitionIndex, 1);
                    });
                }

                // Remove from state's transitions
                const stateTransitionIndex = fromState.transitions.indexOf(transition);
                if (stateTransitionIndex > -1) {
                    $scope.$apply(() => {
                        fromState.transitions.splice(stateTransitionIndex, 1);
                    });
                }

                // Update states attributes and recalculate levels
                $scope.$apply(() => {
                    updateStatesAttributes();
                    calculateStateLevels();
                    updateCurrentEntity();
                });
            }

            function showTransitionModal(existingTransition, callback) {
                // Get the current blade element using $element service
                const modal = document.createElement('div');
                modal.className = 'modal';

                modal.innerHTML = `
            <div class="modal-content">
                <div class="modal-title">${existingTransition ? 'Edit Transition' : 'New Transition'}</div>
                <div class="form-group">
                    <label for="transitionTrigger">Trigger:</label>
                    <input type="text" id="transitionTrigger" value="${existingTransition ? existingTransition.trigger : ''}">
                </div>
                <div class="form-group">
                    <label for="transitionIcon">Icon:</label>
                    <input type="text" id="transitionIcon" value="${existingTransition ? existingTransition.icon || '' : ''}">
                </div>
                <div class="form-group">
                    <label for="transitionDescription">Description:</label>
                    <textarea id="transitionDescription">${existingTransition ? existingTransition.description || '' : ''}</textarea>
                </div>
                <div class="modal-buttons">
                    <button class="modal-button modal-button-primary" id="saveButton">OK</button>
                    <button class="modal-button modal-button-secondary" id="cancelButton">Cancel</button>
                </div>
            </div>
        `;

                // Append to the blade element using $element service
                $element.append(modal);

                const saveButton = modal.querySelector('#saveButton');
                const cancelButton = modal.querySelector('#cancelButton');
                const triggerInput = modal.querySelector('#transitionTrigger');
                const iconInput = modal.querySelector('#transitionIcon');
                const descriptionInput = modal.querySelector('#transitionDescription');

                saveButton.onclick = () => {
                    const trigger = triggerInput.value.trim();
                    if (!trigger) {
                        alert('Trigger cannot be empty');
                        return;
                    }

                    callback({
                        trigger: trigger,
                        icon: iconInput.value.trim(),
                        description: descriptionInput.value.trim()
                    });
                    updateCurrentEntity();
                    modal.remove();
                };

                cancelButton.onclick = () => {
                    modal.remove();
                    removeTempLine();
                    isTransitioning = false;
                };

                // Focus on trigger input
                triggerInput.focus();
            }

            function removeTempLine() {
                var workspace = document.getElementById('visualEditorWorkspace');
                var svg = workspace.querySelector('svg');
                if (tempLine) {
                    svg.removeChild(tempLine);
                    tempLine = null;
                }
            }

            function normalizeStateLayout() {
                var workspace = document.getElementById('visualEditorWorkspace');
                if (!workspace) return;

                // Update workspace size before normalization
                updateWorkspaceSize();

                calculateStateLevels();

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

                function checkTransitionIntersections() {
                    let intersections = 0;
                    const transitions = [];

                    $scope.states.forEach(state => {
                        state.transitions.forEach(transition => {
                            transitions.push({
                                from: state,
                                to: transition.toState,
                                fromX: state.position.x + stateWidth/2,
                                fromY: state.position.y + stateHeight,
                                toX: transition.toState.position.x + stateWidth/2,
                                toY: transition.toState.position.y
                            });
                        });
                    });

                    for (let i = 0; i < $scope.transitions.length; i++) {
                        for (let j = i + 1; j < $scope.transitions.length; j++) {
                            if (doLinesIntersect(
                                $scope.transitions[i].fromX, $scope.transitions[i].fromY,
                                $scope.transitions[i].toX, $scope.transitions[i].toY,
                                $scope.transitions[j].fromX, $scope.transitions[j].fromY,
                                $scope.transitions[j].toX, $scope.transitions[j].toY
                            )) {
                                intersections++;
                            }
                        }

                        $scope.states.forEach(state => {
                            if (state !== $scope.transitions[i].from && state !== $scope.transitions[i].to) {
                                if (transitionCrossesState(
                                    $scope.transitions[i].fromX, $scope.transitions[i].fromY,
                                    $scope.transitions[i].toX, $scope.transitions[i].toY,
                                    state
                                )) {
                                    intersections++;
                                }
                            }
                        });
                    }

                    return intersections;
                }

                const statesByLevel = {};
                $scope.states.forEach(state => {
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
                                const parents = $scope.states.filter(s =>
                                    s.transitions.some(t => t.toState === state)
                                );
                                if (parents.length === 0) return 0;
                                return parents.reduce((sum, p) => sum + p.position.x, 0) / parents.length;
                            };

                            return getParentX(stateA) - getParentX(stateB);
                        });

                        const statesByParent = {};
                        statesInLevel.forEach(state => {
                            const parents = $scope.states.filter(s =>
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
                                const parents = $scope.states.filter(s =>
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

                    const currentIntersections = checkTransitionIntersections();

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
                        const overlap = minSpace - (statesInLevel[i].position.x - statesInLevel[i-1].position.x);
                        if (overlap > 0) {
                            statesInLevel[i].position.x += overlap;
                        }
                    }
                });

                let minX = Infinity, maxX = -Infinity;
                $scope.states.forEach(state => {
                    minX = Math.min(minX, state.position.x);
                    maxX = Math.max(maxX, state.position.x + stateWidth);
                });

                const graphWidth = maxX - minX;
                const workspaceWidth = workspace.clientWidth;

                if (graphWidth < workspaceWidth) {
                    const offset = (workspaceWidth - graphWidth) / 2 - minX;
                    $scope.states.forEach(state => {
                        state.position.x += offset;
                    });
                } else {
                    const rightMargin = 50;
                    workspace.style.width = (graphWidth + rightMargin) + 'px';
                }

                //drawStates();

                let maxY = -Infinity;
                $scope.states.forEach(state => {
                    maxY = Math.max(maxY, state.position.y + stateHeight);
                });
                //workspace.style.height = (maxY + 100) + 'px';

                workspace.style.width = '100%';
            }

            function updateCurrentEntity() {
                // Create a clean representation of states for serialization
                const serializedStates = $scope.states.map(state => ({
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

                // Update blade.currentEntity with stringified state machine
                blade.currentEntity = JSON.stringify(serializedStates);
                blade.parentBlade.updateStateMachineData(blade.currentEntity);
            }

            function updateWorkspaceSize() {
                const workspace = document.getElementById('visualEditorWorkspace');
                const bladeContent = document.querySelector('.blade-content');
                if (!workspace || !bladeContent) return;

                // Get blade content dimensions
                const bladeRect = bladeContent.getBoundingClientRect();

                // Calculate required size based on state positions
                let minX = Infinity;
                let maxX = -Infinity;
                let minY = Infinity;
                let maxY = -Infinity;

                $scope.states.forEach(state => {
                    minX = Math.min(minX, state.position.x);
                    maxX = Math.max(maxX, state.position.x + stateWidth);
                    minY = Math.min(minY, state.position.y);
                    maxY = Math.max(maxY, state.position.y + stateHeight);
                });

                // Add padding
                const padding = 10;
                minX -= padding;
                maxX += padding;
                minY -= padding;
                maxY += padding;

                // Set workspace dimensions
                workspace.style.width = '100%';
                workspace.style.position = 'absolute';
                workspace.style.top = '0';
                workspace.style.left = '0';
                workspace.style.right = '0';
                workspace.style.bottom = '0';
                workspace.style.overflow = 'auto';

                // Update SVG dimensions and position
                const svg = workspace.querySelector('svg');
                if (svg) {
                    // If we have negative coordinates, shift all states to positive space
                    if (minX < 0) {
                        const shift = Math.abs(minX);
                        $scope.states.forEach(state => {
                            state.position.x += shift;
                        });
                        maxX += shift;
                        minX = 0;
                    }

                    if (minY < 0) {
                        const shift = Math.abs(minY);
                        $scope.states.forEach(state => {
                            state.position.y += shift;
                        });
                        maxY += shift;
                        minY = 0;
                    }

                    const totalWidth = maxX - minX;
                    const totalHeight = maxY - minY;

                    svg.style.minWidth = Math.max(totalWidth, bladeRect.width) + 'px';
                    svg.style.minHeight = Math.max(totalHeight, bladeRect.height - 20) + 'px';

                    // Update all transition paths after shifting states
                    updateTransitionPaths();
                }
            }

            // Add workspace context menu handler
            function initializeWorkspaceContextMenu() {
                const workspace = document.getElementById('visualEditorWorkspace');
                if (!workspace) return;

                workspace.oncontextmenu = (e) => {
                    // Only show context menu if clicking on the workspace or SVG background
                    const target = e.target;
                    if (target === workspace || target.tagName === 'svg' || target === workspace.querySelector('svg')) {
                        e.preventDefault();
                        e.stopPropagation();
                        showWorkspaceContextMenu(e);
                        return false;
                    }
                };
            }

            function showWorkspaceContextMenu(e) {
                removeContextMenu();

                const menu = document.createElement('div');
                menu.className = 'context-menu';
                menu.style.position = 'absolute';
                menu.style.left = `${e.pageX}px`;
                menu.style.top = `${e.pageY}px`;
                menu.style.zIndex = '1000';

                const undoItem = document.createElement('div');
                undoItem.className = 'context-menu-item';
                undoItem.innerHTML = '<i class="fas fa-undo"></i>Undo';

                if (!lastSavedState) {
                    undoItem.classList.add('disabled');
                    undoItem.style.opacity = '0.5';
                    undoItem.style.cursor = 'default';
                } else {
                    undoItem.style.cursor = 'pointer';
                    undoItem.onclick = () => {
                        removeContextMenu();
                        $scope.$apply(() => {
                            restoreLastState();
                        });
                    };
                }

                menu.appendChild(undoItem);
                document.body.appendChild(menu);

                // Add click handler to document to close menu
                const handleClickOutside = (event) => {
                    if (!menu.contains(event.target)) {
                        removeContextMenu();
                        document.removeEventListener('click', handleClickOutside);
                    }
                };

                // Delay adding the click listener to prevent immediate closure
                setTimeout(() => {
                    document.addEventListener('click', handleClickOutside);
                }, 0);
            }

            // Add a function to generate unique IDs
            function generateUniqueId() {
                return 'transition_' + Math.random().toString(36).substr(2, 9);
            }

            // Initialize
            blade.refresh();
        }
    ]);
