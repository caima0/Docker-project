using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <summary>
    /// Removes the incorrect "Password" column from AspNetUsers.
    /// ASP.NET Core Identity uses only PasswordHash; the plain "Password" column was added by mistake and is never set.
    /// </summary>
    public partial class RemovePasswordColumnFromAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Password'
                )
                ALTER TABLE AspNetUsers DROP COLUMN [Password];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Password'
                )
                ALTER TABLE AspNetUsers ADD [Password] nvarchar(max) NOT NULL DEFAULT '';
            ");
        }
    }
}
