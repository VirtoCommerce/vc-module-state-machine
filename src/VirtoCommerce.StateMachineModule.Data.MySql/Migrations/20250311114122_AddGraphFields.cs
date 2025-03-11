using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.StateMachineModule.Data.MySql.Migrations
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
                type: "varchar(2083)",
                maxLength: 2083,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StatesGraph",
                table: "StateMachineDefinition",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
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
