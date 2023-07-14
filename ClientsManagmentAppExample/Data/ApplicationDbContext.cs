using ClientsManagmentAppExample.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClientsManagmentAppExample.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<FormModel> Forms { get; set; }
        public DbSet<ClientModel> Clients { get; set; }
        public DbSet<ProjectModel> Projects { get; set; }
        public DbSet<CredentialsModel> Credentials { get; set; }
        public DbSet<ClientCredentialsModel> ClientCredentials { get; set; }
        public DbSet<UpdatesModel> Updates { get; set; }
        public DbSet<ClientUpdatesModel> ClientUpdates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
    
}