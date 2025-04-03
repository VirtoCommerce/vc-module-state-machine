angular.module('virtoCommerce.stateMachineModule')
    .controller('virtoCommerce.stateMachineModule.stateMachineListController', [
        '$timeout', '$scope', 'platformWebApp.bladeUtils',
        'virtoCommerce.stateMachineModule.webApi',
        'platformWebApp.uiGridHelper',
        function ($timeout, $scope, bladeUtils,
            webApi,
            uiGridHelper) {
            var blade = $scope.blade;
            blade.headIcon = 'far fa-plus-square';
            blade.title = 'statemachine.blades.state-machine-list.title';
            blade.subtitle = '';
            blade.importInProgress = false;
            $scope.uiGridConstants = uiGridHelper.uiGridConstants;
            $scope.hasMore = true;
            $scope.items = [];

            var bladeNavigationService = bladeUtils.bladeNavigationService;
            blade.isLoading = true;

            blade.refresh = function (parentRefresh) {
                $scope.items = [];

                if ($scope.pageSettings.currentPage !== 1) {
                    $scope.pageSettings.currentPage = 1;
                }

                loadData(function () {
                    if (parentRefresh && blade.parentRefresh) {
                        blade.parentRefresh();
                    }
                });

                resetStateGrid();
            };

            function loadData(callback) {
                blade.isLoading = true;

                var pagedDataQuery = {
                    objectType: blade.objectType,
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                }
                webApi.searchStateMachineDefinition(
                    pagedDataQuery,
                    function (data) {
                        blade.isLoading = false;
                        $scope.pageSettings.totalItems = data.totalCount;
                        $scope.items = $scope.items.concat(data.results);
                        $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;

                        if (callback) {
                            callback();
                        }
                    });
            }

            function resetStateGrid() {
                if ($scope.gridApi) {
                    $scope.items = [];
                    $scope.gridApi.infiniteScroll.resetScroll(true, true);
                    $scope.gridApi.infiniteScroll.dataLoaded();
                }
            }

            blade.setSelectedItem = function (listItem) {
                $scope.selectedNodeId = listItem.id;
            };

            $scope.selectItem = function (e, listItem) {
                blade.setSelectedItem(listItem);

                var newBlade = {
                    subtitle: '',
                    currentEntity: listItem
                };
                openDetailsBlade(newBlade);
            };

            function openDetailsBlade(node) {

                var newBlade = {
                    id: "stateMachineDetail",
                    objectType: blade.objectType,
                    controller: 'virtoCommerce.stateMachineModule.stateMachineDetailController',
                    template: 'Modules/$(VirtoCommerce.StateMachine)/Scripts/blades/state-machine-detail.tpl.html'
                };
                angular.extend(newBlade, node);

                bladeNavigationService.showBlade(newBlade, blade);
            }

            function showMore() {
                if ($scope.hasMore) {
                    ++$scope.pageSettings.currentPage;
                    $scope.gridApi.infiniteScroll.saveScrollPercentage();
                    loadData(function () {
                        $scope.gridApi.infiniteScroll.dataLoaded();
                    });
                }
            }

            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh",
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "statemachine.blades.state-machine-list.commands.new",
                    icon: 'fas fa-plus',
                    executeMethod: function () {
                        $scope.selectedNodeId = undefined;
                        var newBlade = {
                            subtitle: '',
                            currentEntity: {},
                            isNew: true,
                            onChangesConfirmedFn: function (entry) {
                                $scope.selectedNodeId = entry.id;
                            }
                        };
                        openDetailsBlade(newBlade);
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "statemachine.blades.state-machine-list.commands.import",
                    icon: 'fa fa-download',
                    executeMethod: function () {
                        blade.importStateMachine();
                    },
                    canExecuteMethod: function () {
                        return !blade.importInProgress;
                    }
                }
            ];

            blade.importStateMachine = function () {
                try {
                    // Create a file input element
                    const input = document.createElement('input');
                    input.type = 'file';
                    input.accept = '.zip';

                    // Handle file selection
                    input.onchange = function (e) {
                        const file = e.target.files[0];
                        if (!file) return;

                        blade.importInProgress = true;
                        blade.isLoading = true;
                        // Create a new JSZip instance
                        const zip = new JSZip();

                        // Load the zip file
                        zip.loadAsync(file)
                            .then(function (zip) {
                                // Find the first JSON file in the zip
                                const jsonFile = Object.values(zip.files).find(file =>
                                    file.name.endsWith('.json') && !file.dir
                                );

                                if (!jsonFile) {
                                    throw new Error('No JSON file found in the zip archive');
                                }

                                // Read the JSON file content
                                return jsonFile.async('string');
                            })
                            .then(function (jsonString) {
                                try {
                                    const importedData = JSON.parse(jsonString);

                                    // Validate the imported data structure
                                    if (!importedData.states || !Array.isArray(importedData.states)) {
                                        throw new Error('Invalid state machine format');
                                    }

                                    webApi.updateStateMachineDefinition({
                                        definition: importedData
                                    },
                                    function (data) {
                                        blade.importInProgress = false;
                                        blade.isLoading = false;
                                        blade.refresh();
                                    });

                                } catch (error) {
                                    console.error('Error parsing imported file:', error);
                                }
                            })
                            .catch(function (error) {
                                console.error('Error processing zip file:', error);
                            });
                    };

                    input.click();
                } catch (error) {
                    console.error('Error importing state machine:', error);
                }
            }

            $scope.setGridOptions = function (gridOptions) {
                bladeUtils.initializePagination($scope, true);
                $scope.pageSettings.itemsPerPageCount = 20;
                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    $scope.gridApi = gridApi;
                    $scope.gridApi.infiniteScroll.on.needLoadMoreData($scope, showMore);
                });

                $timeout(function () {
                    blade.refresh();
                });
            };

    }]);
