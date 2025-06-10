using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HomeFinancial.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bank_files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    imported_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "banks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    bic = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    swift = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    inn = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    kpp = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    registration_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_banks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entry_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entry_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "banks_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    account_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    bank_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_banks_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_banks_accounts_banks_bank_id",
                        column: x => x.bank_id,
                        principalTable: "banks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "statement_entries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fit_id = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    bank_account_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_statement_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_statement_entries_bank_files_file_id",
                        column: x => x.file_id,
                        principalTable: "bank_files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_statement_entries_banks_accounts_bank_account_id",
                        column: x => x.bank_account_id,
                        principalTable: "banks_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_statement_entries_entry_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "entry_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bank_files_file_name",
                table: "bank_files",
                column: "file_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_banks_bank_id",
                table: "banks",
                column: "bank_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_banks_accounts_account_id",
                table: "banks_accounts",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_banks_accounts_bank_id",
                table: "banks_accounts",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_entry_categories_name",
                table: "entry_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_statement_entries_bank_account_id",
                table: "statement_entries",
                column: "bank_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_statement_entries_category_id",
                table: "statement_entries",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_statement_entries_file_id",
                table: "statement_entries",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "ix_statement_entries_fit_id",
                table: "statement_entries",
                column: "fit_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "statement_entries");

            migrationBuilder.DropTable(
                name: "bank_files");

            migrationBuilder.DropTable(
                name: "banks_accounts");

            migrationBuilder.DropTable(
                name: "entry_categories");

            migrationBuilder.DropTable(
                name: "banks");
        }
    }
}
