using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskBud.Business.Migrations
{
    public partial class TaskItem_CreationDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreationDate",
                table: "TaskItems",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(2020, 06, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "TaskItems");
        }
    }
}
