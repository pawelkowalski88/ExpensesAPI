﻿// <auto-generated />
using System;
using ExpensesAPI.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExpensesAPI.Domain.Migrations
{
    [DbContext(typeof(MainDbContext))]
    partial class MainDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ExpensesAPI.Domain.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<int>("ScopeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ScopeId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("ExpensesAPI.Domain.Models.Expense", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("nvarchar(1024)")
                        .HasMaxLength(1024);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Details")
                        .HasColumnType("nvarchar(1024)")
                        .HasMaxLength(1024);

                    b.Property<int>("ScopeId")
                        .HasColumnType("int");

                    b.Property<float>("Value")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ScopeId");

                    b.ToTable("Expenses");
                });

            modelBuilder.Entity("ExpensesAPI.Domain.Models.Scope", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OwnerId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Scopes");
                });

            modelBuilder.Entity("ExpensesAPI.Domain.Models.ScopeUser", b =>
                {
                    b.Property<int>("ScopeId")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ScopeId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("ScopeUser");
                });

            modelBuilder.Entity("ExpensesAPI.Domain.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("SelectedScopeId")
                        .HasColumnType("int");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SelectedScopeId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ExpensesAPI.Domain.Models.Category", b =>
                {
                    b.HasOne("ExpensesAPI.Domain.Models.Scope", "Scope")
                        .WithMany("Categories")
                        .HasForeignKey("ScopeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ExpensesAPI.Domain.Models.Expense", b =>
                {
                    b.HasOne("ExpensesAPI.Domain.Models.Category", "Category")
                        .WithMany("Expenses")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ExpensesAPI.Domain.Models.Scope", "Scope")
                        .WithMany("Expenses")
                        .HasForeignKey("ScopeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ExpensesAPI.Domain.Models.Scope", b =>
                {
                    b.HasOne("ExpensesAPI.Domain.Models.User", "Owner")
                        .WithMany("OwnedScopes")
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("ExpensesAPI.Domain.Models.ScopeUser", b =>
                {
                    b.HasOne("ExpensesAPI.Domain.Models.Scope", "Scope")
                        .WithMany("ScopeUsers")
                        .HasForeignKey("ScopeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ExpensesAPI.Domain.Models.User", "User")
                        .WithMany("ScopeUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ExpensesAPI.Domain.Models.User", b =>
                {
                    b.HasOne("ExpensesAPI.Domain.Models.Scope", "SelectedScope")
                        .WithMany()
                        .HasForeignKey("SelectedScopeId");
                });
#pragma warning restore 612, 618
        }
    }
}
