angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineModalService', [
        '$timeout', '$filter',
        'virtoCommerce.stateMachineModule.stateMachineStateService',
        'virtoCommerce.stateMachineModule.stateMachineTransitionService',
        function ($timeout, $filter,
            stateMachineStateService,
            stateMachineTransitionService
        ) {
            function editStateModal($scope, $element, x, y, startState = null, existingState = null, states, transitions, callback = null) {
                var modalComponent = {
                    title: existingState ?
                        $filter('translate')('statemachine.modals.state.edit-title') :
                        $filter('translate')('statemachine.modals.state.add-title'),
                    entity: existingState || {
                        id: '',
                        name: '',
                        description: ''
                    },
                    fields: [
                        {
                            name: 'name',
                            title: $filter('translate')('statemachine.modals.common.name'),
                            valueType: 'ShortText',
                            isRequired: true
                        },
                        {
                            name: 'description',
                            title: $filter('translate')('statemachine.modals.common.description'),
                            valueType: 'LongText'
                        }
                    ],
                    okButtonCaption: $filter('translate')('statemachine.modals.common.ok'),
                    okAction: function () {
                        $scope.modalData = null;
                        if (existingState) {
                            const oldId = existingState.id;
                            existingState.id = modalComponent.entity.name;
                            existingState.name = modalComponent.entity.name;
                            states.forEach(state => {
                                state.transitions.forEach(transition => {
                                    if (transition.toState.id === oldId) {
                                        transition.toState.id = modalComponent.entity.name;
                                        transition.toState.name = modalComponent.entity.name;
                                    }
                                });
                            });
                        } else {
                            const newState = {
                                id: modalComponent.entity.name,
                                name: modalComponent.entity.name,
                                description: modalComponent.entity.description,
                                transitions: [],
                                isInitial: states.length === 0,
                                isFinal: true,
                                isSuccess: false,
                                isFailed: false,
                                togglePosition: 'center',
                                position: { x: x, y: y }
                            };

                            states.push(newState);
                            stateMachineStateService.updateStatesAttributes(states);

                            if (callback) {
                                callback(newState);
                            }
                        }
                    },
                    cancelAction: function () {
                        if (startState) {
                            stateMachineTransitionService.removeTempLine($scope);
                        }

                        $scope.modalData = null;
                    }
                };

                $scope.modalData = modalComponent;

                $timeout(() => {
                    const nameInput = $element[0].querySelector('input[name="name"]');
                    if (nameInput) {
                        nameInput.focus();
                    }
                });
            }

            function editTransitionModal($scope, $element, existingTransition, callback) {
                var modalComponent = {
                    title: existingTransition ?
                        $filter('translate')('statemachine.modals.transition.edit-title') :
                        $filter('translate')('statemachine.modals.transition.new-title'),
                    entity: existingTransition || {
                        trigger: '',
                        icon: '',
                        description: ''
                    },
                    fields: [
                        {
                            name: 'trigger',
                            title: $filter('translate')('statemachine.modals.transition.trigger'),
                            valueType: 'ShortText',
                            isRequired: true
                        },
                        {
                            name: 'icon',
                            title: $filter('translate')('statemachine.modals.transition.icon'),
                            valueType: 'ShortText'
                        },
                        {
                            name: 'description',
                            title: $filter('translate')('statemachine.modals.common.description'),
                            valueType: 'LongText'
                        }
                    ],
                    okButtonCaption: $filter('translate')('statemachine.modals.common.ok'),
                    okAction: function () {
                        const trigger = modalComponent.entity.trigger.trim();
                        if (!trigger) {
                            alert($filter('translate')('statemachine.modals.transition.trigger-required'));
                            return;
                        }

                        callback({
                            trigger: trigger,
                            icon: modalComponent.entity.icon.trim(),
                            description: modalComponent.entity.description.trim()
                        });

                        $scope.modalData = null;
                    },
                    cancelAction: function () {
                        stateMachineTransitionService.removeTempLine($scope);
                        isTransitioning = false;

                        $scope.modalData = null;
                    }
                };

                $scope.modalData = modalComponent;

                $timeout(() => {
                    const triggerInput = $element[0].querySelector('input[name="trigger"]');
                    if (triggerInput) {
                        triggerInput.focus();
                    }
                });
            }

            function editLocalization($scope, $element, item, languages, existedTranslations, saveCallback) {
                var itemText = item.name || item.trigger;
                var localizations = [];
                languages.forEach(language => {
                    localizations.push({
                        locale: language,
                        item: itemText,
                        value: existedTranslations?.find(x => x.locale === language)?.value
                    });
                });
                var modalComponent = {
                    title: $filter('translate')('statemachine.modals.localization.edit-title', { itemText: itemText }),
                    entity: localizations,
                    isArray: true,
                    fields: [
                        {
                            name: 'locale',
                            title: '',
                            valueType: 'Label'
                        },
                        {
                            name: 'value',
                            title: '',
                            valueType: 'ShortText'
                        }
                    ],
                    okButtonCaption: $filter('translate')('statemachine.modals.common.save'),
                    okAction: function () {
                        if (saveCallback) {
                            saveCallback(localizations);
                        }
                        $scope.modalData = null;
                    },
                    cancelAction: function () {
                        $scope.modalData = null;
                    }
                };

                $scope.modalData = modalComponent;

                $timeout(() => {
                    const triggerInput = $element[0].querySelector('input[name="value"]');
                    if (triggerInput) {
                        triggerInput.focus();
                    }
                });
            }

            return {
                editStateModal: editStateModal,
                editTransitionModal: editTransitionModal,
                editLocalization: editLocalization
            };
        }
    ]);
