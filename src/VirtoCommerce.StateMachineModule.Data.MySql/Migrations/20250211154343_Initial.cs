using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.StateMachineModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            var createStateMachineDefinitionTableScript = @"
                CREATE TABLE IF NOT EXISTS `StateMachineDefinition` (
                    `Id` varchar(128) NOT NULL default (uuid()) PRIMARY KEY,
                    `Name` varchar(512) NULL,
                    `EntityType` varchar(128) NOT NULL, 
                    `IsActive` boolean NOT NULL,
                    `Version` varchar(32) NOT NULL,
                    `StatesSerialized` longtext NULL,
                    `CreatedDate` datetime(6) NOT NULL,
                    `ModifiedDate` datetime(6) NULL,
                    `CreatedBy` varchar(64) NULL,
                    `ModifiedBy` varchar(64) NULL
                );";

            migrationBuilder.Sql(createStateMachineDefinitionTableScript);

            var createStateMachineInstanceTableScript = @"
                CREATE TABLE IF NOT EXISTS `StateMachineInstance` (
                    `Id` varchar(128) NOT NULL default (uuid()) PRIMARY KEY,
                    `EntityId` varchar(512) NULL,
                    `EntityType` varchar(128) NOT NULL, 
                    `StateMachineId` varchar(128) NOT NULL, 
                    `State` varchar(128) NOT NULL, 
                    `CreatedDate` datetime(6) NOT NULL,
                    `ModifiedDate` datetime(6) NULL,
                    `CreatedBy` varchar(64) NULL,
                    `ModifiedBy` varchar(64) NULL,
                    CONSTRAINT `FK_StateMachineInstance_StateMachineDefinition_StateMachineId`
                    FOREIGN KEY (`StateMachineId`)
                    REFERENCES `StateMachineDefinition`(`Id`)
                    ON DELETE CASCADE
                );";

            migrationBuilder.Sql(createStateMachineInstanceTableScript);

            var createStateMachineInstanceIndexScript = @"
                CREATE PROCEDURE createsmindex()
                BEGIN
                IF (SELECT 1        
                    FROM `INFORMATION_SCHEMA`.`STATISTICS`
                    WHERE `TABLE_NAME` = 'StateMachineInstance'
                    AND `INDEX_NAME` = 'IX_StateMachineInstance_StateMachineId') IS NULL THEN

                    ALTER TABLE `StateMachineInstance` ADD INDEX `IX_StateMachineInstance_StateMachineId` (`StateMachineId` ASC);

                END IF;
                END;

                CALL createsmindex();
                DROP PROCEDURE createsmindex;
            ";

            migrationBuilder.Sql(createStateMachineInstanceIndexScript);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StateMachineInstance");

            migrationBuilder.DropTable(
                name: "StateMachineDefinition");
        }
    }
}
