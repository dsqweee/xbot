using Microsoft.EntityFrameworkCore;
using XBOT.DataBase.Models.Invites;
using XBOT.DataBase.Models.Roles_data;
using XBOT.Services.Configuration;

namespace XBOT.DataBase
{
    public class Db : DbContext
    {
        //public Db(DbContextOptions<Db> options) : base(options) { }

        public DbSet<PrivateChannel> PrivateChannel { get; set; }
        public DbSet<TextChannel> TextChannel { get; set; }
        public DbSet<Guild_Logs> Guild_Logs { get; set; }
        public DbSet<Guild_Warn> Guild_Warn { get; set; }
        public DbSet<DiscordInvite> DiscordInvite { get; set; }
        public DbSet<DiscordInvite_ConnectionAudit> ConnectionAudits { get; set; }
        public DbSet<DiscordInvite_ReferralLink> ReferralLinks { get; set; }
        public DbSet<DiscordInvite_ReferralRole> ReferralRole { get; set; }

        public DbSet<GiveAways> GiveAways { get; set; }

        public DbSet<EmojiGift> EmojiGift { get; set; }
        public DbSet<EmojiGift_emojiadded> EmojiGift_emojiadded { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<User_Warn> User_Warn { get; set; }
        public DbSet<User_UnWarn> User_UnWarn { get; set; }
        public DbSet<User_Permission> User_Permission { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Roles_Buy> Roles_Buy { get; set; }
        public DbSet<Roles_Gived> Roles_Gived { get; set; }
        public DbSet<Roles_Level> Roles_Level { get; set; }
        public DbSet<Roles_Reputation> Roles_Reputation { get; set; }
        public DbSet<Roles_User> Roles_User { get; set; }
        public DbSet<TransactionUsers_Logs> TransactionUsers_Logs { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(BotSettings.connectionStringDbPath);
            //optionsBuilder.EnableDetailedErrors(false);
            //optionsBuilder.EnableSensitiveDataLogging(false);
            //optionsBuilder.EnableServiceProviderCaching(false);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Settings>()
                        .HasData(new Settings { Id = 1, Prefix = "x.", Status = $"Prefix: `x.`" });


            //// Meeting_logs
            //modelBuilder.Entity<Settings>().HasOne(s => s.PrivateTextChannel).WithOne().HasForeignKey<Settings>(x => x.PrivateTextChannelId);
            //modelBuilder.Entity<Settings>().HasOne(s => s.LeaveTextChannel).WithOne().HasForeignKey<Settings>(x => x.LeaveTextChannelId);
            //modelBuilder.Entity<Settings>().HasOne(s => s.WelcomeTextChannel).WithOne().HasForeignKey<Settings>(x => x.WelcomeTextChannelId);
            ////

            //modelBuilder.Entity<DiscordInvites>().HasOne(d => d.Author).WithMany(u => u.UserInvites).HasForeignKey(d => d.AuthorId);
            //modelBuilder.Entity<DiscordInvites>().HasMany(u => u.UsersInThisInvite).WithOne(d => d.JoinedInvite).HasForeignKey(d => d.JoinedInviteId);

            //Transaction
            modelBuilder.Entity<TransactionUsers_Logs>()
                    .HasOne(t => t.Sender)
                    .WithMany(u => u.SentTransactions)
                    .HasForeignKey(t => t.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionUsers_Logs>()
                .HasOne(t => t.Recipient)
                .WithMany(u => u.ReceivedTransactions)
                .HasForeignKey(t => t.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<DiscordInvite>()
                    .HasMany(i => i.ConnectionAudits)
                    .WithOne(a => a.Invite)
                    .HasForeignKey(a => a.InviteId)
                    .IsRequired();

            //modelBuilder.Entity<DiscordInvite_ReferralLink>()
            //    .HasOne(r => r.User)
            //    .WithMany()
            //    .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<DiscordInvite_ReferralLink>()
                .HasOne(r => r.Invite)
                .WithMany(i => i.ReferralLinks)
                .HasForeignKey(r => r.InviteId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.MyInvites)
                .WithOne(i => i.Author)
                .HasForeignKey(i => i.AuthorId)
                .IsRequired();




            modelBuilder.Entity<Settings>()
                .HasOne(s => s.PrivateTextChannel)
                .WithMany()
                .HasForeignKey(s => s.PrivateTextChannelId);

            // Определение связи между Settings и TextChannel для LeaveTextChannel
            modelBuilder.Entity<Settings>()
                .HasOne(s => s.LeaveTextChannel)
                .WithMany()
                .HasForeignKey(s => s.LeaveTextChannelId);

            // Определение связи между Settings и TextChannel для WelcomeTextChannel
            modelBuilder.Entity<Settings>()
                .HasOne(s => s.WelcomeTextChannel)
                .WithMany()
                .HasForeignKey(s => s.WelcomeTextChannelId);


            modelBuilder.Entity<User>()
                .HasOne(s => s.User_Permission)
                .WithOne(x=>x.User)
                .HasForeignKey<User_Permission>(s => s.User_Id);

            modelBuilder.Entity<User_UnWarn>()
                .HasOne(s => s.Warn)
                .WithOne(x => x.UnWarn)
                .HasForeignKey<User_Warn>(s => s.UnWarnId);

        }
    }
}
