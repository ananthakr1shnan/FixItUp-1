using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixItUp.Migrations
{
    /// <inheritdoc />
    public partial class paymentandtask3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 5, 6, 0, 0, 566, DateTimeKind.Utc).AddTicks(2654));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 5, 5, 18, 38, 590, DateTimeKind.Utc).AddTicks(805));
        }
    }
}
