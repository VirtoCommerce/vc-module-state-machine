angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineRegistrar', [function () {
        var registeredStates = {};

        return {
            registerStateAction: function (stateName, action) {
                registeredStates[stateName] = action;
            },
            getStateAction: function (stateName) {
                return registeredStates[stateName];
            }
        };
    }]);
