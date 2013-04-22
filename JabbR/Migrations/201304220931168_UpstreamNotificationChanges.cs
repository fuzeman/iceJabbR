namespace JabbR.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpstreamNotificationChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChatMessages", "MessageType", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.ChatMessages", "ImageUrl", c => c.String());
            AddColumn("dbo.ChatMessages", "Source", c => c.String());

            AddColumn("dbo.Notifications", "RoomKey", c => c.Int(nullable: true));

            Sql(@"UPDATE n
SET n.RoomKey = m.Room_Key
FROM dbo.Notifications n
INNER JOIN dbo.ChatMessages m on m.[Key] = n.MessageKey");

            AlterColumn("dbo.Notifications", "RoomKey", c => c.Int(nullable: false));

            AddForeignKey("dbo.Notifications", "RoomKey", "dbo.ChatRooms", "Key", cascadeDelete: true);
            CreateIndex("dbo.Notifications", "RoomKey");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Notifications", new[] { "RoomKey" });
            DropForeignKey("dbo.Notifications", "RoomKey", "dbo.ChatRooms");
            DropColumn("dbo.Notifications", "RoomKey");
            DropColumn("dbo.ChatMessages", "Source");
            DropColumn("dbo.ChatMessages", "ImageUrl");
            DropColumn("dbo.ChatMessages", "MessageType");
        }
    }
}
