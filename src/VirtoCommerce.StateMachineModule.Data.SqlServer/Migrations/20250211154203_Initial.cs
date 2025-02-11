using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.StateMachineModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createStateMachineDefinitionTableScript = @"
                IF OBJECT_ID(N'dbo.StateMachineDefinition', N'U') IS NULL
                    CREATE TABLE StateMachineDefinition (
                              Id nvarchar(128) NOT NULL PRIMARY KEY,
                              Name nvarchar(512) NULL,
                              EntityType nvarchar(128) NOT NULL, 
                              IsActive bit NOT NULL,
                              Version nvarchar(32) NOT NULL,
                              StatesSerialized nvarchar(max) NULL,
                              CreatedDate datetime2 NOT NULL,
                              ModifiedDate datetime2 NULL,
                              CreatedBy nvarchar(64) NULL,
                              ModifiedBy nvarchar(64) NULL
                    );";

            migrationBuilder.Sql(createStateMachineDefinitionTableScript);

            var createStateMachineInstanceTableScript = @"
                IF OBJECT_ID(N'dbo.StateMachineInstance', N'U') IS NULL
                    CREATE TABLE StateMachineInstance (
                              Id nvarchar(128) NOT NULL PRIMARY KEY,
                              EntityId nvarchar(512) NULL,
                              EntityType nvarchar(128) NOT NULL, 
                              StateMachineId nvarchar(128) NOT NULL, 
                              State nvarchar(128) NOT NULL, 
                              CreatedDate datetime2 NOT NULL,
                              ModifiedDate datetime2 NULL,
                              CreatedBy nvarchar(64) NULL,
                              ModifiedBy nvarchar(64) NULL,
                              CONSTRAINT FK_StateMachineInstance_StateMachineDefinition_StateMachineId
                              FOREIGN KEY (StateMachineId)
                              REFERENCES StateMachineDefinition(Id)
                              ON DELETE CASCADE
                    );";

            migrationBuilder.Sql(createStateMachineInstanceTableScript);

            var createStateMachineInstanceIndexScript = @"
                IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_StateMachineInstance_StateMachineId' AND object_id = OBJECT_ID('StateMachineInstance'))
                    CREATE INDEX IX_StateMachineInstance_StateMachineId
                    ON StateMachineInstance (StateMachineId);";

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
