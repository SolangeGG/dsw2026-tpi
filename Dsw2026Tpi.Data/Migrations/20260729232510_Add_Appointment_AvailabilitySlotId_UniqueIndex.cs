using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2026Tpi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Appointment_AvailabilitySlotId_UniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments",
                column: "AvailabilitySlotId",
                unique: true,
                filter: "[Status] = 'Booked'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments",
                column: "AvailabilitySlotId");
        }
    }
}
