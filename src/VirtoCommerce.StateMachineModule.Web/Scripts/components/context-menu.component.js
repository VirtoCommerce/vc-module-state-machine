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
                    $ctrl.filteredItems = filterItemsByPermissions($ctrl.items);
                };

                $ctrl.$onChanges = function (changes) {
                    if (changes.items && changes.items.currentValue) {
                        $ctrl.filteredItems = filterItemsByPermissions(changes.items.currentValue);
                    }
                };

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

                $ctrl.doAction = function (action) {
                    if (action) {
                        action();
                    }
                }
            }]
    });
