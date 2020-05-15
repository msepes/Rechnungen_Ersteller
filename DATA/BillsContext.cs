using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DATA
{
    public class BillsContext : DbContext
    {
        public DbSet<Rabbat> Rabbat { get; set; }
        public DbSet<Adresse> Adressen { get; set; }
        public DbSet<Kunde> Kunden { get; set; }
        public DbSet<Benutzer> Benutzer { get; set; }
        public DbSet<Rechnung> Rechnungen { get; set; }
        public DbSet<Rechnungsposition> Rechnungsposition { get; set; }
        public DbSet<Angebot> Angebote { get; set; }
        public DbSet<Angebotsposition> Angebotsposition { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=localhost;database=Bills;user=root;password=Admin");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }

    }
}
