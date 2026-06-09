namespace HexWriter.Web.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<HexWriter.Web.Models.HexWriterContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "HexWriter.Web.Models.HexWriterContext";
        }

        protected override void Seed(HexWriter.Web.Models.HexWriterContext context)
        {
        }
    }
}
