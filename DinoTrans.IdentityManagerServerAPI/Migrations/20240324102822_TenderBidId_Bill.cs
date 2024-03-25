using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinoTrans.IdentityManagerServerAPI.Migrations
{
    /// <inheritdoc />
    public partial class TenderBidId_Bill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TenderId",
                table: "Bills",
                newName: "TenderBidId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TenderBidId",
                table: "Bills",
                newName: "TenderId");
        }
    }
}
