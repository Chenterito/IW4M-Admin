﻿using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLibraryCore.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<EFClient> Clients { get; set; }
        public DbSet<EFAlias> Aliases { get; set; }
        public DbSet<EFAliasLink> AliasLinks { get; set; }
        public DbSet<EFPenalty> Penalties { get; set; }
        public DbSet<EFMeta> EFMeta { get; set; }
        public DbSet<EFChangeHistory> EFChangeHistory { get; set; }

        static string _ConnectionString;
        static string _provider;
        private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole()
            .AddDebug()
            .AddFilter((category, level) => true);
        });

        public DatabaseContext(DbContextOptions<DatabaseContext> opt) : base(opt)
        {
        }

        public DatabaseContext()
        {
        }

        public override void Dispose()
        {
        }

        public DatabaseContext(bool disableTracking) : this()
        {
            if (disableTracking)
            {
                this.ChangeTracker.AutoDetectChangesEnabled = false;
                this.ChangeTracker.LazyLoadingEnabled = false;
                this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }

            else
            {
                this.ChangeTracker.AutoDetectChangesEnabled = true;
                this.ChangeTracker.LazyLoadingEnabled = true;
                this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
        }

        public DatabaseContext(string connStr, string provider) : this()
        {
            _ConnectionString = connStr;
            _provider = provider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseLoggerFactory(_loggerFactory)
            //  .EnableSensitiveDataLogging();

            if (string.IsNullOrEmpty(_ConnectionString))
            {
                string currentPath = Utilities.OperatingDirectory;
                // allows the application to find the database file
                currentPath = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                    $"{Path.DirectorySeparatorChar}{currentPath}" :
                    currentPath;

                var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = Path.Join(currentPath, "Database", "Database.db") };
                var connectionString = connectionStringBuilder.ToString();
                var connection = new SqliteConnection(connectionString);

                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseSqlite(connection);
                }
            }

            else
            {
                switch (_provider)
                {
                    default:
                    case "mysql":
                        optionsBuilder.UseMySql(_ConnectionString, _options => _options.EnableRetryOnFailure());
                        break;
                    case "postgresql":
                        optionsBuilder.UseNpgsql(_ConnectionString, _options => _options.EnableRetryOnFailure());
                        break;
                }
            }
        }

        private void SetAuditColumns()
        {
            return;
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is SharedEntity && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified)).ToList();

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    //((SharedEntity)entityEntry.Entity).CreatedDateTime = DateTime.UtcNow;
                }

                else
                {
                    //((SharedEntity)entityEntry.Entity).UpdatedDateTime = DateTime.UtcNow;
                }
            }
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetAuditColumns();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges()
        {
            SetAuditColumns();
            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // make network id unique
            modelBuilder.Entity<EFClient>(entity =>
            {
                entity.HasIndex(e => e.NetworkId).IsUnique();
            });

            modelBuilder.Entity<EFPenalty>(entity =>
            {
                entity.HasOne(p => p.Offender)
                .WithMany(c => c.ReceivedPenalties)
                .HasForeignKey(c => c.OffenderId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Punisher)
                .WithMany(p => p.AdministeredPenalties)
                .HasForeignKey(c => c.PunisherId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.Property(p => p.Expires)
                    .IsRequired(false);
            });

            modelBuilder.Entity<EFAliasLink>(entity =>
            {
                entity.HasMany(e => e.Children)
                .WithOne(a => a.Link)
                .HasForeignKey(k => k.LinkId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EFAlias>(ent =>
            {
                ent.Property(a => a.IPAddress).IsRequired(false);
                ent.HasIndex(a => a.IPAddress);
                ent.Property(a => a.Name).HasMaxLength(24);
                ent.HasIndex(a => a.Name);
                ent.Property(_alias => _alias.SearchableName).HasMaxLength(24);
                ent.HasIndex(_alias => _alias.SearchableName);
                ent.HasIndex(_alias => new { _alias.Name, _alias.IPAddress }).IsUnique();
            });

            modelBuilder.Entity<EFMeta>(ent =>
            {
                ent.HasIndex(_meta => _meta.Key);
            });

            // force full name for database conversion
            modelBuilder.Entity<EFClient>().ToTable("EFClients");
            modelBuilder.Entity<EFAlias>().ToTable("EFAlias");
            modelBuilder.Entity<EFAliasLink>().ToTable("EFAliasLinks");
            modelBuilder.Entity<EFPenalty>().ToTable("EFPenalties");

            // adapted from
            // https://aleemkhan.wordpress.com/2013/02/28/dynamically-adding-dbset-properties-in-dbcontext-for-entity-framework-code-first/

            string pluginDir = Path.Join(Utilities.OperatingDirectory, "Plugins");

            if (Utilities.IsDevelopment)
            {
                pluginDir = Path.Join(Utilities.OperatingDirectory, "..", "..", "..", "..", "BUILD", "Plugins");
            }

            IEnumerable<string> directoryFiles = Directory.GetFiles(pluginDir).Where(f => f.EndsWith(".dll"));

            foreach (string dllPath in directoryFiles)
            {
                Assembly library;
                try
                {
                    library = Assembly.LoadFrom(dllPath);
                }

                // not a valid assembly, ie plugin support files
                catch (Exception)
                {
                    continue;
                }

                var configurations = library.ExportedTypes.Where(c => c.GetInterfaces().FirstOrDefault(i => typeof(IModelConfiguration).IsAssignableFrom(i)) != null)
                    .Select(c => (IModelConfiguration)Activator.CreateInstance(c));

                foreach (var configurable in configurations)
                {
                    configurable.Configure(modelBuilder);
                }

                foreach (var type in library.ExportedTypes)
                {
                    if (type.IsClass && type.IsSubclassOf(typeof(SharedEntity)))
                    {
                        var method = modelBuilder.GetType().GetMethod("Entity", new[] { typeof(Type) });
                        method.Invoke(modelBuilder, new[] { type });
                    }
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
