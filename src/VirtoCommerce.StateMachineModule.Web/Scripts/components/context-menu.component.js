angular.module('virtoCommerce.stateMachineModule')
    .component('contextMenu', {
        bindings: {
            items: '<',
            x: '<',
            y: '<',
        },
        templateUrl: 'Modules/$(VirtoCommerce.StateMachine)/Scripts/components/context-menu.tpl.html',
        controller: ['$timeout',
            function ($timeout) {
                var $ctrl = this;

                $ctrl.doAction = function (action) {
                    if (action) {
                        action();
                    }
                }
            }]
    });
