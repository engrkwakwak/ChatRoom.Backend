using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Configuration;

namespace Repository {
    public class RepositoryContext : DbContext {
        public RepositoryContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Status)
                .WithMany(s => s.Chats)
                .HasForeignKey(c => c.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ChatMember>()
                .HasOne(cm => cm.Status)
                .WithMany(s => s.ChatMembers)
                .HasForeignKey(cm => cm.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ChatMember>()
                .HasOne(cm => cm.Message)
                .WithMany(m => m.LastSeenUsers)
                .HasForeignKey(cm => cm.LastSeenMessageId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.User)
                .WithMany(u => u.Contacts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.UserContact)
                .WithMany()
                .HasForeignKey(c => c.ContactId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Status)
                .WithMany(s => s.Contacts)
                .HasForeignKey(c => c.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Contact>()
                .HasKey(x => new { x.UserId, x.ContactId });
            modelBuilder.Entity<ChatMember>()
                .HasKey(x => new { x.ChatId, x.UserId });
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Status)
                .WithMany(s => s.Messages)
                .HasForeignKey(m => m.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Username)
                .IsUnique();

            modelBuilder.ApplyConfiguration(new StatusConfiguration());
            modelBuilder.ApplyConfiguration(new ChatTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MessageTypeConfiguration());
            //modelBuilder.ApplyConfiguration(new TestUsersConfiguration());
        }

        public DbSet<Chat>? Chats { get; set; }
        public DbSet<ChatMember>? ChatMembers { get; set; }
        public DbSet<ChatType>? ChatTypes { get; set; }
        public DbSet<Contact>? Contacts { get; set; }
        public DbSet<Message>? Messages { get; set; }
        public DbSet<MessageType>? MessageTypes { get; set; }
        public DbSet<Status>? Status { get; set; }
        public DbSet<User>? Users { get; set; }
    }
}
