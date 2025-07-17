angular.module('virtoCommerce.stateMachineModule')
    .component('contextMenu', {
        bindings: {
            items: '<',
            x: '<',
            y: '<',
        },
        templateUrl: 'Modules/$(VirtoCommerce.StateMachine)/Scripts/components/context-menu.tpl.html',
        controller: ['$timeout', 'platformWebApp.authService',
            function ($timeout, authService) {
                var $ctrl = this;

                $ctrl.$onInit = function () {
                    $ctrl.filteredItems = filterItems($ctrl.items);
                };

                $ctrl.$onChanges = function (changes) {
                    if (changes.items && changes.items.currentValue) {
                        $ctrl.filteredItems = filterItems(changes.items.currentValue);
                    }
                };

                function filterItems(items) {
                    var filteredByPermissionItems = filterItemsByPermissions(items);
                    var filteredByConditionItems = filterItemsByConditions(filteredByPermissionItems);
                    return filteredByConditionItems;
                }

                function filterItemsByPermissions(items) {
                    if (!items || !Array.isArray(items)) {
                        return [];
                    }

                    return items.filter(function (item) {
                        if (!item.permission) {
                            return true;
                        }

                        return authService.checkPermission(item.permission);
                    });
                }

                function filterItemsByConditions(items) {
                    var filteredItems = items.filter(x => {
                        if (x.isVisible) {
                            var isVisible = x.isVisible()
                            return isVisible;
                        }

                        return true;
                    });
                    return filteredItems;
                }

                $ctrl.doAction = function (action) {
                    if (action) {
                        action();
                    }
                }
            }]
    });
