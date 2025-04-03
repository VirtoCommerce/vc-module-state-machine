angular.module('virtoCommerce.stateMachineModule')
    .controller('virtoCommerce.stateMachineModule.stateMachineDetailController', [
            '$scope', 'platformWebApp.bladeNavigationService',
            'virtoCommerce.stateMachineModule.webApi',
        function ($scope, bladeNavigationService,
            webApi) {
            var blade = $scope.blade;
            blade.headIcon = 'far fa-plus-square';
            blade.title = '';
            blade.savingInProgress = false;

            function initializeBlade(data) {
                if (!blade.isNew) {
                    if (!data.statesGraph) {
                        data.statesGraph = JSON.stringify(data.states, null, 2);
                    }
                }
                blade.currentEntity = angular.copy(data);
                blade.origEntity = data;

                blade.isLoading = false;
            }

            blade.refresh = function () {
                initializeBlade(blade.currentEntity);
                blade.savingInProgress = false;
            };

            $scope.setForm = function (form) { $scope.formScope = form; }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            }

            function canSave() {
                return isDirty() && $scope.formScope && $scope.formScope.$valid && !blade.savingInProgress;
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
                webApi.updateStateMachineDefinition({
                    definition: blade.currentEntity
                },
                    function (data) {
                    blade.refresh();
                    if (blade.childrenBlades && blade.childrenBlades.length == 1
                        && blade.childrenBlades[0].refresh) {
                        blade.childrenBlades[0].refresh();
                    }
                        blade.parentBlade.refresh(true);
                    },
                function (error) {
                    bladeNavigationService.setError('Error ' + error.status, blade);
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
                        angular.copy(blade.origEntity, blade.currentEntity);
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

            blade.exportStateMachine = function () {
                try {
                    const stateMachineData = {
                        id: blade.currentEntity.id,
                        name: blade.currentEntity.name,
                        version: blade.currentEntity.version,
                        entityType: blade.currentEntity.entityType,
                        isActive: blade.currentEntity.isActive,
                        statesGraph: blade.currentEntity.statesGraph,
                        statesCapture: blade.currentEntity.statesCapture,
                        states: JSON.parse(blade.currentEntity.statesGraph),
                        createdDate: blade.currentEntity.createdDate,
                        modifiedDate: blade.currentEntity.modifiedDate,
                        createdBy: blade.currentEntity.createdBy,
                        modifiedBy: blade.currentEntity.modifiedBy
                    };

                    const jsonString = JSON.stringify(stateMachineData, null, 2);

                    // Create a new JSZip instance
                    const zip = new JSZip();

                    // Add the JSON file to the zip
                    zip.file(`${blade.currentEntity.name || 'state-machine-export'}.json`, jsonString);

                    // Generate the zip file
                    zip.generateAsync({
                        type: "blob",
                        compression: "DEFLATE",
                        compressionOptions: {
                            level: 6  // Normal compression level (1-9, where 6 is normal)
                        }
                    })
                        .then(function(content) {
                            // Create download link
                            const link = document.createElement('a');
                            link.href = URL.createObjectURL(content);
                            link.download = `${blade.currentEntity.name || 'state-machine-export'}.zip`;

                            // Trigger download
                            document.body.appendChild(link);
                            link.click();
                            document.body.removeChild(link);

                            // Clean up
                            URL.revokeObjectURL(link.href);
                        })
                        .catch(function(error) {
                            console.error('Error creating zip file:', error);
                        });
                } catch (error) {
                    console.error('Error exporting state machine:', error);
                }
            }

            blade.openVisualEditor = function () {
                var newBlade = {
                    id: "stateMachineVisualEditor",
                    currentEntity: blade.currentEntity.statesGraph,
                    controller: 'virtoCommerce.stateMachineModule.stateMachineVisualEditorController',
                    template: 'Modules/$(VirtoCommerce.StateMachine)/Scripts/blades/state-machine-visual-editor.tpl.html'
                };
                newBlade.parentBlade = blade;
                bladeNavigationService.showBlade(newBlade, blade);
            }

            blade.updateStateMachineData = function (data) {
                blade.currentEntity.statesGraph = data;
            }

            blade.updateStateMachineSnapshot = function (data) {
                $scope.$apply(() => {
                    blade.currentEntity.statesCapture = data;
                });
            }

            blade.refresh();
        }]);
