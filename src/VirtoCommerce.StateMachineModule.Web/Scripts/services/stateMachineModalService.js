angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineModalService', [
        '$timeout',
        'virtoCommerce.stateMachineModule.stateMachineStateService',
        'virtoCommerce.stateMachineModule.stateMachineTransitionService',
        function ($timeout,
            stateMachineStateService,
            stateMachineTransitionService
        ) {
            function editStateModal($scope, $element, x, y, startState = null, existingState = null, states, transitions, callback = null) {
                var modalComponent = {
                    title: existingState ? 'Edit State' : 'Add New State',
                    entity: existingState || {
                        id: '',
                        name: '',
                        description: ''
                    },
                    fields: [
                        {
                            name: 'name',
                            title: 'Name',
                            valueType: 'ShortText',
                            isRequired: true
                        },
                        {
                            name: 'description',
                            title: 'Description',
                            valueType: 'LongText'
                        }
                    ],
                    okButtonCaption: 'Ok',
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
                    title: existingTransition ? 'Edit Transition' : 'New Transition',
                    entity: existingTransition || {
                        trigger: '',
                        icon: '',
                        description: ''
                    },
                    fields: [
                        {
                            name: 'trigger',
                            title: 'Trigger',
                            valueType: 'ShortText',
                            isRequired: true
                        },
                        {
                            name: 'icon',
                            title: 'Icon',
                            valueType: 'ShortText'
                        },
                        {
                            name: 'description',
                            title: 'Description',
                            valueType: 'LongText'
                        }
                    ],
                    okButtonCaption: 'Ok',
                    okAction: function () {
                        const trigger = modalComponent.entity.trigger.trim();
                        if (!trigger) {
                            alert('Trigger cannot be empty');
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
                    title: 'Edit localization ' + itemText,
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
                    okButtonCaption: 'Save',
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
