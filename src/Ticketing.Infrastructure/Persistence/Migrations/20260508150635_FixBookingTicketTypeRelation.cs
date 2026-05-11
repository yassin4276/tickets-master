using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ticketing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixBookingTicketTypeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingTicketTypes_TicketTypes_Id",
                table: "BookingTicketTypes");

            migrationBuilder.DropIndex(
                name: "IX_BookingTicketTypes_BookingId_Id",
                table: "BookingTicketTypes");

            migrationBuilder.DropIndex(
                name: "IX_BookingTicketTypes_Id",
                table: "BookingTicketTypes");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "BookingTicketTypes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_BookingTicketTypes_BookingId_TicketTypesId",
                table: "BookingTicketTypes",
                columns: new[] { "BookingId", "TicketTypesId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingTicketTypes_TicketTypesId",
                table: "BookingTicketTypes",
                column: "TicketTypesId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingTicketTypes_TicketTypes_TicketTypesId",
                table: "BookingTicketTypes",
                column: "TicketTypesId",
                principalTable: "TicketTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingTicketTypes_TicketTypes_TicketTypesId",
                table: "BookingTicketTypes");

            migrationBuilder.DropIndex(
                name: "IX_BookingTicketTypes_BookingId_TicketTypesId",
                table: "BookingTicketTypes");

            migrationBuilder.DropIndex(
                name: "IX_BookingTicketTypes_TicketTypesId",
                table: "BookingTicketTypes");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "BookingTicketTypes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_BookingTicketTypes_BookingId_Id",
                table: "BookingTicketTypes",
                columns: new[] { "BookingId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingTicketTypes_Id",
                table: "BookingTicketTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingTicketTypes_TicketTypes_Id",
                table: "BookingTicketTypes",
                column: "Id",
                principalTable: "TicketTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
