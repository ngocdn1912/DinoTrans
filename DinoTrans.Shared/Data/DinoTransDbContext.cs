using DinoTrans.Shared.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Data
{
    public class DinoTransDbContext: IdentityDbContext <ApplicationUser, ApplicationRole, int>
    {
        public DinoTransDbContext(DbContextOptions<DinoTransDbContext> options) : base(options)
        {

        }
        public DbSet<Company>Companies { get; set; }
        public DbSet<ContructionMachine> ContructionMachines { get; set; }
        public DbSet<Tender> Tenders { get; set; }
        public DbSet<TenderBid> TenderBids { get; set; }
        public DbSet<TenderBidTransportation> TenderBidTransportations { get; set; }
        public DbSet<TenderContructionMachine> TenderContructionMachines { get; set; }
        public DbSet<Transportation> Transportations { get; set; }
        public DbSet<Bill> Bills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TenderBid>(e =>
            {
                e.HasOne(m => m.Tender).WithMany().OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<TenderContructionMachine>(e =>
            {
                e.HasOne(m => m.Tender).WithMany().OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<TenderBidTransportation>(e =>
            {
                e.HasOne(m => m.TenderBid).WithMany().OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
