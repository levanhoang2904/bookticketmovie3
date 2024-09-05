﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookTicketMovie.Migrations
{
    /// <inheritdoc />
    public partial class add_photo1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Photo",
                table: "Movie",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Movie");
        }
    }
}
