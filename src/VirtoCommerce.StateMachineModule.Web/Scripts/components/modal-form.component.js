angular.module('virtoCommerce.stateMachineModule')
    .component('modalForm', {
        bindings: {
            title: '<',
            entity: '<',
            fields: '<',
            okAction: '<',
            cancelAction: '<',
        },
        templateUrl: 'Modules/$(VirtoCommerce.StateMachine)/Scripts/components/modal-form.tpl.html',
        controller: ['$timeout',
            function ($timeout) {
                var $ctrl = this;

                $ctrl.doOkAction = function () {
                    if ($ctrl.okAction) {
                        $ctrl.okAction();
                    }
                }

                $ctrl.doCancelAction = function () {
                    if ($ctrl.cancelAction) {
                        $ctrl.cancelAction();
                    }
                }
            }]
    });
