angular.module('virtoCommerce.stateMachineModule')
    .directive('ngRightClick', ['$parse', function ($parse) {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                var fn = $parse(attrs.ngRightClick);
                element.on('contextmenu', function (event) {
                    event.preventDefault();
                    scope.$apply(function () {
                        fn(scope, { $event: event });
                    });
                    return false;
                });
            }
        };
    }]);
