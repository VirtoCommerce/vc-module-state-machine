angular.module('virtoCommerce.stateMachineModule')
    .component('stateNode', {
        bindings: {
            state: '<',
            machineData: '<',
            parentScope: '<',
        },
        templateUrl: 'Modules/$(VirtoCommerce.StateMachine)/Scripts/components/state-node.tpl.html',
        controller: ['$scope', '$element', '$filter', 'platformWebApp.authService',
            'virtoCommerce.stateMachineModule.stateMachineModalService',
            'virtoCommerce.stateMachineModule.stateMachineStateService',
            'virtoCommerce.stateMachineModule.stateMachineTransitionService',
            'virtoCommerce.stateMachineModule.stateMachineWorkspaceService',
            function ($scope, $element, $filter, authService,
                stateMachineModalService,
                stateMachineStateService,
                stateMachineTransitionService,
                stateMachineWorkspaceService
            ) {
                var $ctrl = this;

                const stateWidth = 150;
                const stateHeight = 100;
                var isTransitioning = false;
                var isDragging = false;
                var transitionStartState = null;

                $ctrl.canEdit = function() {
                    return authService.checkPermission('statemachine:update');
                };

                $ctrl.onStateHover = function (state, isHovered) {
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
                    const connectedTransitions = $ctrl.machineData.transitions.filter(x => x.fromState.id === state.id || x.toState.id === state.id);
                    connectedTransitions.forEach(transition => {
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

                $ctrl.onStateToggleChange = function (state, newPosition) {
                    if (!$ctrl.canEdit()) {
                        return;
                    }

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

                    stateMachineStateService.updateStateAttributes(state, $ctrl.machineData.states);
                };

                $ctrl.showStateContextMenu = function (e, state) {
                    if (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        e.returnValue = false;
                    }

                    const workspace = document.getElementById('visualEditorWorkspace');
                    if (!workspace) return;

                    const rect = workspace.getBoundingClientRect();
                    const x = e.clientX - rect.left;
                    const y = e.clientY - rect.top;

                    const contextMenuItems = [
                        {
                            label: $filter('translate')('statemachine.components.state-node.context-menu.edit-state'),
                            icon: 'fas fa-edit',
                            permission: 'statemachine:update',
                            action: () => {
                                $ctrl.parentScope.contextMenuData = null;
                                stateMachineModalService.editStateModal($ctrl.parentScope, $element, state.position.x, state.position.y, null, state, $ctrl.machineData.states, $ctrl.machineData.transitions);
                            }
                        },
                        {
                            label: $filter('translate')('statemachine.components.state-node.context-menu.edit-localization'),
                            icon: 'fas fa-globe',
                            permission: 'statemachine:localize',
                            action: async () => {
                                $ctrl.parentScope.contextMenuData = null;
                                var languages = $ctrl.parentScope.blade.allLanguages;
                                var existedTranslations = [];
                                if ($ctrl.parentScope.blade.getCurrentTranslations) {
                                    existedTranslations = await $ctrl.parentScope.blade.getCurrentTranslations(state);
                                }
                                stateMachineModalService.editLocalization($ctrl.parentScope, $element, state, languages, existedTranslations, $ctrl.parentScope.blade.saveCurrentTranslations);
                            }
                        },
                        {
                            label: $filter('translate')('statemachine.components.state-node.context-menu.delete-state'),
                            icon: 'fas fa-trash',
                            permission: 'statemachine:update',
                            action: () => {
                                $ctrl.parentScope.contextMenuData = null;
                                $ctrl.deleteState(state);
                            }
                        }
                    ];

                    $ctrl.parentScope.contextMenuData = { items: contextMenuItems, x: x, y: y };

                    const handleClickOutside = (event) => {
                        $ctrl.parentScope.$apply(() => {
                            $ctrl.parentScope.contextMenuData = null;
                        });
                        stateMachineTransitionService.removeTempLine($ctrl.parentScope);
                        isTransitioning = false;
                        document.removeEventListener('click', handleClickOutside);
                    };

                    setTimeout(() => {
                        document.addEventListener('click', handleClickOutside);
                    }, 100);

                    return false;
                };

                $ctrl.startDrag = function (event, state) {
                    if (!$ctrl.canEdit()) {
                        return;
                    }

                    var workspace = document.getElementById('visualEditorWorkspace');

                    let startX = event.clientX;
                    let startY = event.clientY;

                    isDragging = true;

                    workspace.onmousemove = (e) => {
                        if (isDragging) {
                            let dx = e.clientX - startX;
                            let dy = e.clientY - startY;

                            $ctrl.parentScope.$apply(() => {
                                state.position.x += dx;
                                state.position.y += dy;

                                const affectedTransitions = $ctrl.machineData.transitions.filter(transition =>
                                    transition.fromState === state || transition.toState === state
                                );

                                affectedTransitions.forEach(transition => {
                                    stateMachineTransitionService.updateTransitionPath(transition, $ctrl.machineData.transitions, $ctrl.parentScope, workspace);
                                });

                                stateMachineWorkspaceService.updateWorkspaceSize($ctrl.machineData.states, $ctrl.machineData.transitions, $ctrl.parentScope, workspace);
                            });

                            startX = e.clientX;
                            startY = e.clientY;
                        }
                    };

                    workspace.onmouseup = () => {
                        isDragging = false;
                        workspace.onmousemove = null;
                        workspace.onmouseup = null;

                        $ctrl.parentScope.$apply(() => {
                            stateMachineTransitionService.updateTransitionPaths($ctrl.machineData.transitions, $ctrl.parentScope, workspace);
                        });
                    };
                };

                $ctrl.startTransition = function (event, state) {
                    if (!$ctrl.canEdit()) {
                        return;
                    }

                    isTransitioning = true;
                    transitionStartState = state;
                    var workspace = document.getElementById('visualEditorWorkspace');
                    var svg = workspace.querySelector('svg');

                    $ctrl.parentScope.tempLine = document.createElementNS("http://www.w3.org/2000/svg", "path");
                    $ctrl.parentScope.tempLine.setAttribute("class", "transition");
                    svg.appendChild($ctrl.parentScope.tempLine);

                    workspace.onmousemove = (e) => {
                        if (isTransitioning && $ctrl.parentScope.tempLine) {
                            const rect = workspace.getBoundingClientRect();
                            const fromX = transitionStartState.position.x + stateWidth / 2;
                            const fromY = transitionStartState.position.y + stateHeight;
                            const toX = e.clientX - rect.left + workspace.scrollLeft;
                            const toY = e.clientY - rect.top + workspace.scrollTop;

                            const deltaX = toX - fromX;
                            const deltaY = toY - fromY;
                            const distance = Math.sqrt(deltaX * deltaX + deltaY * deltaY);
                            const curveIntensity = Math.min(Math.max(distance * 0.2, 30), 100);

                            const cp1y = fromY + curveIntensity;
                            const cp2y = toY - curveIntensity;

                            $ctrl.parentScope.tempLine.setAttribute("d", `M ${fromX} ${fromY} C ${fromX} ${cp1y}, ${toX} ${cp2y}, ${toX} ${toY}`);
                        }
                    };

                    workspace.onmouseup = (event) => {
                        if (!isTransitioning) return;

                        const rect = workspace.getBoundingClientRect();
                        const mouseX = event.pageX - rect.left + workspace.scrollLeft;
                        const mouseY = event.pageY - rect.top + workspace.scrollTop;

                        const targetState = $ctrl.machineData.states.find(s =>
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
                        stateMachineTransitionService.removeTempLine($ctrl.parentScope);
                        return;
                    }

                    var workspace = document.getElementById('visualEditorWorkspace');
                    if (!workspace) return;

                    stateMachineModalService.editTransitionModal($ctrl.parentScope, $element, null, (transitionData) => {
                        const newTransition = {
                            id: stateMachineTransitionService.generateUniqueId(),
                            trigger: transitionData.trigger,
                            icon: transitionData.icon || '',
                            description: transitionData.description || '',
                            toState: targetState,
                            fromState: transitionStartState
                        };

                        $ctrl.machineData.transitions.push(newTransition);
                        transitionStartState.transitions.push(newTransition);
                        transitionStartState.isFinal = false;
                        stateMachineTransitionService.updateTransitionPath(newTransition, $ctrl.machineData.transitions, $ctrl.parentScope, workspace);
                        stateMachineStateService.calculateStateLevels($ctrl.machineData.states);
                        stateMachineStateService.updateStatesAttributes($ctrl.machineData.states);

                        isTransitioning = false;
                        stateMachineTransitionService.removeTempLine($ctrl.parentScope);
                    });
                }

                function showNewStateContextMenu(e, startState, mouseX, mouseY) {
                    e.preventDefault();
                    e.stopPropagation();

                    const workspace = document.getElementById('visualEditorWorkspace');
                    if (!workspace) return;
                    const rect = workspace.getBoundingClientRect();
                    const x = e.clientX - rect.left;
                    const y = e.clientY - rect.top;

                    const contextMenuItems = [
                        {
                            label: $filter('translate')('statemachine.components.state-node.context-menu.add-new-state'),
                            icon: 'fas fa-plus',
                            permission: 'statemachine:update',
                            action: () => {
                                $ctrl.parentScope.contextMenuData = null;
                                stateMachineModalService.editStateModal($ctrl.parentScope, $element, mouseX - stateWidth / 2, mouseY, startState, null, $ctrl.machineData.states, $ctrl.machineData.transitions, (newState) => {
                                    stateMachineModalService.editTransitionModal($ctrl.parentScope, $element, null, (transitionData) => {
                                        const newTransition = {
                                            id: stateMachineTransitionService.generateUniqueId(),
                                            trigger: transitionData.trigger,
                                            icon: transitionData.icon || '',
                                            description: transitionData.description || '',
                                            toState: newState,
                                            fromState: startState
                                        };
                                        $ctrl.machineData.transitions.push(newTransition);
                                        startState.transitions.push(newTransition);
                                        startState.isFinal = false;
                                        stateMachineTransitionService.updateTransitionPaths($ctrl.machineData.transitions, $ctrl.parentScope, workspace);
                                        stateMachineStateService.calculateStateLevels($ctrl.machineData.states);
                                        stateMachineStateService.updateStatesAttributes($ctrl.machineData.states);
                                        stateMachineTransitionService.removeTempLine($ctrl.parentScope);
                                    });
                                });
                            }
                        }
                    ];

                    $ctrl.parentScope.$apply(() => {
                        $ctrl.parentScope.contextMenuData = { items: contextMenuItems, x: x, y: y };
                    });

                    const handleClickOutside = (event) => {
                        $ctrl.parentScope.$apply(() => {
                            $ctrl.parentScope.contextMenuData = null;
                        });
                        stateMachineTransitionService.removeTempLine($ctrl.parentScope);
                        isTransitioning = false;
                        document.removeEventListener('click', handleClickOutside);
                    };

                    setTimeout(() => {
                        document.addEventListener('click', handleClickOutside);
                    }, 100);
                }

                $ctrl.editState = function (e, state) {
                    if (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    }

                    stateMachineModalService.editStateModal($ctrl.parentScope, $element, state.position.x, state.position.y, null, state, $ctrl.machineData.states, $ctrl.machineData.transitions);
                };

                $ctrl.deleteState = function (state) {
                    var workspace = document.getElementById('visualEditorWorkspace');
                    if (!workspace) return;

                    $ctrl.machineData.states.forEach(s => {
                        s.transitions = s.transitions.filter(t => t.toState !== state);
                    });

                    $ctrl.machineData.transitions = $ctrl.machineData.transitions.filter(t =>
                        t.fromState !== state && t.toState !== state
                    );

                    const stateIndex = $ctrl.machineData.states.indexOf(state);
                    if (stateIndex > -1) {
                        $ctrl.machineData.states.splice(stateIndex, 1);
                    }

                    stateMachineStateService.updateStatesAttributes($ctrl.machineData.states);
                    stateMachineStateService.calculateStateLevels($ctrl.machineData.states);

                    stateMachineTransitionService.updateTransitionPaths($ctrl.machineData.transitions, $ctrl.parentScope, workspace);
                }


            }]
    });
