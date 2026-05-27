using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Crystal.Web.Migrations
{
    /// <inheritdoc />
    public partial class UserProjectAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProjectAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ProjectEntityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjectAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProjectAccesses_Projects_ProjectEntityId",
                        column: x => x.ProjectEntityId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProjectAccesses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "UserProjectAccesses" ("UserId", "ProjectEntityId")
                SELECT u."Id", p."Id"
                FROM "Users" u
                CROSS JOIN "Projects" p
                WHERE
                    (u."Email" = 'admin@crystal.local' AND p."Name" = 'Internal Portal') OR
                    (u."Email" = 'admin@crystal.local' AND p."Name" = 'Demo Release') OR
                    (u."Email" = 'anna@crystal.local' AND p."Name" = 'Demo Release') OR
                    (u."Email" = 'oleg@crystal.local' AND p."Name" = 'Internal Portal');
                """);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEBqEySPIpl73S1dCWxEGI9z9GT1cqpV+AzHXo33cbBCTgWymBd81t3nJppvXb6ZLtQ==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEO2AUy6XbPUBFCBNK31joyTJjtIeTMtBa/i55OlTfLT8JXfuMqRz+Egw1VWcQm/kew==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEFv611jAShbUJebBJZ8RCpxyUxDnAAsCsSkryM+XNePK+Z2bdQ6lIdNncuzLjSih3A==");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectAccesses_ProjectEntityId",
                table: "UserProjectAccesses",
                column: "ProjectEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectAccesses_UserId_ProjectEntityId",
                table: "UserProjectAccesses",
                columns: new[] { "UserId", "ProjectEntityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProjectAccesses");

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
        }
    }
}
