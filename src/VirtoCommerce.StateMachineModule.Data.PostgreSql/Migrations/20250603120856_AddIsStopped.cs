using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.StateMachineModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddIsStopped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStopped",
                table: "StateMachineInstance",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStopped",
                table: "StateMachineInstance");
        }
    }
}
