angular.module('virtoCommerce.stateMachineModule')
    .controller('virtoCommerce.stateMachineModule.stateMachineConditionController',
        ['$scope', 'virtoCommerce.coreModule.common.dynamicExpressionService',
            'virtoCommerce.stateMachineModule.stateMachineTypes',
            function ($scope, dynamicExpressionService,
                stateMachineTypes
            ) {
            var blade = $scope.blade;
            blade.isLoading = false;

            function initializeBlade(data) {
                if (data.children) {
                    data.children.forEach(x => extendElementBlock(x));
                }

                blade.currentEntity = data;
                blade.originalEntity = angular.copy(data);

                blade.isLoading = false;
            }

            blade.refresh = function (parentRefresh) {
                blade.isLoading = true;
                if (blade.currentTransition.condition) {
                    initializeBlade(blade.currentTransition.condition);
                }
                else {
                    if (blade.currentEntityType) {
                        var currentEntityTypeInfo = stateMachineTypes.getTypeInfo(blade.currentEntityType);
                        if (currentEntityTypeInfo && currentEntityTypeInfo.getConditionTreeCallback) {
                            currentEntityTypeInfo.getConditionTreeCallback({}, function (result) {
                                initializeBlade(result.result);
                            });
                        }
                    }
                }

                if (parentRefresh) {
                    blade.parentBlade.refresh(true);
                }
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.reset", icon: 'fa fa-undo',
                    executeMethod: function () {
                        blade.reset();
                    }, 
                    canExecuteMethod: isDirty
                }
            ];

            blade.reset = function (resetValue)
            {
                if (resetValue) {
                    blade.currentTransition.condition = resetValue;
                }
                else {
                    blade.currentTransition.condition = angular.copy(blade.originalEntity);
                }
                blade.refresh();
            }

            $scope.setForm = function (form) { $scope.formScope = form; };

            $scope.saveChanges = function () {
                if (blade.currentEntity) {
                    stripOffUiInformation(blade.currentEntity);
                }

                blade.currentTransition.condition = blade.currentEntity;
                if (blade.saveCallback) {
                    blade.saveCallback(blade.currentTransition, blade.currentEntity);
                };
                $scope.bladeClose();
            }

            $scope.cancelChanges = function(){
                $scope.bladeClose();
            }

            $scope.isValid = function(){
                return isDirty();
            }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.originalEntity);
            }

            function extendElementBlock(expressionBlock) {
                var retVal = dynamicExpressionService.expressions[expressionBlock.id];
                if (!retVal) {
                    return expressionBlock;
                }

                angular.extend(expressionBlock, retVal);

                if (!expressionBlock.children) {
                    expressionBlock.children = [];
                }

                expressionBlock.children.forEach(x => extendElementBlock(x));
                expressionBlock.availableChildren.forEach(x => extendElementBlock(x));
                return expressionBlock;
            }

            function stripOffUiInformation(expressionElement) {
                //expressionElement.availableChildren = undefined;
                expressionElement.displayName = undefined;
                expressionElement.getValidationError = undefined;
                expressionElement.groupName = undefined;
                expressionElement.newChildLabel = undefined;
                expressionElement.templateURL = undefined;

                expressionElement.children.forEach(x => stripOffUiInformation(x));
            };

            blade.refresh();
        }
    ]);
