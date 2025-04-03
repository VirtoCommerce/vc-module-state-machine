using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.StateMachineModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RenameGraphFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatesCaptureUrl",
                table: "StateMachineDefinition");

            migrationBuilder.AddColumn<string>(
                name: "StatesCapture",
                table: "StateMachineDefinition",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatesCapture",
                table: "StateMachineDefinition");

            migrationBuilder.AddColumn<string>(
                name: "StatesCaptureUrl",
                table: "StateMachineDefinition",
                type: "nvarchar(2083)",
                maxLength: 2083,
                nullable: true);
        }
    }
}
