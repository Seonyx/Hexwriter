namespace HexWriter.Web.Migrations
{
    using System.Data.Entity.Migrations;

    // Baseline migration — existing tables are managed via Database/hexwriter-schema.sql.
    // This empty migration lets EF track subsequent migrations without attempting
    // to recreate the pre-existing schema.
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
        }

        public override void Down()
        {
        }
    }
}
