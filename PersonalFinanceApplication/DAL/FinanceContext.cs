using PersonalFinanceApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.DAL
{
    public class PersonalFinanceApplicationContext : DbContext
    {
        public PersonalFinanceApplicationContext() : base("PersonalFinanceApplicationContext")
        {

        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountType> AccountTypes { get; set; }
        public virtual DbSet<Budget> Budgets { get; set; }
        public virtual DbSet<BudgetPeriod> BudgetPeriods { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CategoryType> CategoryTypes { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<Vendor> Vendors { get; set; }
        public virtual DbSet<VendorAbbrev> VendorAbbrevs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Budget>()
                .HasMany(e => e.Categories)
                .WithMany(e => e.Budgets)
                .Map(m => m.ToTable("BudgetCategory").MapLeftKey("BudgetID").MapRightKey("CategoryID"));

            modelBuilder.Entity<Category>()
                .HasMany(e => e.Vendors)
                .WithMany(e => e.Categories)
                .Map(m => m.ToTable("VendorCategory").MapLeftKey("CategoryID").MapRightKey("VendorID"));
        }
    }
}