using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Placeless.MetadataStore.Sql.Migrations
{
    public partial class RemoveFileStreams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contents",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "FileGuid",
                table: "Files");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Contents",
                table: "Files",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FileGuid",
                table: "Files",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
