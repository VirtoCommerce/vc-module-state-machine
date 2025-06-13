using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.StateMachineModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var ddl = @"
            CREATE TABLE IF NOT EXISTS ""StateMachineDefinition"" (
                ""Id"" varchar(128) NOT NULL DEFAULT gen_random_uuid(),
                ""Name"" varchar(512) NULL,
                ""EntityType"" varchar(128) NOT NULL,
                ""IsActive"" boolean NOT NULL,
                ""Version"" varchar(32) NOT NULL,
                ""StatesSerialized"" text NULL,
                ""CreatedDate"" timestamp with time zone NOT NULL,
                ""ModifiedDate"" timestamp with time zone NULL,
                ""CreatedBy"" varchar(64) NULL,
                ""ModifiedBy"" varchar(64) NULL,
                PRIMARY KEY(""Id"")
            );

            CREATE TABLE IF NOT EXISTS ""StateMachineInstance"" (
                ""Id"" varchar(128) NOT NULL DEFAULT gen_random_uuid(),
                ""EntityId"" varchar(512) NULL,
                ""EntityType"" varchar(128) NOT NULL,
                ""StateMachineId"" varchar(128) NOT NULL,
                ""State"" varchar(128) NOT NULL,
                ""CreatedDate"" timestamp with time zone NOT NULL,
                ""ModifiedDate"" timestamp with time zone NULL,
                ""CreatedBy"" varchar(64) NULL,
                ""ModifiedBy"" varchar(64) NULL,
                PRIMARY KEY(""Id""),
                CONSTRAINT ""FK_StateMachineInstance_StateMachineDefinition_StateMachineId""
                FOREIGN KEY(""StateMachineId"")
                REFERENCES ""StateMachineDefinition""(""Id"")
                ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS ""IX_StateMachineInstance_StateMachineId""
                ON ""StateMachineInstance""(""StateMachineId"");
            ";
            migrationBuilder.Sql(ddl);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DROP TABLE IF EXISTS ""StateMachineInstance"";
            DROP TABLE IF EXISTS ""StateMachineDefinition"";
            ");
        }
    }
}
