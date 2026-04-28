using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenlyAPI.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Complaint_Trail_Prop_Action_Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActionType",
                table: "ComplaintsTrail",
                type: "int",
                nullable: false,
                defaultValue: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "ComplaintsTrail");
        }
    }
}
