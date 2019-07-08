using Microsoft.EntityFrameworkCore.Migrations;

namespace Calendar.Data.Migrations
{
    public partial class changestringtoint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Locations_locationId",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "locationId",
                table: "Rooms",
                newName: "LocationId");

            migrationBuilder.RenameColumn(
                name: "RoomPropities",
                table: "Rooms",
                newName: "RoomProperties");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_locationId",
                table: "Rooms",
                newName: "IX_Rooms_LocationId");

            migrationBuilder.AlterColumn<int>(
                name: "RoomNumber",
                table: "Rooms",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CalendarId",
                table: "Rooms",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Locations_LocationId",
                table: "Rooms",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Locations_LocationId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CalendarId",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "Rooms",
                newName: "locationId");

            migrationBuilder.RenameColumn(
                name: "RoomProperties",
                table: "Rooms",
                newName: "RoomPropities");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_LocationId",
                table: "Rooms",
                newName: "IX_Rooms_locationId");

            migrationBuilder.AlterColumn<string>(
                name: "RoomNumber",
                table: "Rooms",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Locations_locationId",
                table: "Rooms",
                column: "locationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
