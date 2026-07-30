using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2026Tpi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fix_AvailabilitySlot_UniqueIndex_Filter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AvailabilitySlots_DoctorId_SlotDate_StartTime",
                table: "AvailabilitySlots");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_DoctorId_SlotDate_StartTime",
                table: "AvailabilitySlots",
                columns: new[] { "DoctorId", "SlotDate", "StartTime" },
                unique: true,
                filter: "[Deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AvailabilitySlots_DoctorId_SlotDate_StartTime",
                table: "AvailabilitySlots");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_DoctorId_SlotDate_StartTime",
                table: "AvailabilitySlots",
                columns: new[] { "DoctorId", "SlotDate", "StartTime" },
                unique: true);
        }
    }
}
