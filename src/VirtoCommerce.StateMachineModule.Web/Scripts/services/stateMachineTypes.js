angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineTypes', [function () {
        var registeredTypes = [];

        return {
            addType: function (caption, value) {
                registeredTypes.push({caption: caption, value: value});
            },
            getAllTypes: function () {
                return registeredTypes;
            }
        };
    }]);
