angular.module('virtoCommerce.stateMachineModule')
    .component('stateTransitions', {
        bindings: {
            transitions: '<',
            machineData: '<',
            parentScope: '<',
        },
        templateUrl: 'Modules/$(VirtoCommerce.StateMachine)/Scripts/components/state-transitions.tpl.html',
        controller: ['$scope', '$element',
            'virtoCommerce.stateMachineModule.stateMachineModalService',
            'virtoCommerce.stateMachineModule.stateMachineStateService',
            'virtoCommerce.stateMachineModule.stateMachineTransitionService',
            function ($scope, $element,
                stateMachineModalService,
                stateMachineStateService,
                stateMachineTransitionService
            ) {
                var $ctrl = this;

                $ctrl.onTransitionHover = function (transition, isHovered) {
                    const workspace = document.getElementById('visualEditorWorkspace');
                    if (!workspace) return;

                    const transitionGroup = workspace.querySelector(`g[ng-repeat="transition in transitions"]:nth-child(${$ctrl.machineData.transitions.indexOf(transition) + 1})`);
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

                $ctrl.showTransitionContextMenu = function (e, transition) {
                    if (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        e.returnValue = false;
                    }

                    const workspace = document.getElementById('visualEditorWorkspace');
                    if (!workspace) return;

                    const rect = workspace.getBoundingClientRect();
                    const x = e.clientX - rect.left + workspace.scrollLeft;
                    const y = e.clientY - rect.top + workspace.scrollTop;

                    const contextMenuItems = [
                        {
                            label: 'Edit transition',
                            icon: 'fas fa-edit',
                            action: () => {
                                $ctrl.parentScope.contextMenuData = null;
                                stateMachineModalService.editTransitionModal($ctrl.parentScope, $element, transition, (transitionData) => {
                                    transition.trigger = transitionData.trigger;
                                    transition.icon = transitionData.icon;
                                    transition.description = transitionData.description;

                                    stateMachineStateService.updateStatesAttributes($ctrl.machineData.states);
                                });
                            }
                        },
                        {
                            label: 'Delete transition',
                            icon: 'fas fa-trash',
                            action: () => {
                                $ctrl.parentScope.contextMenuData = null;
                                stateMachineTransitionService.deleteTransition($ctrl.machineData.states, transition, $ctrl.machineData.transitions);
                            }
                        },
                        {
                            label: 'Insert state here',
                            icon: 'fas fa-plus',
                            action: () => {
                                $ctrl.parentScope.contextMenuData = null;
                                const sourceState = transition.fromState;
                                const targetState = transition.toState;
                                const newX = (sourceState.position.x + targetState.position.x) / 2;
                                const newY = (sourceState.position.y + targetState.position.y) / 2;

                                stateMachineModalService.editStateModal($ctrl.parentScope, $element, newX, newY, null, null, $ctrl.machineData.states, $ctrl.machineData.transitions, (newState) => {
                                    stateMachineModalService.editTransitionModal($ctrl.parentScope, $element, null, (transitionData) => {
                                        const newTransition = {
                                            id: stateMachineTransitionService.generateUniqueId(),
                                            trigger: transitionData.trigger,
                                            icon: transitionData.icon || '',
                                            description: transitionData.description || '',
                                            fromState: newState,
                                            toState: targetState
                                        };

                                        transition.toState = newState;

                                        $ctrl.machineData.transitions.push(newTransition);
                                        newState.transitions.push(newTransition);

                                        stateMachineStateService.updateStatesAttributes($ctrl.machineData.states);
                                        stateMachineStateService.calculateStateLevels($ctrl.machineData.states);

                                        stateMachineTransitionService.updateTransitionPath(transition, $ctrl.machineData.transitions, $ctrl.parentScope, workspace);
                                        stateMachineTransitionService.updateTransitionPath(newTransition, $ctrl.machineData.transitions, $ctrl.parentScope, workspace);
                                    });
                                });
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

                $ctrl.editTransition = function (e, transition) {
                    if (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    }

                    stateMachineModalService.editTransitionModal($ctrl.parentScope, $element, transition, (transitionData) => {
                        transition.trigger = transitionData.trigger;
                        transition.icon = transitionData.icon;
                        transition.description = transitionData.description;
                        stateMachineStateService.updateStatesAttributes($ctrl.machineData.states);
                    });
                }

            }]
    });
