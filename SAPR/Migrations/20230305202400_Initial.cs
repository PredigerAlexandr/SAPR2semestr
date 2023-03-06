using Microsoft.EntityFrameworkCore.Migrations;

namespace SAPR.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExecutableСodes",
                columns: table => new
                {
                    ExecutableСodeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutableСodes", x => x.ExecutableСodeId);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    RuleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseId = table.Column<long>(type: "bigint", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleText = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.RuleId);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    PurchaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeforeRuleRuleId = table.Column<long>(type: "bigint", nullable: true),
                    AfterRuleRuleId = table.Column<long>(type: "bigint", nullable: true),
                    GeneratedCodeExecutableСodeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.PurchaseId);
                    table.ForeignKey(
                        name: "FK_Purchases_ExecutableСodes_GeneratedCodeExecutableСodeId",
                        column: x => x.GeneratedCodeExecutableСodeId,
                        principalTable: "ExecutableСodes",
                        principalColumn: "ExecutableСodeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Purchases_Rules_AfterRuleRuleId",
                        column: x => x.AfterRuleRuleId,
                        principalTable: "Rules",
                        principalColumn: "RuleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Purchases_Rules_BeforeRuleRuleId",
                        column: x => x.BeforeRuleRuleId,
                        principalTable: "Rules",
                        principalColumn: "RuleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    FieldId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Alias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchaseId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.FieldId);
                    table.ForeignKey(
                        name: "FK_Fields_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "PurchaseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fields_PurchaseId",
                table: "Fields",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_AfterRuleRuleId",
                table: "Purchases",
                column: "AfterRuleRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_BeforeRuleRuleId",
                table: "Purchases",
                column: "BeforeRuleRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_GeneratedCodeExecutableСodeId",
                table: "Purchases",
                column: "GeneratedCodeExecutableСodeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fields");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "ExecutableСodes");

            migrationBuilder.DropTable(
                name: "Rules");
        }
    }
}
