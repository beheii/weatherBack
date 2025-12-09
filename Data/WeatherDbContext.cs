using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using weatherapp.Models;

namespace weatherapp.Data;

public partial class WeatherDbContext : DbContext
{
    public WeatherDbContext()
    {
    }

    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Sun> Suns { get; set; }

    public virtual DbSet<WeatherCondition> WeatherConditions { get; set; }

    public virtual DbSet<WeatherCurrent> WeatherCurrents { get; set; }

    public virtual DbSet<Wind> Winds { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=Weather;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.CityId).HasName("PK__City__B4BEB95E196182F2");

            entity.ToTable("City");

            entity.Property(e => e.CityId).HasColumnName("cityId");
            entity.Property(e => e.Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Timezone).HasColumnName("timezone");
        });

        modelBuilder.Entity<Sun>(entity =>
        {
            entity.HasKey(e => e.SunId).HasName("PK__Sun__B593B04E97859F4D");

            entity.ToTable("Sun");

            entity.Property(e => e.SunId).HasColumnName("sunId");
            entity.Property(e => e.Sunrise).HasColumnName("sunrise");
            entity.Property(e => e.Sunset).HasColumnName("sunset");
            entity.Property(e => e.WeatherCurrentId).HasColumnName("weatherCurrentId");

            entity.HasOne(d => d.WeatherCurrent).WithMany(p => p.Suns)
                .HasForeignKey(d => d.WeatherCurrentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sun__weatherCurr__73BA3083");
        });

        modelBuilder.Entity<WeatherCondition>(entity =>
        {
            entity.HasKey(e => e.WeatherConditionId).HasName("PK__WeatherC__79D49466365D9545");

            entity.ToTable("WeatherCondition");

            entity.Property(e => e.WeatherConditionId).HasColumnName("weatherConditionId");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.MainName)
                .HasMaxLength(50)
                .IsFixedLength()
                .HasColumnName("mainName");
        });

        modelBuilder.Entity<WeatherCurrent>(entity =>
        {
            entity.HasKey(e => e.WeatherCurrentId).HasName("PK__WeatherC__198D9F0569D8DCE1");

            entity.ToTable("WeatherCurrent");

            entity.Property(e => e.WeatherCurrentId).HasColumnName("weatherCurrentId");
            entity.Property(e => e.CityId).HasColumnName("cityId");
            entity.Property(e => e.Cloudiness).HasColumnName("cloudiness");
            entity.Property(e => e.FeelsLike).HasColumnName("feelsLike");
            entity.Property(e => e.Humidity).HasColumnName("humidity");
            entity.Property(e => e.Pressure).HasColumnName("pressure");
            entity.Property(e => e.Temperature).HasColumnName("temperature");
            entity.Property(e => e.TemperatureMax).HasColumnName("temperatureMax");
            entity.Property(e => e.TemperatureMin).HasColumnName("temperatureMin");
            entity.Property(e => e.TimeStamp).HasColumnName("timeStamp");
            entity.Property(e => e.Visibility).HasColumnName("visibility");
            entity.Property(e => e.WeatherConditionId).HasColumnName("weatherConditionId");

            entity.HasOne(d => d.City).WithMany(p => p.WeatherCurrents)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WeatherCu__cityI__6D0D32F4");

            entity.HasOne(d => d.WeatherCondition).WithMany(p => p.WeatherCurrents)
                .HasForeignKey(d => d.WeatherConditionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WeatherCu__weath__6E01572D");
        });

        modelBuilder.Entity<Wind>(entity =>
        {
            entity.HasKey(e => e.WindId).HasName("PK__Wind__E232D2EC77AEBE36");

            entity.ToTable("Wind");

            entity.Property(e => e.WindId).HasColumnName("windId");
            entity.Property(e => e.Deg).HasColumnName("deg");
            entity.Property(e => e.Gust).HasColumnName("gust");
            entity.Property(e => e.Speed).HasColumnName("speed");
            entity.Property(e => e.WeatherCurrentId).HasColumnName("weatherCurrentId");

            entity.HasOne(d => d.WeatherCurrent).WithMany(p => p.Winds)
                .HasForeignKey(d => d.WeatherCurrentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Wind__weatherCur__70DDC3D8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
