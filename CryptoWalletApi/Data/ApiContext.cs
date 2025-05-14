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

            // Wallet
            modelBuilder.Entity<Wallet>()
                .HasMany(w => w.SentTransactions)
                .WithOne(t => t.SenderWallet)
                .HasForeignKey(t => t.SenderWalletId)
                .OnDelete(DeleteBehavior.Restrict); // disable cascade delete for Wallet transactions

            modelBuilder.Entity<Wallet>()
                .HasMany(w => w.ReceivedTransactions)
                .WithOne(t => t.ReceiverWallet)
                .HasForeignKey(t => t.ReceiverWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // User
            modelBuilder.Entity<User>()
                .HasMany(u => u.Wallets)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserId);

            // CryptoBalance
            modelBuilder.Entity<CryptoBalance>()
                .HasOne(cb => cb.Wallet)
                .WithMany(w => w.CryptoBalances)
                .HasForeignKey(cb => cb.WalletId);
        }
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {

        }
    }
}