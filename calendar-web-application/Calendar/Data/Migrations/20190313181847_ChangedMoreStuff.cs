using Microsoft.EntityFrameworkCore.Migrations;

namespace Calendar.Data.Migrations
{
    public partial class ChangedMoreStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Building",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Campus",
                table: "Rooms");

            migrationBuilder.AddColumn<int>(
                name: "BuildingID",
                table: "Rooms",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_BuildingID",
                table: "Rooms",
                column: "BuildingID");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Buildings_BuildingID",
                table: "Rooms",
                column: "BuildingID",
                principalTable: "Buildings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Buildings_BuildingID",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_BuildingID",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "BuildingID",
                table: "Rooms");

            migrationBuilder.AddColumn<string>(
                name: "Building",
                table: "Rooms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Campus",
                table: "Rooms",
                nullable: true);
        }
    }
}
