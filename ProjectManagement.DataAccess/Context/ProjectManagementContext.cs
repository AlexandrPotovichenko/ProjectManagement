using Microsoft.EntityFrameworkCore;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Reflection;

namespace ProjectManagement.DataAccess.Context
{
    public class ProjectManagementContext : DbContext, IUnitOfWork
    {
        public DbSet<User> Users { get; set; }
        public DbSet<AppFile> AppFiles { get; set; }
        public DbSet<Board> Boards { get; set; } 
        public DbSet<List>Lists { get; set; }
        public DbSet<Card>Cards { get; set; }
        public DbSet<CheckList> CheckLists { get; set; }
        public DbSet<CheckListItem> CheckListItems { get; set; }
        public DbSet<BoardMember>  BoardMembers { get; set; }
        public DbSet<CardMember> CardMembers { get; set; }
      
        public ProjectManagementContext(DbContextOptions<ProjectManagementContext> options) : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.Entity<Board>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasMany(x => x.Lists).WithOne(x=>x.Board);
                b.HasMany(x => x.BoardMembers).WithOne(x=>x.Board);
                b.Property(x => x.Name);
            });
            modelBuilder.Entity<List>(l =>
            {
                l.HasKey(x => x.Id);
                l.HasMany(x => x.Cards).WithOne(x => x.List);
                l.Property(x => x.Name);
            });
            modelBuilder.Entity<Card>(c =>
            {
                c.HasKey(x => x.Id);
                c.HasMany(x => x.CheckLists).WithOne(x => x.Card);
                c.HasMany(x => x.CardMembers).WithOne(x => x.Card);
                c.HasMany(x => x.Actions).WithOne(x => x.Card);
                c.Property(x => x.Name);
            });
            modelBuilder.Entity<CheckList>(cl =>
            {
                cl.HasKey(x => x.Id);
                cl.HasMany(x => x.ChecklistItems).WithOne(x => x.CheckList);
                cl.Property(x => x.Name);
            });
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().HasOne(u => u.Avatar);
            modelBuilder.Entity<User>().HasData(new User { Id = 1, Name = "admin",
                PasswordHash=$"$2a$11$Qy0GEfdSnqxaWN7A6DOlDeid0Yvo7Yzm.etE5P13Pc78MRv7ulY7u",CanAdministerUsers=true });// Hash of Password 'administrator'
            modelBuilder.Entity<CardAction>(ca =>
            {
                ca.HasKey(x => x.Id);
                ca.Property(t => t.Date).HasColumnType("DateTime");
            });
            modelBuilder.Entity<AppFile>(af=>
            { 
                af.HasKey(x => x.Id);
                af.Property(x => x.Content);
            });
        }
    }
}
