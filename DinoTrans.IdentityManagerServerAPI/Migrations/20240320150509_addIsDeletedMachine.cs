using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinoTrans.IdentityManagerServerAPI.Migrations
{
    /// <inheritdoc />
    public partial class addIsDeletedMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ContructionMachines",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ContructionMachines");
        }
    }
}
