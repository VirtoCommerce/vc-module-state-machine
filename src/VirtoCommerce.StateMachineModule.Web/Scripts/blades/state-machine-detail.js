angular.module('virtoCommerce.stateMachineModule')
    .controller('virtoCommerce.stateMachineModule.stateMachineDetailController', [
            '$scope', 'platformWebApp.bladeNavigationService',
            'virtoCommerce.stateMachineModule.webApi',
        function ($scope, bladeNavigationService,
            webApi) {
            var blade = $scope.blade;
            blade.headIcon = 'far fa-plus-square';
            blade.title = '';

            blade.refresh = function () {
                if (!blade.isNew) {
                    blade.currentEntity.statesSerialized = JSON.stringify(blade.currentEntity.states, null, 2);
                }
                blade.origEntity = angular.copy(blade.currentEntity);
                blade.isLoading = false;
            };
      
            $scope.setForm = function (form) { $scope.formScope = form; }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            }

            $scope.saveChanges = function () {
                blade.currentEntity.states = JSON.parse(blade.currentEntity.statesSerialized);
                if (!blade.currentEntity.version) {
                    blade.currentEntity.version = '0';
                }
                webApi.updateStateMachineDefinition({ definition: blade.currentEntity },
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
                        return isDirty() && $scope.formScope && $scope.formScope.$valid;
                    }
                },
                {
                    name: "platform.commands.reset", icon: 'fa fa-undo',
                    executeMethod: function () {
                        angular.copy(blade.origEntity, blade.currentEntity);
                    },
                    canExecuteMethod: isDirty
                }
            ];

            blade.openVisualEditor = function () {
                var newBlade = {
                    id: "stateMachineVisualEditor",
                    currentEntity: blade.currentEntity.statesSerialized,
                    controller: 'virtoCommerce.stateMachineModule.stateMachineVisualEditorController',
                    template: 'Modules/$(VirtoCommerce.StateMachine)/Scripts/blades/state-machine-visual-editor.tpl.html'
                };

                bladeNavigationService.showBlade(newBlade, blade);
            }

            blade.refresh();
        }]);
