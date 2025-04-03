using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.StateMachineModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddGraphFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatesCaptureUrl",
                table: "StateMachineDefinition",
                type: "character varying(2083)",
                maxLength: 2083,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatesGraph",
                table: "StateMachineDefinition",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatesCaptureUrl",
                table: "StateMachineDefinition");

            migrationBuilder.DropColumn(
                name: "StatesGraph",
                table: "StateMachineDefinition");
        }
    }
}
