using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Crystal.Web.Migrations
{
    /// <inheritdoc />
    public partial class StatusTransitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskStatusTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromTaskStatusEntityId = table.Column<int>(type: "integer", nullable: false),
                    ToTaskStatusEntityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskStatusTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskStatusTransitions_TaskStatuses_FromTaskStatusEntityId",
                        column: x => x.FromTaskStatusEntityId,
                        principalTable: "TaskStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskStatusTransitions_TaskStatuses_ToTaskStatusEntityId",
                        column: x => x.ToTaskStatusEntityId,
                        principalTable: "TaskStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TaskStatusTransitions",
                columns: new[] { "Id", "FromTaskStatusEntityId", "ToTaskStatusEntityId" },
                values: new object[,]
                {
                    { 1, 1, 2 },
                    { 2, 2, 3 },
                    { 3, 2, 1 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEDj3mQJ128BGBpQv0V/yN5worSBLEVzuc/n7sxWJn1sUZ3s4TU7mJxoLggmGgUGoPg==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEatSRNBGiltJnJtfi4nFEjbb7OuVo+yVKR1MRVfAAWb0ITPfvOwAmtwLq6E4jUVNQ==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEHio+0m1WsyHSr9FzksHbFpOmrnZrUnNWuK1uVdV8gv7tySRMfDP2ysS+tBxdKqNDw==");

            migrationBuilder.CreateIndex(
                name: "IX_TaskStatusTransitions_FromTaskStatusEntityId_ToTaskStatusEn~",
                table: "TaskStatusTransitions",
                columns: new[] { "FromTaskStatusEntityId", "ToTaskStatusEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskStatusTransitions_ToTaskStatusEntityId",
                table: "TaskStatusTransitions",
                column: "ToTaskStatusEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskStatusTransitions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAECcrk48gZPNMwf12mEN6K7kLJi05zeiwEBY+3oy7PujuWSwJ1yWxzZVXbdkmBnUN/Q==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEGBw+9Sceqkfznp7l/Vg46c0spqgcdTppBWk2tjxMK9MWPy3RMVljYEp3NUeSGQPMQ==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENwhaMTC8W7g3WELvQ4ksQiQsPX+Dn7jUfhDkfHqAbPLl1qxf90vmDNu68kvnl8piQ==");
        }
    }
}
