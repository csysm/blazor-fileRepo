﻿// <auto-generated />
using System;
using FileRepoSys.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FileRepoSys.Api.Migrations
{
    [DbContext(typeof(FileRepoSysDbContext))]
    [Migration("20230209151038_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("FileRepoSys.Api.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<long>("CurrentCapacity")
                        .HasColumnType("bigint");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<long>("MaxCapacity")
                        .HasColumnType("bigint");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = new Guid("07dc2516-590c-46d9-adf4-d1c89b67b991"),
                            CreateTime = new DateTime(2023, 2, 9, 23, 10, 38, 633, DateTimeKind.Local).AddTicks(408),
                            CurrentCapacity = 0L,
                            Email = "593676339@qq.com",
                            IsActive = true,
                            IsDeleted = false,
                            MaxCapacity = 10485760L,
                            Password = "123",
                            UserName = "wxx"
                        });
                });

            modelBuilder.Entity("FileRepoSys.Api.Entities.UserFile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<string>("FileStorageName")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Suffix")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserFiles");
                });

            modelBuilder.Entity("FileRepoSys.Api.Entities.UserFile", b =>
                {
                    b.HasOne("FileRepoSys.Api.Entities.User", "User")
                        .WithMany("UserFiles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("FileRepoSys.Api.Entities.User", b =>
                {
                    b.Navigation("UserFiles");
                });
#pragma warning restore 612, 618
        }
    }
}
