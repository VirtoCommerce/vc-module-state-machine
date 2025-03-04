angular.module('virtoCommerce.stateMachineModule')
    .controller('virtoCommerce.stateMachineModule.stateMachineVisualEditorController', [
            '$scope',
        function ($scope) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-code';
            blade.title = 'statemachine.blades.state-machine-visual-editor.title';

            blade.refresh = function () {
                blade.origEntity = angular.copy(blade.currentEntity);
                blade.isLoading = false;
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
                    name: "platform.commands.reset", icon: 'fa fa-undo',
                    executeMethod: function () {
                        angular.copy(blade.origEntity, blade.currentEntity);
                    },
                    canExecuteMethod: isDirty
                }
            ];

            $scope.setForm = function (form) { $scope.formScope = form; }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            }

            $scope.addState = function () {
                alert("Add new State");
            }

            blade.refresh();
        }]);
