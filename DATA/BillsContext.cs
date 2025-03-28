﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DATA
{
    public class BillsContext : DbContext
    {
        private string ConnectionString = string.Empty;


        public BillsContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DbSet<Rabbat> Rabbat { get; set; }
        public DbSet<Adresse> Adressen { get; set; }
        public DbSet<Kunde> Kunden { get; set; }
        public DbSet<Benutzer> Benutzer { get; set; }
        public DbSet<Rechnung> Rechnungen { get; set; }
        public DbSet<Rechnungsposition> Rechnungsposition { get; set; }
        public DbSet<Angebot> Angebote { get; set; }
        public DbSet<Angebotsposition> Angebotsposition { get; set; }
        public DbSet<EmailConf> EmailConf { get; set; }
        public DbSet<ZusatzText> ZusatzTexte { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(ConnectionString); //"server=localhost;database=Bills;user=root;password=Admin"
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EmailConf>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.EmailServer).IsRequired();
                entity.Property(e => e.EmailBetriff).IsRequired();
                entity.Property(e => e.EmailInhalt).IsRequired();
                entity.Property(e => e.UserName).IsRequired();
                entity.Property(e => e.password).IsRequired();
                entity.Property(e => e.Port).IsRequired();
            });

            modelBuilder.Entity<Adresse>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Ort).IsRequired();
                entity.Property(e => e.Strasse).IsRequired();
                entity.Property(e => e.PLZ).IsRequired();
                entity.Property(e => e.HasuNr).IsRequired();

            });

            modelBuilder.Entity<Rabbat>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.satz).IsRequired();
                entity.Property(e => e.Beschreibung).IsRequired();
            });

            modelBuilder.Entity<Kunde>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.FirmaName).IsRequired();

                entity.HasOne(d => d.addresse)
                  .WithMany(p => p.Kunden);
            });

            modelBuilder.Entity<Rechnung>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Datum).IsRequired();
                entity.Property(e => e.Nr).IsRequired();
                entity.Property(e => e.Umsatzsteuer).IsRequired();

                entity.HasOne(d => d.Kunde)
                  .WithMany(p => p.Rechnungen);

                entity.HasOne(d => d.Rabbat);
                entity.HasOne(d => d.ZusatzText);
            });

            modelBuilder.Entity<Angebot>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Datum).IsRequired();
                entity.Property(e => e.Nr).IsRequired();
                entity.Property(e => e.Umsatzsteuer).IsRequired();

                entity.HasOne(d => d.Kunde)
                  .WithMany(p => p.Angebote);
            });

            modelBuilder.Entity<Rechnungsposition>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Einzeln_Preis).IsRequired();
                entity.Property(e => e.Menge).IsRequired();
                entity.Property(e => e.Beschreibung).IsRequired();

                entity.HasOne(d => d.Rechnung)
                  .WithMany(p => p.Positions);
            });

            modelBuilder.Entity<Angebotsposition>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Einzeln_Preis).IsRequired();
                entity.Property(e => e.Menge).IsRequired();

                entity.HasOne(d => d.Angebot)
                  .WithMany(p => p.Positions);
            });

            modelBuilder.Entity<Benutzer>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.BankName).IsRequired();
                entity.Property(e => e.BIC).IsRequired();
                entity.Property(e => e.IBAN).IsRequired();
                entity.Property(e => e.SteuerID).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Vorname).IsRequired();
                entity.Property(e => e.FirmaName).IsRequired();
                entity.HasOne(d => d.addresse);
            });

            modelBuilder.Entity<ZusatzText>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Beschreibung).IsRequired();
            });
        }

        public void RollBack()
        {
            var changedEntries = this.ChangeTracker.Entries()
                .Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }
    }
    
}
