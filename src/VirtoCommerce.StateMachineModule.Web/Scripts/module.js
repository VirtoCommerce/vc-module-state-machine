var moduleName = 'virtoCommerce.stateMachineModule';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .component('state-node', require('./components/state-node.component'))
    .component('state-transitions', require('./components/state-transitions.component'))
    .component('context-menu', require('./components/context-menu.component'))
    .component('modal-form', require('./components/modal-form.component'))
    .component('three-position-toggle', require('./components/three-position-toggle.component'))
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider.state('workspace.statemachine',
                {
                    url: '/state-machine',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: ['platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var blade = {
                                id: 'state-machine-list',
                                controller: 'virtoCommerce.stateMachineModule.stateMachineListController',
                                template: 'Modules/$(VirtoCommerce.StateMachine)/Scripts/blades/state-machine-list.tpl.html',
                                isClosingDisabled: true
                            };
                            bladeNavigationService.showBlade(blade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService',
        '$http', '$compile', '$state',
        'virtoCommerce.coreModule.common.dynamicExpressionService',
        function (mainMenuService,
            $http, $compile, $state,
            dynamicExpressionService
        ) {
            var stateMachineMenuItem = {
                path: 'configuration/state-machine',
                icon: 'fas fa-project-diagram',
                title: 'statemachine.main-menu.statemachine',
                priority: 2,
                action: function () { $state.go('workspace.statemachine'); },
                permission: 'statemachine:access'
            };
            mainMenuService.addMenuItem(stateMachineMenuItem);

            // Transition's condition template registration
            dynamicExpressionService.registerExpression({
                id: 'StateMachineConditionHasPermission',
                displayName: 'Permission ...',
                templateURL: 'StateMachineConditionHasPermission.html'
            });

            $http.get('Modules/$(VirtoCommerce.StateMachine)/Scripts/dynamicConditions/templates.html').then(function (response) {
                $compile(response.data);
            });
        }
    ]);
