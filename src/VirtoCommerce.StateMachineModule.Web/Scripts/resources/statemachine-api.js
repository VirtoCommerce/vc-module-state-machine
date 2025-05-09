angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.webApi', ['$resource', function ($resource) {
        return $resource('api/statemachine', null, {
            // state machine
            searchStateMachineDefinition: { method: 'POST', url: 'api/statemachine/definitions/search' },
            updateStateMachineDefinition: { method: 'POST', url: 'api/statemachine/definitions/new' },
            allStates: { method: 'GET', url: 'api/statemachine/definitions/allstates', isArray: true },
            searchStateMachineInstance: { method: 'POST', url: 'api/statemachine/instances/search' },
            getStateMachineInstanceById: { method: 'GET', url: 'api/statemachine/instances/:instanceId' },
            createStateMachineInstance: { method: 'POST', url: 'api/statemachine/instances/:definitionId/new' },
            fireStateMachineInstanceTrigger: { method: 'POST', url: 'api/statemachine/firetrigger' },
            // localizations
            searchStateMachineLocalization: { method: 'POST', url: 'api/statemachine/localization/search' },
            updateStateMachineLocalization: { method: 'POST', url: 'api/statemachine/localization/update' },
            // settings
            getStateMachineSettings: { method: 'GET', url: 'api/statemachine/settings' },
        });
    }]);
