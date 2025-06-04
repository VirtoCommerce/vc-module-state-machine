angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineExportImportService', [
        'virtoCommerce.stateMachineModule.webApi',
        function (webApi) {

            function exportStateMachine(stateMachineDefinition) {
                return new Promise((resolve, reject) => {
                    try {
                        const stateMachineData = {
                            id: stateMachineDefinition.id,
                            name: stateMachineDefinition.name,
                            version: stateMachineDefinition.version,
                            entityType: stateMachineDefinition.entityType,
                            isActive: stateMachineDefinition.isActive,
                            statesGraph: stateMachineDefinition.statesGraph,
                            statesCapture: stateMachineDefinition.statesCapture,
                            states: JSON.parse(stateMachineDefinition.statesGraph),
                            createdDate: stateMachineDefinition.createdDate,
                            modifiedDate: stateMachineDefinition.modifiedDate,
                            createdBy: stateMachineDefinition.createdBy,
                            modifiedBy: stateMachineDefinition.modifiedBy
                        };

                        // Get all localizations for this state machine
                        const localizationPromises = [];

                        // Collect all state names and transition triggers for localization lookup
                        const itemsToLocalize = new Set();

                        // Add state names
                        if (stateMachineData.states && Array.isArray(stateMachineData.states)) {
                            stateMachineData.states.forEach(state => {
                                if (state.name) {
                                    itemsToLocalize.add(state.name);
                                }
                            });

                            // Add transition triggers
                            stateMachineData.states.forEach(state => {
                                if (state.transitions && Array.isArray(state.transitions)) {
                                    state.transitions.forEach(transition => {
                                        if (transition.trigger) {
                                            itemsToLocalize.add(transition.trigger);
                                        }
                                    });
                                }
                            });
                        }

                        // Fetch localizations for all items
                        Array.from(itemsToLocalize).forEach(item => {
                            const searchCriteria = {
                                definitionId: stateMachineDefinition.id,
                                item: item
                            };
                            localizationPromises.push(
                                webApi.searchStateMachineLocalization(searchCriteria).$promise
                            );
                        });

                        // If no items to localize, proceed directly with export
                        if (localizationPromises.length === 0) {
                            stateMachineData.localizations = [];
                            _createZipFile(stateMachineData)
                                .then(resolve)
                                .catch(reject);
                            return;
                        }

                        // Wait for all localizations to be fetched
                        Promise.all(localizationPromises)
                            .then(function(localizationResults) {
                                // Combine all localizations
                                const allLocalizations = [];
                                localizationResults.forEach(result => {
                                    if (result && result.results && Array.isArray(result.results)) {
                                        allLocalizations.push(...result.results);
                                    }
                                });

                                // Add localizations to export data
                                stateMachineData.localizations = allLocalizations;

                                return _createZipFile(stateMachineData);
                            })
                            .then(resolve)
                            .catch(function(error) {
                                console.error('Error fetching localizations:', error);

                                // Fallback: export without localizations
                                stateMachineData.localizations = [];
                                _createZipFile(stateMachineData)
                                    .then(resolve)
                                    .catch(reject);
                            });

                    } catch (error) {
                        console.error('Error exporting state machine:', error);
                        reject(error);
                    }
                });
            }

            function importStateMachine(file) {
                return new Promise((resolve, reject) => {
                    try {
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
                                const importedData = JSON.parse(jsonString);

                                // Validate the imported data structure
                                if (!importedData.states || !Array.isArray(importedData.states)) {
                                    throw new Error('Invalid state machine format');
                                }

                                // Extract localizations if present
                                const localizations = importedData.localizations || [];

                                // Remove localizations from the definition data before creating
                                const { localizations: _, ...stateMachineDefinition } = importedData;

                                return _importStateMachineDefinition(stateMachineDefinition, localizations);
                            })
                            .then(resolve)
                            .catch(reject);
                    } catch (error) {
                        console.error('Error importing state machine:', error);
                        reject(error);
                    }
                });
            }

            function _createZipFile(stateMachineData) {
                return new Promise((resolve, reject) => {
                    try {
                        const jsonString = JSON.stringify(stateMachineData, null, 2);
                        const zip = new JSZip();
                        zip.file(`${stateMachineData.name || 'state-machine-export'}.json`, jsonString);

                        zip.generateAsync({
                            type: "blob",
                            compression: "DEFLATE",
                            compressionOptions: {
                                level: 6  // Normal compression level (1-9, where 6 is normal)
                            }
                        })
                            .then(function(content) {
                                // Create download link
                                const link = document.createElement('a');
                                link.href = URL.createObjectURL(content);
                                link.download = `${stateMachineData.name || 'state-machine-export'}.zip`;

                                // Trigger download
                                document.body.appendChild(link);
                                link.click();
                                document.body.removeChild(link);

                                // Clean up
                                URL.revokeObjectURL(link.href);

                                resolve();
                            })
                            .catch(reject);
                    } catch (error) {
                        reject(error);
                    }
                });
            }

            function _importStateMachineDefinition(stateMachineDefinition, localizations) {
                return new Promise((resolve, reject) => {
                    webApi.updateStateMachineDefinition({
                        definition: stateMachineDefinition
                    }).$promise
                    .then(function (createdDefinition) {
                        // If there are localizations and we have a definition ID, import them
                        if (localizations.length > 0 && createdDefinition && createdDefinition.id) {
                            // Update localization definition IDs to match the newly created definition
                            const updatedLocalizations = localizations.map(localization => ({
                                ...localization,
                                definitionId: createdDefinition.id
                            }));

                            // Import localizations
                            webApi.updateStateMachineLocalization({
                                localizations: updatedLocalizations
                            }).$promise
                            .then(function () {
                                resolve(createdDefinition);
                            })
                            .catch(function (localizationError) {
                                console.error('Error importing localizations:', localizationError);
                                // Continue without localizations - the state machine itself was imported successfully
                                resolve(createdDefinition);
                            });
                        } else {
                            // No localizations to import
                            resolve(createdDefinition);
                        }
                    })
                    .catch(reject);
                });
            }

            return {
                exportStateMachine: exportStateMachine,
                importStateMachine: importStateMachine
            };
        }
    ]);
