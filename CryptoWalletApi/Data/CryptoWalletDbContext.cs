using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoWalletApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoWalletApi.Data
{
    public class CryptoWalletDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<CryptoBalance> CryptoBalances { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the relationships and constraints here if needed
            base.OnModelCreating(modelBuilder);

            // Transaction
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.SenderWallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.SenderWalletId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ReceiverWallet)
                .WithMany()
                .HasForeignKey(t => t.ReceiverWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // CryptoBalance
            modelBuilder.Entity<CryptoBalance>()
                .HasIndex(cb => new { cb.WalletId, cb.Currency })
                .IsUnique(); // prevents duplicate coin criptocurrency in the same wallet

        }
        public CryptoWalletDbContext(DbContextOptions<CryptoWalletDbContext> options) : base(options)
        {

        }
    }
}