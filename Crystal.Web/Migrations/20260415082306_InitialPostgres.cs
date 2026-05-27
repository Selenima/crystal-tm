using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Crystal.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskFieldDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FieldType = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsBase = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskFieldDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ColorClass = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkQueues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Position = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Department = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    WorkQueueId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_WorkQueues_WorkQueueId",
                        column: x => x.WorkQueueId,
                        principalTable: "WorkQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProjectEntityId = table.Column<int>(type: "integer", nullable: false),
                    WorkQueueId = table.Column<int>(type: "integer", nullable: true),
                    AssignedToId = table.Column<int>(type: "integer", nullable: true),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    TaskStatusEntityId = table.Column<int>(type: "integer", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Projects_ProjectEntityId",
                        column: x => x.ProjectEntityId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_TaskStatuses_TaskStatusEntityId",
                        column: x => x.TaskStatusEntityId,
                        principalTable: "TaskStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_WorkQueues_WorkQueueId",
                        column: x => x.WorkQueueId,
                        principalTable: "WorkQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TaskComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskItemId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskComments_Tasks_TaskItemId",
                        column: x => x.TaskItemId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskFieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskItemId = table.Column<int>(type: "integer", nullable: false),
                    TaskFieldDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskFieldValues_TaskFieldDefinitions_TaskFieldDefinitionId",
                        column: x => x.TaskFieldDefinitionId,
                        principalTable: "TaskFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskFieldValues_Tasks_TaskItemId",
                        column: x => x.TaskItemId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Внутренний проект для сотрудников.", "Internal Portal" },
                    { 2, "Учебная демонстрация для сдачи.", "Demo Release" }
                });

            migrationBuilder.InsertData(
                table: "TaskFieldDefinitions",
                columns: new[] { "Id", "FieldType", "IsBase", "IsRequired", "Name" },
                values: new object[,]
                {
                    { 1, 2, true, true, "Estimate" },
                    { 2, 1, true, false, "Due Note" },
                    { 3, 1, false, false, "Customer" }
                });

            migrationBuilder.InsertData(
                table: "TaskStatuses",
                columns: new[] { "Id", "ColorClass", "Name" },
                values: new object[,]
                {
                    { 1, "secondary", "New" },
                    { 2, "primary", "In Progress" },
                    { 3, "success", "Done" }
                });

            migrationBuilder.InsertData(
                table: "WorkQueues",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Разработчики backend.", "Backend Queue" },
                    { 2, "Команда тестирования.", "QA Queue" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Department", "Email", "FirstName", "IsAdmin", "LastName", "PasswordHash", "Position", "WorkQueueId" },
                values: new object[,]
                {
                    { 1, "Management", "admin@crystal.local", "Admin", true, "Crystal", "AQAAAAIAAYagAAAAECcrk48gZPNMwf12mEN6K7kLJi05zeiwEBY+3oy7PujuWSwJ1yWxzZVXbdkmBnUN/Q==", "Administrator", 1 },
                    { 2, "Development", "anna@crystal.local", "Anna", false, "Smirnova", "AQAAAAIAAYagAAAAEGBw+9Sceqkfznp7l/Vg46c0spqgcdTppBWk2tjxMK9MWPy3RMVljYEp3NUeSGQPMQ==", "Backend Developer", 1 },
                    { 3, "QA", "oleg@crystal.local", "Oleg", false, "Sidorov", "AQAAAAIAAYagAAAAENwhaMTC8W7g3WELvQ4ksQiQsPX+Dn7jUfhDkfHqAbPLl1qxf90vmDNu68kvnl8piQ==", "QA Engineer", 2 }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "AssignedToId", "CreatedById", "Deadline", "Description", "Priority", "ProjectEntityId", "TaskStatusEntityId", "Title", "WorkQueueId" },
                values: new object[,]
                {
                    { 1, 2, 1, new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Создать базовую MVC структуру и подключить PostgreSQL.", 3, 2, 2, "Подготовить структуру проекта", 1 },
                    { 2, 3, 1, new DateTime(2026, 4, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Подготовить краткий список проверок для релиза.", 2, 1, 1, "Составить тест-кейсы", 2 }
                });

            migrationBuilder.InsertData(
                table: "TaskComments",
                columns: new[] { "Id", "Comment", "CreatedAt", "TaskItemId", "UserId" },
                values: new object[,]
                {
                    { 1, "Главное - не усложнять решение.", new DateTime(2026, 4, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1, 1 },
                    { 2, "Начала перенос на PostgreSQL.", new DateTime(2026, 4, 15, 11, 15, 0, 0, DateTimeKind.Utc), 1, 2 }
                });

            migrationBuilder.InsertData(
                table: "TaskFieldValues",
                columns: new[] { "Id", "TaskFieldDefinitionId", "TaskItemId", "Value" },
                values: new object[,]
                {
                    { 1, 1, 1, "8" },
                    { 2, 2, 1, "Нужно закончить до презентации" },
                    { 3, 3, 2, "Учебная группа" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_TaskItemId",
                table: "TaskComments",
                column: "TaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_UserId",
                table: "TaskComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskFieldDefinitions_Name",
                table: "TaskFieldDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskFieldValues_TaskFieldDefinitionId",
                table: "TaskFieldValues",
                column: "TaskFieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskFieldValues_TaskItemId_TaskFieldDefinitionId",
                table: "TaskFieldValues",
                columns: new[] { "TaskItemId", "TaskFieldDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToId",
                table: "Tasks",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedById",
                table: "Tasks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectEntityId",
                table: "Tasks",
                column: "ProjectEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TaskStatusEntityId",
                table: "Tasks",
                column: "TaskStatusEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_WorkQueueId",
                table: "Tasks",
                column: "WorkQueueId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskStatuses_Name",
                table: "TaskStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_WorkQueueId",
                table: "Users",
                column: "WorkQueueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskComments");

            migrationBuilder.DropTable(
                name: "TaskFieldValues");

            migrationBuilder.DropTable(
                name: "TaskFieldDefinitions");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "TaskStatuses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WorkQueues");
        }
    }
}
