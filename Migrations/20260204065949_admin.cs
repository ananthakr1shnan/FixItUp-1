using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixItUp.Migrations
{
    /// <inheritdoc />
    public partial class admin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvgResponseTime", "Email", "FullName", "IsFastBidder", "IsTopRated", "IsVerifiedPro", "JobCompletionRate", "OnTimeArrivalRate", "PasswordHash", "Role", "TrustScore" },
                values: new object[] { 999, 0, "admin@fixitup.com", "System Administrator", false, false, true, 0.0, 0.0, "admin123", "Admin", 100 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999);
        }
    }
}
