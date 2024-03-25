using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinoTrans.IdentityManagerServerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenderId = table.Column<int>(type: "int", nullable: false),
                    BillType = table.Column<int>(type: "int", nullable: false),
                    vnp_Amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vnp_BankCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vnp_BankTranNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vnp_CardType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vnp_OrderInfo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vnp_PayDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vnp_ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vnp_TmnCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vnp_TransactionNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vnp_TransactionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vnp_TxnRef = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vnp_SecureHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bills");
        }
    }
}
