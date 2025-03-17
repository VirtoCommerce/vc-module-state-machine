angular.module('virtoCommerce.stateMachineModule')
    .controller('virtoCommerce.stateMachineModule.stateMachineDetailController', [
            '$scope', 'platformWebApp.bladeNavigationService',
            'virtoCommerce.stateMachineModule.webApi',
        function ($scope, bladeNavigationService,
            webApi) {
            var blade = $scope.blade;
            blade.headIcon = 'far fa-plus-square';
            blade.title = '';
            blade.canSave = true;

            blade.refresh = function () {
                if (!blade.isNew) {
                    if (!blade.currentEntity.statesGraph) {
                        blade.currentEntity.statesGraph = JSON.stringify(blade.currentEntity.states, null, 2);
                    }
                }
                blade.origEntity = angular.copy(blade.currentEntity);
                blade.isLoading = false;
            };
      
            $scope.setForm = function (form) { $scope.formScope = form; }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            }

            $scope.saveChanges = async function () {
                blade.canSave = false;
                blade.isLoading = true;
                if (blade.childrenBlades && blade.childrenBlades.length == 1
                    && blade.childrenBlades[0].makeSnaphot) {
                    await blade.childrenBlades[0].makeSnaphot();
                }
                blade.currentEntity.states = JSON.parse(blade.currentEntity.statesGraph);
                if (!blade.currentEntity.version) {
                    blade.currentEntity.version = '0';
                }
                webApi.updateStateMachineDefinition({
                    definition: blade.currentEntity
                },
                    function (data) {
                        $scope.bladeClose();
                        blade.parentBlade.refresh(true);
                    },
                    function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.save", icon: 'fas fa-save',
                    executeMethod: function () {
                        $scope.saveChanges();
                    },
                    canExecuteMethod: function () {
                        return isDirty() && $scope.formScope && $scope.formScope.$valid && blade.canSave;
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
                },
                {
                    name: "statemachine.blades.state-machine-details.commands.import",
                    icon: 'fa fa-download',
                    executeMethod: function () {
                        blade.importStateMachine();
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                }
            ];

            blade.exportStateMachine = function () {
                alert("I am an export");
            }

            blade.importStateMachine = function () {
                alert("I am an import");
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
