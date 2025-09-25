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

                        const localizationPromises = [];
                        const attributePromises = [];

                        const itemsToLocalize = new Set();
                        const itemsToAttribute = new Set();

                        if (stateMachineData.states && Array.isArray(stateMachineData.states)) {
                            stateMachineData.states.forEach(state => {
                                if (state.name) {
                                    itemsToLocalize.add(state.name);
                                    itemsToAttribute.add(state.name);
                                }
                            });

                            stateMachineData.states.forEach(state => {
                                if (state.transitions && Array.isArray(state.transitions)) {
                                    state.transitions.forEach(transition => {
                                        if (transition.trigger) {
                                            itemsToLocalize.add(transition.trigger);
                                            itemsToAttribute.add(transition.trigger);
                                        }
                                    });
                                }
                            });
                        }

                        if (itemsToLocalize.size > 0) {
                            const searchCriteria = {
                                definitionId: stateMachineDefinition.id
                            };
                            localizationPromises.push(
                                webApi.searchStateMachineLocalization(searchCriteria).$promise
                            );
                        }

                        if (itemsToAttribute.size > 0) {
                            const searchCriteria = {
                                definitionId: stateMachineDefinition.id
                            };
                            attributePromises.push(
                                webApi.searchStateMachineAttribute(searchCriteria).$promise
                            );
                        }

                        if (localizationPromises.length === 0 && attributePromises.length === 0) {
                            stateMachineData.localizations = [];
                            stateMachineData.attributes = [];
                            _createZipFile(stateMachineData)
                                .then(resolve)
                                .catch(reject);
                            return;
                        }

                        const localizationPromise = localizationPromises.length > 0 ? Promise.all(localizationPromises) : Promise.resolve([]);
                        const attributePromise = attributePromises.length > 0 ? Promise.all(attributePromises) : Promise.resolve([]);

                        Promise.all([localizationPromise, attributePromise])
                            .then(function(results) {
                                const [localizationResults, attributeResults] = results;

                                const allLocalizations = [];
                                if (Array.isArray(localizationResults)) {
                                    localizationResults.forEach(result => {
                                        if (result && result.results && Array.isArray(result.results)) {
                                            allLocalizations.push(...result.results);
                                        }
                                    });
                                }

                                const allAttributes = [];
                                if (Array.isArray(attributeResults)) {
                                    attributeResults.forEach(result => {
                                        if (result && result.results && Array.isArray(result.results)) {
                                            allAttributes.push(...result.results);
                                        }
                                    });
                                }

                                stateMachineData.localizations = allLocalizations;
                                stateMachineData.attributes = allAttributes;

                                return _createZipFile(stateMachineData);
                            })
                            .then(resolve)
                            .catch(function(error) {
                                console.error('Error fetching localizations or attributes:', error);

                                stateMachineData.localizations = [];
                                stateMachineData.attributes = [];
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
                        const zip = new JSZip();

                        zip.loadAsync(file)
                            .then(function (zip) {
                                const jsonFile = Object.values(zip.files).find(file =>
                                    file.name.endsWith('.json') && !file.dir
                                );

                                if (!jsonFile) {
                                    throw new Error('No JSON file found in the zip archive');
                                }

                                return jsonFile.async('string');
                            })
                            .then(function (jsonString) {
                                const importedData = JSON.parse(jsonString);

                                if (!importedData.states || !Array.isArray(importedData.states)) {
                                    throw new Error('Invalid state machine format');
                                }

                                const localizations = importedData.localizations || [];
                                const attributes = importedData.attributes || [];

                                const { localizations: _, attributes: __, ...stateMachineDefinition } = importedData;

                                return _importStateMachineDefinition(stateMachineDefinition, localizations, attributes);
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
                                level: 6 
                            }
                        })
                            .then(function(content) {
                                const link = document.createElement('a');
                                link.href = URL.createObjectURL(content);
                                link.download = `${stateMachineData.name || 'state-machine-export'}.zip`;

                                document.body.appendChild(link);
                                link.click();
                                document.body.removeChild(link);

                                URL.revokeObjectURL(link.href);

                                resolve();
                            })
                            .catch(reject);
                    } catch (error) {
                        reject(error);
                    }
                });
            }

            function _importStateMachineDefinition(stateMachineDefinition, localizations, attributes) {
                return new Promise((resolve, reject) => {
                    webApi.updateStateMachineDefinition({
                        definition: stateMachineDefinition
                    }).$promise
                    .then(function (createdDefinition) {
                        const importPromises = [];

                        if (localizations.length > 0 && createdDefinition && createdDefinition.id) {
                            const updatedLocalizations = localizations.map(localization => ({
                                ...localization,
                                definitionId: createdDefinition.id
                            }));

                            importPromises.push(
                                webApi.updateStateMachineLocalization({
                                    localizations: updatedLocalizations
                                }).$promise.catch(function (localizationError) {
                                    console.error('Error importing localizations:', localizationError);
                                    return null;
                                })
                            );
                        }

                        if (attributes.length > 0 && createdDefinition && createdDefinition.id) {
                            const updatedAttributes = attributes.map(attribute => ({
                                ...attribute,
                                definitionId: createdDefinition.id
                            }));

                            importPromises.push(
                                webApi.updateStateMachineAttribute({
                                    attributes: updatedAttributes
                                }).$promise.catch(function (attributeError) {
                                    console.error('Error importing attributes:', attributeError);
                                    return null;
                                })
                            );
                        }

                        if (importPromises.length > 0) {
                            Promise.all(importPromises)
                                .then(function () {
                                    resolve(createdDefinition);
                                })
                                .catch(function (error) {
                                    console.error('Error importing localizations or attributes:', error);
                                    resolve(createdDefinition);
                                });
                        } else {
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
