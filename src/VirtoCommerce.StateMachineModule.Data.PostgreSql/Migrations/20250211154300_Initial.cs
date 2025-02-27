using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.StateMachineModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createStateMachineDefinitionTableScript = @"
                CREATE TABLE IF NOT EXISTS StateMachineDefinition (
                    Id character varying(128) NOT NULL PRIMARY KEY,
                    Name character varying(512) NULL,
                    EntityType character varying(128) NOT NULL, 
                    IsActive boolean NOT NULL,
                    Version character varying(32) NOT NULL,
                    StatesSerialized text NULL,
                    CreatedDate timestamp with time zone NOT NULL,
                    ModifiedDate timestamp with time zone NULL,
                    CreatedBy character varying(64) NULL,
                    ModifiedBy character varying(64) NULL
                );";

            migrationBuilder.Sql(createStateMachineDefinitionTableScript);

            var createStateMachineInstanceTableScript = @"
                CREATE TABLE IF NOT EXISTS StateMachineInstance (
                    Id character varying(128) NOT NULL PRIMARY KEY,
                    EntityId character varying(512) NULL,
                    EntityType character varying(128) NOT NULL, 
                    StateMachineId character varying(128) NOT NULL, 
                    State character varying(128) NOT NULL, 
                    CreatedDate timestamp with time zone NOT NULL,
                    ModifiedDate timestamp with time zone NULL,
                    CreatedBy character varying(64) NULL,
                    ModifiedBy character varying(64) NULL,
                    CONSTRAINT FK_StateMachineInstance_StateMachineDefinition_StateMachineId
                    FOREIGN KEY (StateMachineId)
                    REFERENCES StateMachineDefinition(Id)
                    ON DELETE CASCADE
                );";

            migrationBuilder.Sql(createStateMachineInstanceTableScript);

            var createStateMachineInstanceIndexScript = @"
                CREATE INDEX IF NOT EXISTS IX_StateMachineInstance_StateMachineId
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
