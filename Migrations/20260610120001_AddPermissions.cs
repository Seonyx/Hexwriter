namespace HexWriter.Web.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddPermissions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Groups",
                c => new
                {
                    Id          = c.Int(nullable: false, identity: true),
                    Name        = c.String(nullable: false, maxLength: 200),
                    Description = c.String(maxLength: 500),
                    CreatedAt   = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "UQ_Groups_Name");

            CreateTable(
                "dbo.GroupUsers",
                c => new
                {
                    Id      = c.Int(nullable: false, identity: true),
                    GroupId = c.Int(nullable: false),
                    UserId  = c.Int(nullable: false),
                    AddedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.GroupId)
                .ForeignKey("dbo.Users",  t => t.UserId)
                .Index(t => new { t.GroupId, t.UserId }, unique: true, name: "UQ_GroupUsers_GroupUser");

            CreateTable(
                "dbo.BookGroups",
                c => new
                {
                    Id            = c.Int(nullable: false, identity: true),
                    BookProjectID = c.Int(nullable: false),
                    GroupId       = c.Int(nullable: false),
                    AccessLevel   = c.String(nullable: false, maxLength: 20),
                    GrantedAt     = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BookProjects", t => t.BookProjectID)
                .ForeignKey("dbo.Groups",       t => t.GroupId)
                .Index(t => new { t.BookProjectID, t.GroupId }, unique: true, name: "UQ_BookGroups_BookGroup");

            CreateTable(
                "dbo.BookUsers",
                c => new
                {
                    Id            = c.Int(nullable: false, identity: true),
                    BookProjectID = c.Int(nullable: false),
                    UserId        = c.Int(nullable: false),
                    AccessLevel   = c.String(nullable: false, maxLength: 20),
                    GrantedAt     = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BookProjects", t => t.BookProjectID)
                .ForeignKey("dbo.Users",        t => t.UserId)
                .Index(t => new { t.BookProjectID, t.UserId }, unique: true, name: "UQ_BookUsers_BookUser");
        }

        public override void Down()
        {
            DropTable("dbo.BookUsers");
            DropTable("dbo.BookGroups");
            DropTable("dbo.GroupUsers");
            DropTable("dbo.Groups");
        }
    }
}
