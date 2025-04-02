angular.module('virtoCommerce.stateMachineModule')
    .component('threePositionToggle', {
        bindings: {
            toggleId: '@',
            leftLabel: '@',
            rightLabel: '@',
            ngModel: '=',
            onChange: '&'
        },
        templateUrl: 'Modules/$(VirtoCommerce.StateMachine)/Scripts/components/three-position-toggle.tpl.html',
        controller: ['$scope',
            function ($scope) {
                var $ctrl = this;

                $ctrl.togglePosition = function (event) {
                    const toggle = event.currentTarget;
                    const rect = toggle.getBoundingClientRect();
                    const clickX = event.clientX - rect.left;
                    const width = rect.width;

                    if (clickX < width / 3) {
                        $ctrl.ngModel = 'left';
                    } else if (clickX > (width * 2 / 3)) {
                        $ctrl.ngModel = 'right';
                    } else {
                        $ctrl.ngModel = 'center';
                    }

                    $ctrl.onChange({ $position: $ctrl.ngModel });
                };
            }]
    });
