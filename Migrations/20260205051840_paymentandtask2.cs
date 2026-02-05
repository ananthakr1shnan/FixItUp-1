using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixItUp.Migrations
{
    /// <inheritdoc />
    public partial class paymentandtask2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 5, 5, 18, 38, 590, DateTimeKind.Utc).AddTicks(805));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 5, 4, 21, 30, 320, DateTimeKind.Utc).AddTicks(9981));
        }
    }
}
