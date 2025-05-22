angular.module('virtoCommerce.stateMachineModule')
    .controller('virtoCommerce.stateMachineModule.permissionConditionController',
        ['$scope', 'platformWebApp.roles',
            function ($scope, roles) {
                roles.queryPermissions({ take: 10000 }, function (result) {
                    $scope.permissions = result.map(x => x.name);
                });
            }
        ]
    );
