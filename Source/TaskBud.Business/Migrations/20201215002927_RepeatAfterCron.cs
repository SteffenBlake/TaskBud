using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskBud.Business.Migrations
{
    public partial class RepeatAfterCron : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepeatAfterCount",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "RepeatAfterType",
                table: "TaskItems");

            migrationBuilder.AddColumn<string>(
                name: "RepeatCron",
                table: "TaskItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartingAssignedUserId",
                table: "TaskItems",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_StartingAssignedUserId",
                table: "TaskItems",
                column: "StartingAssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_AspNetUsers_StartingAssignedUserId",
                table: "TaskItems",
                column: "StartingAssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_AspNetUsers_StartingAssignedUserId",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_StartingAssignedUserId",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "RepeatCron",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "StartingAssignedUserId",
                table: "TaskItems");

            migrationBuilder.AddColumn<int>(
                name: "RepeatAfterCount",
                table: "TaskItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatAfterType",
                table: "TaskItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
