using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using NoHesi.Bot.FileObjects;
using Org.BouncyCastle.Math.EC;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace NoHesi.databasemodels;

public partial class NoHesiBotContext : DbContext
{
    public NoHesiBotContext()
    {
    }

    public NoHesiBotContext(DbContextOptions<NoHesiBotContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SteamLink> SteamLinks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql($"server={Config.Default.DbServer};database={Config.Default.DbName};user={Config.Default.DbUser};password={Config.Default.DbPassword}", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.6.16-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<SteamLink>(entity =>
        {
            entity.HasKey(e => e.DiscordId).HasName("PRIMARY");

            entity.ToTable("steam_links");

            entity.Property(e => e.DiscordId)
                .HasMaxLength(45)
                .HasColumnName("discord_id");
            entity.Property(e => e.SteamId)
                .HasMaxLength(45)
                .HasColumnName("steam_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
