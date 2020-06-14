using System;
using Microsoft.EntityFrameworkCore.Migrations;
using TaskBud.Business.Data;

namespace TaskBud.Business.Migrations
{
    public partial class Task_RepeatAfter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepeatAfterCount",
                table: "TaskItems",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatAfterType",
                table: "TaskItems",
                nullable: false,
                defaultValue: (int)RepeatType.Days);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "WaitUntil",
                table: "TaskItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepeatAfterCount",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "RepeatAfterType",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "WaitUntil",
                table: "TaskItems");
        }
    }
}
