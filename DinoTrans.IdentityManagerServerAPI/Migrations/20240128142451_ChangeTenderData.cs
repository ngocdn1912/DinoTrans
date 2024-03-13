using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinoTrans.IdentityManagerServerAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTenderData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContructionMachineNotes",
                table: "TenderContructionMachines");

            migrationBuilder.DropColumn(
                name: "DeiliverDate",
                table: "TenderContructionMachines");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "TenderContructionMachines");

            migrationBuilder.DropColumn(
                name: "DeliveryContact",
                table: "TenderContructionMachines");

            migrationBuilder.DropColumn(
                name: "PickUpAddress",
                table: "TenderContructionMachines");

            migrationBuilder.DropColumn(
                name: "PickUpContact",
                table: "TenderContructionMachines");

            migrationBuilder.DropColumn(
                name: "PickUpDate",
                table: "TenderContructionMachines");

            migrationBuilder.AddColumn<string>(
                name: "ContructionMachineNotes",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeiliverDate",
                table: "Tenders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryContact",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Documentations",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PickUpAddress",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickUpContact",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickUpDate",
                table: "Tenders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContructionMachineNotes",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "DeiliverDate",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "DeliveryContact",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "Documentations",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "PickUpAddress",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "PickUpContact",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "PickUpDate",
                table: "Tenders");

            migrationBuilder.AddColumn<string>(
                name: "ContructionMachineNotes",
                table: "TenderContructionMachines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeiliverDate",
                table: "TenderContructionMachines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "TenderContructionMachines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryContact",
                table: "TenderContructionMachines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PickUpAddress",
                table: "TenderContructionMachines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PickUpContact",
                table: "TenderContructionMachines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PickUpDate",
                table: "TenderContructionMachines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
