﻿// <auto-generated />
using System;
using DRES.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DRES.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250316074919_useractivitylog")]
    partial class useractivitylog
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DRES.Models.Site", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<string>("addresszip")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("city")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("country")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("createdat")
                        .HasColumnType("datetime2");

                    b.Property<int>("createdbyid")
                        .HasColumnType("int");

                    b.Property<string>("description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("siteaddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("sitename")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("state")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("status")
                        .HasColumnType("int");

                    b.Property<DateTime>("updatedat")
                        .HasColumnType("datetime2");

                    b.Property<int>("updatedbyid")
                        .HasColumnType("int");

                    b.HasKey("id");

                    b.HasIndex("sitename")
                        .IsUnique();

                    b.ToTable("Sites");
                });

            modelBuilder.Entity("DRES.Models.User", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("createdbyid")
                        .HasColumnType("int");

                    b.Property<string>("passwordhash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("role")
                        .HasColumnType("int");

                    b.Property<int?>("siteid")
                        .HasColumnType("int");

                    b.Property<int>("updatedbyid")
                        .HasColumnType("int");

                    b.Property<string>("username")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("id");

                    b.HasIndex("siteid");

                    b.HasIndex("username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DRES.Models.UserActivityLog", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<DateTime>("createdat")
                        .HasColumnType("datetime2");

                    b.Property<string>("devicetype")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ipaddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("jwttoken")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("location")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("userid")
                        .HasColumnType("int");

                    b.HasKey("id");

                    b.ToTable("UserActivityLogs");
                });

            modelBuilder.Entity("DRES.Models.User", b =>
                {
                    b.HasOne("DRES.Models.Site", "Site")
                        .WithMany("Users")
                        .HasForeignKey("siteid")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Site");
                });

            modelBuilder.Entity("DRES.Models.Site", b =>
                {
                    b.Navigation("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
