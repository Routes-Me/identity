using Microsoft.EntityFrameworkCore;

namespace IdentitiesService.Models.DBModels
{
    public partial class IdentitiesServiceContext : DbContext
    {
        public IdentitiesServiceContext()
        {
        }

        public IdentitiesServiceContext(DbContextOptions<IdentitiesServiceContext> options)
            : base(options)
        {
        }

        public virtual DbSet<RevokedRefreshTokens> RevokedRefreshTokens { get; set; }
        public virtual DbSet<RevokedAccessTokens> RevokedAccessTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RevokedRefreshTokens>(entity =>
            {
                entity.HasKey(e => e.RevokedRefreshTokenId).HasName("PRIMARY");

                entity.ToTable("revoked_refresh_tokens");

                entity.Property(e => e.RevokedRefreshTokenId).HasColumnName("revoked_refresh_token_id");

                entity.Property(e => e.IdentityId).HasColumnName("identity_id");

                entity.Property(e => e.RefreshTokenReference)
                    .HasColumnName("refresh_token_reference")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ExpiryAt)
                    .HasColumnName("expiry_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.RevokedAt)
                    .HasColumnName("revoked_at")
                    .HasColumnType("timestamp");
            });

            modelBuilder.Entity<RevokedAccessTokens>(entity =>
            {
                entity.HasKey(e => e.RevokedAccessTokenId).HasName("PRIMARY");

                entity.ToTable("revoked_access_tokens");

                entity.Property(e => e.RevokedAccessTokenId).HasColumnName("revoked_access_token_id");

                entity.Property(e => e.IdentityId).HasColumnName("identity_id");

                entity.Property(e => e.AccessTokenReference)
                    .HasColumnName("access_token_reference")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ExpiryAt)
                    .HasColumnName("expiry_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.RevokedAt)
                    .HasColumnName("revoked_at")
                    .HasColumnType("timestamp");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
