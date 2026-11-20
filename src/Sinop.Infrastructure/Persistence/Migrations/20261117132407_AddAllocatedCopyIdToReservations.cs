using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sinop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAllocatedCopyIdToReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllocatedCopyId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_AllocatedCopyId",
                table: "Reservations",
                column: "AllocatedCopyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_BookCopies_AllocatedCopyId",
                table: "Reservations",
                column: "AllocatedCopyId",
                principalTable: "BookCopies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_BookCopies_AllocatedCopyId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_AllocatedCopyId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "AllocatedCopyId",
                table: "Reservations");
        }
    }
}
