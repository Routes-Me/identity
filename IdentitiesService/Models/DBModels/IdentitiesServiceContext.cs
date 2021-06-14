using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

        public virtual DbSet<Applications> Applications { get; set; }
        public virtual DbSet<PhoneIdentities> PhoneIdentities { get; set; }
        public virtual DbSet<EmailIdentities> EmailIdentities { get; set; }
        public virtual DbSet<Privileges> Privileges { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Identities> Identities { get; set; }
        public virtual DbSet<IdentitiesRoles> IdentitiesRoles { get; set; }
        public virtual DbSet<RevokedRefreshTokens> RevokedRefreshTokens { get; set; }
        public virtual DbSet<RevokedAccessTokens> RevokedAccessTokens { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Applications>(entity =>
            {
                entity.HasKey(e => e.ApplicationId)
                    .HasName("PRIMARY");

                entity.ToTable("applications");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<PhoneIdentities>(entity =>
            {
                entity.HasKey(e => e.PhoneIdentityId).HasName("PRIMARY");

                entity.ToTable("phone_identities");

                entity.Property(e => e.PhoneIdentityId).HasColumnName("phone_identity_id");

                entity.Property(e => e.IdentityId).HasColumnName("identity_id");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnName("phone_number")
                    .HasColumnType("varchar(20)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.PhoneIdentities)
                    .HasForeignKey(d => d.IdentityId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("phone_identities_ibfk_1");
            });

            modelBuilder.Entity<EmailIdentities>(entity =>
            {
                entity.HasKey(e => e.EmailIdentityId).HasName("PRIMARY");

                entity.ToTable("email_identities");

                entity.Property(e => e.EmailIdentityId).HasColumnName("email_identity_id");

                entity.Property(e => e.IdentityId).HasColumnName("identity_id");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasColumnType("varchar(40)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasColumnType("char(64)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.HasOne(d => d.Identity)
                    .WithOne(p => p.EmailIdentity)
                    .HasForeignKey<EmailIdentities>(d => d.IdentityId)
                    .HasConstraintName("email_identities_ibfk_1");
            });

            modelBuilder.Entity<Privileges>(entity =>
            {
                entity.HasKey(e => e.PrivilegeId)
                    .HasName("PRIMARY");

                entity.ToTable("privileges");

                entity.Property(e => e.PrivilegeId).HasColumnName("privilege_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.HasKey(e => new { e.ApplicationId, e.PrivilegeId })
                    .HasName("PRIMARY");

                entity.ToTable("roles");

                entity.HasIndex(e => e.PrivilegeId)
                    .HasName("privilege_id");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.PrivilegeId).HasColumnName("privilege_id");

                entity.HasOne(d => d.Application)
                    .WithMany(p => p.Roles)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("roles_ibfk_2");

                entity.HasOne(d => d.Privilege)
                    .WithMany(p => p.Roles)
                    .HasForeignKey(d => d.PrivilegeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("roles_ibfk_1");
            });

            modelBuilder.Entity<Identities>(entity =>
            {
                entity.HasKey(e => e.IdentityId).HasName("PRIMARY");

                entity.ToTable("identities");

                entity.Property(e => e.IdentityId).HasColumnName("identity_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");
            });

            modelBuilder.Entity<IdentitiesRoles>(entity =>
            {
                entity.HasKey(e => new { e.IdentityId, e.ApplicationId, e.PrivilegeId })
                    .HasName("PRIMARY");

                entity.ToTable("identities_roles");

                entity.HasIndex(e => new { e.ApplicationId, e.PrivilegeId })
                    .HasName("application_id");

                entity.Property(e => e.IdentityId).HasColumnName("identity_id");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.PrivilegeId).HasColumnName("privilege_id");

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.IdentitiesRoles)
                    .HasForeignKey(d => d.IdentityId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("Identities_roles_ibfk_1");

                entity.HasOne(d => d.Roles)
                    .WithMany(p => p.IdentitiesRoles)
                    .HasForeignKey(d => new { d.ApplicationId, d.PrivilegeId })
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("Identities_roles_ibfk_2");
            });

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

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.RevokedRefreshTokens)
                    .HasForeignKey(d => d.IdentityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("identities_refresh_tokens_ibfk_1");
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

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.RevokedAccessTokens)
                    .HasForeignKey(d => d.IdentityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("identities_access_tokens_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
