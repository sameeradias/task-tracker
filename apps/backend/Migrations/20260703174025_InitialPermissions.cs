using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaseEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Role_Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Role_Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    RoleId = table.Column<int>(type: "integer", nullable: true),
                    PermissionId = table.Column<int>(type: "integer", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    User_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    UserRole_RoleId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_LastUpdatedBy",
                        column: x => x.LastUpdatedBy,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_RoleId",
                        column: x => x.RoleId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_UserId",
                        column: x => x.UserId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_UserRole_RoleId",
                        column: x => x.UserRole_RoleId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_CreatedBy",
                table: "BaseEntity",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_Email",
                table: "BaseEntity",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_LastUpdatedBy",
                table: "BaseEntity",
                column: "LastUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_Name",
                table: "BaseEntity",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_PermissionId",
                table: "BaseEntity",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_Role_Name",
                table: "BaseEntity",
                column: "Role_Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_RoleId_PermissionId",
                table: "BaseEntity",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_UserId_UserRole_RoleId",
                table: "BaseEntity",
                columns: new[] { "UserId", "UserRole_RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_UserRole_RoleId",
                table: "BaseEntity",
                column: "UserRole_RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaseEntity");
        }
    }
}
