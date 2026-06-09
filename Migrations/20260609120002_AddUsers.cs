namespace HexWriter.Web.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddUsers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                {
                    Id           = c.Int(nullable: false, identity: true),
                    Username     = c.String(nullable: false, maxLength: 100),
                    Email        = c.String(nullable: false, maxLength: 320),
                    DisplayName  = c.String(nullable: false, maxLength: 200),
                    PasswordHash = c.String(nullable: false, maxLength: 255),
                    Role         = c.String(nullable: false, maxLength: 50),
                    IsActive     = c.Boolean(nullable: false),
                    CreatedAt    = c.DateTime(nullable: false),
                    LastLoginAt  = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Username, unique: true, name: "UQ_Users_Username")
                .Index(t => t.Email, unique: true, name: "UQ_Users_Email");
        }

        public override void Down()
        {
            DropIndex("dbo.Users", "UQ_Users_Email");
            DropIndex("dbo.Users", "UQ_Users_Username");
            DropTable("dbo.Users");
        }
    }
}
