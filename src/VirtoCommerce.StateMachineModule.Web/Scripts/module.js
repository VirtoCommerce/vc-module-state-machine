// Call this to register your module to main application
var moduleName = 'virtoCommerce.stateMachineModule';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
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
    .run(['platformWebApp.mainMenuService', '$state',
        function (mainMenuService, $state) {
            //Register module in main menu
            var stateMachineMenuItem = {
                path: 'configuration/state-machine',
                icon: 'fas fa-project-diagram',
                title: 'statemachine.main-menu.statemachine',
                priority: 2,
                action: function () { $state.go('workspace.statemachine'); },
                permission: 'statemachine:access'
            };
            mainMenuService.addMenuItem(stateMachineMenuItem);


        }
    ]);
