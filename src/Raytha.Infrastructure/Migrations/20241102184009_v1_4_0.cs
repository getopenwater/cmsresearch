using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raytha.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class v1_4_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ContentTypeFields
                SET FieldType = 'wysiwyg'
                WHERE FieldType = 'long_text';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
