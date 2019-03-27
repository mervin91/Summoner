using System;
using Microsoft.EntityFrameworkCore;
using Summoner.Entities;

namespace Summoner
{
	internal sealed class BotDatabaseContext : DbContext
	{
		private DbSet<LatestUpdate> Updates { get; set; }
		private DbSet<PidorLaunch> PidorLaunches { get; set; }

		private static readonly object Padlock = new object();

		internal BotDatabaseContext()
		{
			lock (Padlock)
			{
				Database.EnsureCreated();
			}
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(ConfigResolver.Instance.DatabaseConnectionString);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			var updates = modelBuilder.Entity<LatestUpdate>();
			updates.HasKey(update => update.Id);
			updates.Property(update => update.LatestUpdateId).IsRequired();

			var pidorLaunches = modelBuilder.Entity<PidorLaunch>();
			pidorLaunches.HasKey(launch => launch.LastLaunchDate);
			pidorLaunches.Property(launch => launch.LastLaunchDate).IsRequired();
		}

		internal void GetLatestUpdateId(out int updateId)
		{
			updateId = Updates.FirstOrDefaultAsync()?.Result?.LatestUpdateId ?? 0;
		}

		internal void SetLatestUpdateId(int updateId)
		{
			var update = Updates.FirstOrDefaultAsync().Result;

			if (update == null)
			{
				Updates.AddAsync(new LatestUpdate { Id = new Guid(), LatestUpdateId = updateId });
			}
			else
			{
				Updates.Update(update);
				update.LatestUpdateId = updateId;
			}

			SaveChangesAsync();
		}

		internal void GetLatestPidorLaunchDate(out DateTime launchDate)
		{
			launchDate = PidorLaunches.FirstOrDefaultAsync().Result?.LastLaunchDate ?? default(DateTime);
		}

		internal void SetLastPidorLaunchDate()
		{
			var lastPidorLaunch = PidorLaunches.FirstOrDefaultAsync().Result;

			if (lastPidorLaunch == null)
			{
				PidorLaunches.AddAsync(new PidorLaunch { LastLaunchDate = DateTime.Now });
			}
			else
			{
				PidorLaunches.Update(lastPidorLaunch);
				lastPidorLaunch.LastLaunchDate = DateTime.Now;
			}

			SaveChangesAsync();
		}
	}
}
