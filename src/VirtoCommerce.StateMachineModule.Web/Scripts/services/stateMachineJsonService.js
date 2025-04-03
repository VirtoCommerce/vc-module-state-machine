angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineJsonService', [
        '$timeout',
        function ($timeout) {

            function initializeJsonEditor(jsonEditor, $scope, blade) {
                const formattedJson = JSON.stringify(JSON.parse(blade.currentEntity), null, 2);
                jsonEditor.value = formattedJson;

                jsonEditor.addEventListener('input', () => {
                    refreshJsonEditor(jsonEditor, $scope, blade);
                });

                jsonEditor.addEventListener('blur', () => {
                    refreshJsonEditor(jsonEditor, $scope, blade);
                });

            }

            function refreshJsonEditor(jsonEditor, $scope, blade) {
                try {
                    const jsonContent = jsonEditor.value;
                    const parsedJson = JSON.parse(jsonContent);

                    $scope.$apply(() => {
                        blade.currentEntity = JSON.stringify(parsedJson);
                        blade.parentBlade.updateStateMachineData(blade.currentEntity);
                    });

                    jsonEditor.style.border = '1px solid #ccc';
                } catch (error) {
                    jsonEditor.style.border = '1px solid #ff0000';
                }
            }

            return {
                initializeJsonEditor: initializeJsonEditor,
                refreshJsonEditor: refreshJsonEditor
            };
        }
    ]);
