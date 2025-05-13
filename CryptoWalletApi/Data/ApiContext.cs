using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoWalletApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoWalletApi.Data
{
    public class ApiContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<CryptoBalance> CryptoBalances { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the relationships and constraints here if needed
            base.OnModelCreating(modelBuilder);
        }
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {

        }
    }
}