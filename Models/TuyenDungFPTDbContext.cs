using Microsoft.EntityFrameworkCore;

namespace TuyenDungFPT.Models
{
	public class TuyenDungFPTDbContext : DbContext
	{
		public TuyenDungFPTDbContext(DbContextOptions<TuyenDungFPTDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Company> Companies { get; set; }
		public DbSet<Job> Jobs { get; set; }
		public DbSet<JobCategory> JobCategories { get; set; }
		public DbSet<Application> Applications { get; set; }
		public DbSet<Resume> Resumes { get; set; }
		public DbSet<SavedJob> SavedJobs { get; set; }
		public DbSet<ChuDe> ChuDe { get; set; }
		public DbSet<BaiViet> BaiViet { get; set; }
		public DbSet<BinhLuanBaiViet> BinhLuanBaiViet { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Application>()
				.HasOne(a => a.User)
				.WithMany(u => u.Applications)
				.HasForeignKey(a => a.UserId)
				.OnDelete(DeleteBehavior.NoAction);  // Không cho phép cascade delete

			modelBuilder.Entity<Application>()
				.HasOne(a => a.Job)
				.WithMany(j => j.Applications)
				.HasForeignKey(a => a.JobId)
				.OnDelete(DeleteBehavior.Cascade);  // Không cho phép cascade delete

			modelBuilder.Entity<Application>()
				.HasOne(a => a.Resume)
				.WithMany()
				.HasForeignKey(a => a.ResumeId)
				.OnDelete(DeleteBehavior.NoAction);  // Không cho phép cascade delete

			modelBuilder.Entity<SavedJob>()
				.HasKey(sj => new { sj.UserId, sj.JobId });

			modelBuilder.Entity<SavedJob>()
				.HasOne(sj => sj.User)
				.WithMany(u => u.SavedJobs)
				.HasForeignKey(sj => sj.UserId)
				.OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<SavedJob>()
				.HasOne(sj => sj.Job)
				.WithMany()
				.HasForeignKey(sj => sj.JobId)
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<ChuDe>().ToTable("ChuDe");
			modelBuilder.Entity<BaiViet>().ToTable("BaiViet");
			modelBuilder.Entity<BinhLuanBaiViet>().ToTable("BinhLuanBaiViet");
		}

	}
}
