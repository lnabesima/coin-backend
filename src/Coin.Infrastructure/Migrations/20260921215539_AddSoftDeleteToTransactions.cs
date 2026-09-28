using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Transactions");
        }
    }
}
