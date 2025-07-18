angular.module('virtoCommerce.stateMachineModule')
    .controller('virtoCommerce.stateMachineModule.stateMachineDetailController', [
        '$scope', 'platformWebApp.bladeNavigationService',
        'platformWebApp.dialogService',
        'platformWebApp.authService',
        'virtoCommerce.stateMachineModule.stateMachineTypes',
        'virtoCommerce.stateMachineModule.webApi',
        'virtoCommerce.stateMachineModule.stateMachineExportImportService',
        function ($scope, bladeNavigationService,
            dialogService,
            authService,
            stateMachineTypes,
            stateMachineApi,
            stateMachineExportImportService) {
            var blade = $scope.blade;
            blade.headIcon = 'far fa-plus-square';
            blade.title = '';
            blade.savingInProgress = false;
            blade.stateMachineRegisteredTypes = [];

            function initializeBlade(data) {
                if (!blade.isNew) {
                    if (!data.statesGraph) {
                        data.statesGraph = JSON.stringify(data.states, null, 2);
                    }
                }

                stateMachineApi.searchStateMachineDefinition({
                    objectTypes: [blade.currentEntity.entityType]
                },
                    (result) => blade.allEntityTypes = result.results
                );

                var searchCriteria = {
                    definitionId: blade.currentEntity.id
                };

                var promises = [
                    stateMachineApi.searchStateMachineLocalization(searchCriteria).$promise,
                    stateMachineApi.searchStateMachineAttribute(searchCriteria).$promise
                ];

                Promise.all(promises)
                    .then(function (results) {
                        blade.localizations = results[0] ? results[0].results : [];
                        blade.origLocalizations = angular.copy(blade.localizations);
                        blade.attributes = results[1] ? results[1].results : [];
                        blade.origAttributes = angular.copy(blade.attributes);

                    })
                    .catch(function (error) {
                        bladeNavigationService.setError('Error ' + (error.status || ''), blade);
                    });

                blade.currentEntity = data;
                blade.origEntity = angular.copy(data);

                blade.stateMachineRegisteredTypes = stateMachineTypes.getAllTypes();
                blade.updateAnotherActiveStateMachine = false;
                blade.isLoading = false;
            }

            blade.refresh = function () {
                initializeBlade(blade.currentEntity);
                blade.savingInProgress = false;
            };

            $scope.setForm = function (form) { $scope.formScope = form; }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity)
                    || !angular.equals(blade.localizations, blade.origLocalizations)
                    || !angular.equals(blade.attributes, blade.origAttributes);
            }

            function canSave() {
                return isDirty() && $scope.formScope && $scope.formScope.$valid && !blade.savingInProgress;
            }

            function showIsActiveConfirmation() {
                const dialog = {
                    id: "confirmIsActive",
                    title: 'Confirm action',
                    message: 'All others state machines of the same type will be disabled?',
                    callback: (confirmed) => {
                        if (!confirmed) {
                            blade.currentEntity.isActive = false;
                            blade.updateAnotherActiveStateMachine = false;
                        } else {
                            blade.anotherActiveStateMachine.isActive = false;
                            blade.updateAnotherActiveStateMachine = true;
                        }
                    }
                };
                dialogService.showConfirmationDialog(dialog);
            }

            $scope.saveChanges = async function () {
                blade.savingInProgress = true;
                blade.isLoading = true;
                if (blade.childrenBlades && blade.childrenBlades.length == 1
                    && blade.childrenBlades[0].recalculateStatePositions) {
                    blade.childrenBlades[0].recalculateStatePositions();
                }
                if (blade.childrenBlades && blade.childrenBlades.length == 1
                    && blade.childrenBlades[0].makeSnapshot) {
                    await blade.childrenBlades[0].makeSnapshot();
                }
                blade.currentEntity.states = JSON.parse(blade.currentEntity.statesGraph);
                if (!blade.currentEntity.version) {
                    blade.currentEntity.version = '0';
                }

                const promises = [
                    stateMachineApi.updateStateMachineDefinition({ definition: blade.currentEntity }).$promise,
                    stateMachineApi.updateStateMachineLocalization({ localizations: blade.localizations }),
                    stateMachineApi.updateStateMachineAttribute({ attributes: blade.attributes })
                ];

                if (blade.anotherActiveStateMachine) {
                    promises.push(
                        stateMachineApi.updateStateMachineDefinition({ definition: blade.anotherActiveStateMachine }).$promise
                    );
                }

                Promise.all(promises)
                    .then(function (results) {
                        blade.refresh();
                        if (blade.childrenBlades && blade.childrenBlades.length == 1
                            && blade.childrenBlades[0].refresh) {
                            blade.childrenBlades[0].refresh();
                        }
                        blade.parentBlade.refresh(true);
                    })
                    .catch(function (error) {
                        bladeNavigationService.setError('Error ' + (error.status || ''), blade);
                    });
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.save", icon: 'fas fa-save',
                    executeMethod: function () {
                        $scope.saveChanges();
                    },
                    canExecuteMethod: function () {
                        return canSave();
                    }
                },
                {
                    name: "platform.commands.reset", icon: 'fa fa-undo',
                    executeMethod: function () {
                        blade.currentEntity = angular.copy(blade.origEntity);
                        blade.localizations = angular.copy(blade.origLocalizations);
                        blade.attributes = angular.copy(blade.origAttributes);
                        if (blade.childrenBlades && blade.childrenBlades.length > 0) {
                            blade.childrenBlades.forEach(x => {
                                if (x.reset) {
                                    x.localizations = blade.localizations;
                                    x.attributes = blade.attributes;
                                    x.reset();
                                }
                            });
                        }
                    },
                    canExecuteMethod: isDirty
                },
                {
                    name: "statemachine.blades.state-machine-details.commands.export",
                    icon: 'fa fa-upload',
                    executeMethod: function () {
                        blade.exportStateMachine();
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                }
            ];

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "statemachine.dialogs.state-machine-details-save.title", "statemachine.dialogs.state-machine-details-save.message");
            };

            blade.onIsActiveChanged = function () {
                if (blade.currentEntity.isActive && blade.allEntityTypes) {
                    blade.anotherActiveStateMachine = blade.anotherActiveStateMachine = blade.allEntityTypes.find(x => x.isActive === true && x.id !== blade.currentEntity.id) || null;
                    if (blade.anotherActiveStateMachine != null) {
                        showIsActiveConfirmation();
                    }
                }
            }

            blade.exportStateMachine = function () {
                stateMachineExportImportService.exportStateMachine(blade.currentEntity)
                    .then(function() {
                        console.log('State machine exported successfully');
                    })
                    .catch(function(error) {
                        console.error('Error exporting state machine:', error);
                    });
            }

            blade.openVisualEditor = function () {
                var newBlade = {
                    id: "stateMachineVisualEditor",
                    stateMachineDefinitionId: blade.currentEntity.id,
                    currentEntity: blade.currentEntity.statesGraph,
                    localizations: blade.localizations,
                    attributes: blade.attributes,
                    controller: 'virtoCommerce.stateMachineModule.stateMachineVisualEditorController',
                    template: 'Modules/$(VirtoCommerce.StateMachine)/Scripts/blades/state-machine-visual-editor.tpl.html'
                };
                newBlade.parentBlade = blade;
                bladeNavigationService.showBlade(newBlade, blade);
            }

            blade.updateStateMachineData = function (data) {
                blade.currentEntity.statesGraph = data;
            }

            blade.updateLocalizationsAttributes = function (localizations, attributes) {
                blade.localizations = localizations;
                blade.attributes = attributes;
            }

            blade.updateStateMachineSnapshot = function (data) {
                $scope.$apply(() => {
                    blade.currentEntity.statesCapture = data;
                });
            }

            blade.canEdit = function () {
                return authService.checkPermission('statemachine:update');
            }

            blade.refresh();
        }]);
