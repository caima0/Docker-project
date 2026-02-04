using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRateResponseItemRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rates_ResponseItem_ResponseItemId",
                table: "Rates");

            migrationBuilder.AddForeignKey(
                name: "FK_Rates_ResponseItem_ResponseItemId",
                table: "Rates",
                column: "ResponseItemId",
                principalTable: "ResponseItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rates_ResponseItem_ResponseItemId",
                table: "Rates");

            migrationBuilder.AddForeignKey(
                name: "FK_Rates_ResponseItem_ResponseItemId",
                table: "Rates",
                column: "ResponseItemId",
                principalTable: "ResponseItem",
                principalColumn: "Id");
        }
    }
}
