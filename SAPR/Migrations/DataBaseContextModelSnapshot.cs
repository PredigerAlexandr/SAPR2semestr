﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SAPR.Models;

namespace SAPR.Migrations
{
    [DbContext(typeof(DataBaseContext))]
    partial class DataBaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("SAPR.Models.Field", b =>
                {
                    b.Property<int>("FieldId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Alias")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DefaultValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PurchaseId")
                        .HasColumnType("int");

                    b.HasKey("FieldId");

                    b.HasIndex("PurchaseId");

                    b.ToTable("Fields");
                });

            modelBuilder.Entity("SAPR.Models.Purchase", b =>
                {
                    b.Property<int>("PurchaseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<long?>("AfterRuleRuleId")
                        .HasColumnType("bigint");

                    b.Property<long?>("BeforeRuleRuleId")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PurchaseId");

                    b.HasIndex("AfterRuleRuleId");

                    b.HasIndex("BeforeRuleRuleId");

                    b.ToTable("Purchases");
                });

            modelBuilder.Entity("SAPR.Models.Rule", b =>
                {
                    b.Property<long>("RuleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<long>("PurchaseId")
                        .HasColumnType("bigint");

                    b.Property<string>("RuleText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Stage")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RuleId");

                    b.ToTable("Rules");
                });

            modelBuilder.Entity("SAPR.Models.Field", b =>
                {
                    b.HasOne("SAPR.Models.Purchase", null)
                        .WithMany("Fields")
                        .HasForeignKey("PurchaseId");
                });

            modelBuilder.Entity("SAPR.Models.Purchase", b =>
                {
                    b.HasOne("SAPR.Models.Rule", "AfterRule")
                        .WithMany()
                        .HasForeignKey("AfterRuleRuleId");

                    b.HasOne("SAPR.Models.Rule", "BeforeRule")
                        .WithMany()
                        .HasForeignKey("BeforeRuleRuleId");

                    b.Navigation("AfterRule");

                    b.Navigation("BeforeRule");
                });

            modelBuilder.Entity("SAPR.Models.Purchase", b =>
                {
                    b.Navigation("Fields");
                });
#pragma warning restore 612, 618
        }
    }
}
