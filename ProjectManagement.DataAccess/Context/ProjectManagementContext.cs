using Microsoft.EntityFrameworkCore;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Reflection;

namespace ProjectManagement.DataAccess.Context
{
    public class ProjectManagementContext : DbContext, IUnitOfWork
    {
        public DbSet<User> Users { get; set; }
        public DbSet<CheckListItem> CheckListItems { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Card>Cards { get; set; }
        public DbSet<List>Lists { get; set; }

        public ProjectManagementContext(DbContextOptions<ProjectManagementContext> options) : base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.Entity<Board>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasMany(x => x.Lists);
                b.HasMany(x => x.BoardMembers);
                b.Property(x => x.Name);
            });

            //modelBuilder.Entity<CheckListItem>(b =>
            //{
            //    b.HasKey(x => x.Id);
            //    //b.HasOne(x => x.User)
            //    //    .WithMany(u => u.ToDoItems)
            //    //    .HasForeignKey(x => x.UserId);
            //    b.Property(x => x.Name)
            //    b.Property(x => x.IsDone)
            //        .HasMaxLength(100);
            //});

            //modelBuilder.Entity<User>(b =>
            //{
            //    b.HasKey(x => x.Id);
            //});
            modelBuilder.Entity<User>().HasKey(t => t.Id);
            modelBuilder.Entity<User>().HasData(
            
                new User {Id=1,  Name= "John Doe" }
            );
        }
    }
}
