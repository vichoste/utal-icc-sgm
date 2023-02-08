﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using Utal.Icc.Sgm.Data;

#nullable disable

namespace Utal.Icc.Sgm.Migrations {
	[DbContext(typeof(ApplicationDbContext))]
	[Migration("20230208080415_Init")]
	partial class Init {
		/// <inheritdoc />
		protected override void BuildTargetModel(ModelBuilder modelBuilder) {
#pragma warning disable 612, 618
			modelBuilder
				.HasAnnotation("ProductVersion", "7.0.2")
				.HasAnnotation("Relational:MaxIdentifierLength", 128);

			SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

			modelBuilder.Entity("ApplicationUserStudentProposal", b => {
				b.Property<string>("AssistantTeachersOfTheStudentProposalId")
					.HasColumnType("nvarchar(450)");

				b.Property<string>("ImAssistantTeacherOfTheStudentProposalsId")
					.HasColumnType("nvarchar(450)");

				b.HasKey("AssistantTeachersOfTheStudentProposalId", "ImAssistantTeacherOfTheStudentProposalsId");

				b.HasIndex("ImAssistantTeacherOfTheStudentProposalsId");

				b.ToTable("ApplicationUserStudentProposal");
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b => {
				b.Property<string>("Id")
					.HasColumnType("nvarchar(450)");

				b.Property<string>("ConcurrencyStamp")
					.IsConcurrencyToken()
					.HasColumnType("nvarchar(max)");

				b.Property<string>("Name")
					.HasMaxLength(256)
					.HasColumnType("nvarchar(256)");

				b.Property<string>("NormalizedName")
					.HasMaxLength(256)
					.HasColumnType("nvarchar(256)");

				b.HasKey("Id");

				b.HasIndex("NormalizedName")
					.IsUnique()
					.HasDatabaseName("RoleNameIndex")
					.HasFilter("[NormalizedName] IS NOT NULL");

				b.ToTable("AspNetRoles", (string)null);
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b => {
				b.Property<int>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnType("int");

				SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

				b.Property<string>("ClaimType")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("ClaimValue")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("RoleId")
					.IsRequired()
					.HasColumnType("nvarchar(450)");

				b.HasKey("Id");

				b.HasIndex("RoleId");

				b.ToTable("AspNetRoleClaims", (string)null);
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b => {
				b.Property<int>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnType("int");

				SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

				b.Property<string>("ClaimType")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("ClaimValue")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("UserId")
					.IsRequired()
					.HasColumnType("nvarchar(450)");

				b.HasKey("Id");

				b.HasIndex("UserId");

				b.ToTable("AspNetUserClaims", (string)null);
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b => {
				b.Property<string>("LoginProvider")
					.HasColumnType("nvarchar(450)");

				b.Property<string>("ProviderKey")
					.HasColumnType("nvarchar(450)");

				b.Property<string>("ProviderDisplayName")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("UserId")
					.IsRequired()
					.HasColumnType("nvarchar(450)");

				b.HasKey("LoginProvider", "ProviderKey");

				b.HasIndex("UserId");

				b.ToTable("AspNetUserLogins", (string)null);
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b => {
				b.Property<string>("UserId")
					.HasColumnType("nvarchar(450)");

				b.Property<string>("RoleId")
					.HasColumnType("nvarchar(450)");

				b.HasKey("UserId", "RoleId");

				b.HasIndex("RoleId");

				b.ToTable("AspNetUserRoles", (string)null);
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b => {
				b.Property<string>("UserId")
					.HasColumnType("nvarchar(450)");

				b.Property<string>("LoginProvider")
					.HasColumnType("nvarchar(450)");

				b.Property<string>("Name")
					.HasColumnType("nvarchar(450)");

				b.Property<string>("Value")
					.HasColumnType("nvarchar(max)");

				b.HasKey("UserId", "LoginProvider", "Name");

				b.ToTable("AspNetUserTokens", (string)null);
			});

			modelBuilder.Entity("Utal.Icc.Sgm.Models.ApplicationUser", b => {
				b.Property<string>("Id")
					.HasColumnType("nvarchar(450)");

				b.Property<int>("AccessFailedCount")
					.HasColumnType("int");

				b.Property<string>("ConcurrencyStamp")
					.IsConcurrencyToken()
					.HasColumnType("nvarchar(max)");

				b.Property<DateTimeOffset>("CreatedAt")
					.HasColumnType("datetimeoffset");

				b.Property<string>("Email")
					.HasMaxLength(256)
					.HasColumnType("nvarchar(256)");

				b.Property<bool>("EmailConfirmed")
					.HasColumnType("bit");

				b.Property<string>("FirstName")
					.HasColumnType("nvarchar(max)");

				b.Property<bool>("IsDeactivated")
					.HasColumnType("bit");

				b.Property<string>("LastName")
					.HasColumnType("nvarchar(max)");

				b.Property<bool>("LockoutEnabled")
					.HasColumnType("bit");

				b.Property<DateTimeOffset?>("LockoutEnd")
					.HasColumnType("datetimeoffset");

				b.Property<string>("NormalizedEmail")
					.HasMaxLength(256)
					.HasColumnType("nvarchar(256)");

				b.Property<string>("NormalizedUserName")
					.HasMaxLength(256)
					.HasColumnType("nvarchar(256)");

				b.Property<string>("PasswordHash")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("PhoneNumber")
					.HasColumnType("nvarchar(max)");

				b.Property<bool>("PhoneNumberConfirmed")
					.HasColumnType("bit");

				b.Property<string>("Rut")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("SecurityStamp")
					.HasColumnType("nvarchar(max)");

				b.Property<bool>("StudentIsDoingThePractice")
					.HasColumnType("bit");

				b.Property<bool>("StudentIsWorking")
					.HasColumnType("bit");

				b.Property<string>("StudentRemainingCourses")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("StudentUniversityId")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("TeacherOffice")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("TeacherSchedule")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("TeacherSpecialization")
					.HasColumnType("nvarchar(max)");

				b.Property<bool>("TwoFactorEnabled")
					.HasColumnType("bit");

				b.Property<DateTimeOffset?>("UpdatedAt")
					.HasColumnType("datetimeoffset");

				b.Property<string>("UserName")
					.HasMaxLength(256)
					.HasColumnType("nvarchar(256)");

				b.HasKey("Id");

				b.HasIndex("NormalizedEmail")
					.HasDatabaseName("EmailIndex");

				b.HasIndex("NormalizedUserName")
					.IsUnique()
					.HasDatabaseName("UserNameIndex")
					.HasFilter("[NormalizedUserName] IS NOT NULL");

				b.ToTable("AspNetUsers", (string)null);
			});

			modelBuilder.Entity("Utal.Icc.Sgm.Models.StudentProposal", b => {
				b.Property<string>("Id")
					.HasColumnType("nvarchar(450)");

				b.Property<DateTimeOffset?>("CreatedAt")
					.HasColumnType("datetimeoffset");

				b.Property<string>("Description")
					.HasColumnType("nvarchar(max)");

				b.Property<string>("GuideTeacherOfTheStudentProposalId")
					.HasColumnType("nvarchar(450)");

				b.Property<string>("GuideTeacherWhoRejectedThisStudentProposalId")
					.HasColumnType("nvarchar(450)");

				b.Property<int?>("ProposalStatus")
					.HasColumnType("int");

				b.Property<string>("RejectionReason")
					.HasColumnType("nvarchar(max)");

				b.Property<byte[]>("RowVersion")
					.IsConcurrencyToken()
					.ValueGeneratedOnAddOrUpdate()
					.HasColumnType("rowversion");

				b.Property<string>("StudentOwnerOfTheStudentProposalId")
					.HasColumnType("nvarchar(450)");

				b.Property<string>("Title")
					.HasColumnType("nvarchar(max)");

				b.Property<DateTimeOffset?>("UpdatedAt")
					.HasColumnType("datetimeoffset");

				b.HasKey("Id");

				b.HasIndex("GuideTeacherOfTheStudentProposalId");

				b.HasIndex("GuideTeacherWhoRejectedThisStudentProposalId");

				b.HasIndex("StudentOwnerOfTheStudentProposalId");

				b.ToTable("StudentProposals");
			});

			modelBuilder.Entity("ApplicationUserStudentProposal", b => {
				b.HasOne("Utal.Icc.Sgm.Models.ApplicationUser", null)
					.WithMany()
					.HasForeignKey("AssistantTeachersOfTheStudentProposalId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();

				b.HasOne("Utal.Icc.Sgm.Models.StudentProposal", null)
					.WithMany()
					.HasForeignKey("ImAssistantTeacherOfTheStudentProposalsId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b => {
				b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
					.WithMany()
					.HasForeignKey("RoleId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b => {
				b.HasOne("Utal.Icc.Sgm.Models.ApplicationUser", null)
					.WithMany()
					.HasForeignKey("UserId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b => {
				b.HasOne("Utal.Icc.Sgm.Models.ApplicationUser", null)
					.WithMany()
					.HasForeignKey("UserId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b => {
				b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
					.WithMany()
					.HasForeignKey("RoleId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();

				b.HasOne("Utal.Icc.Sgm.Models.ApplicationUser", null)
					.WithMany()
					.HasForeignKey("UserId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b => {
				b.HasOne("Utal.Icc.Sgm.Models.ApplicationUser", null)
					.WithMany()
					.HasForeignKey("UserId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();
			});

			modelBuilder.Entity("Utal.Icc.Sgm.Models.StudentProposal", b => {
				b.HasOne("Utal.Icc.Sgm.Models.ApplicationUser", "GuideTeacherOfTheStudentProposal")
					.WithMany("ImGuideTeacherOfTheStudentProposals")
					.HasForeignKey("GuideTeacherOfTheStudentProposalId");

				b.HasOne("Utal.Icc.Sgm.Models.ApplicationUser", "GuideTeacherWhoRejectedThisStudentProposal")
					.WithMany("IRejectedTheseStudentProposals")
					.HasForeignKey("GuideTeacherWhoRejectedThisStudentProposalId");

				b.HasOne("Utal.Icc.Sgm.Models.ApplicationUser", "StudentOwnerOfTheStudentProposal")
					.WithMany("StudentProposalsWhichIOwn")
					.HasForeignKey("StudentOwnerOfTheStudentProposalId");

				b.Navigation("GuideTeacherOfTheStudentProposal");

				b.Navigation("GuideTeacherWhoRejectedThisStudentProposal");

				b.Navigation("StudentOwnerOfTheStudentProposal");
			});

			modelBuilder.Entity("Utal.Icc.Sgm.Models.ApplicationUser", b => {
				b.Navigation("IRejectedTheseStudentProposals");

				b.Navigation("ImGuideTeacherOfTheStudentProposals");

				b.Navigation("StudentProposalsWhichIOwn");
			});
#pragma warning restore 612, 618
		}
	}
}