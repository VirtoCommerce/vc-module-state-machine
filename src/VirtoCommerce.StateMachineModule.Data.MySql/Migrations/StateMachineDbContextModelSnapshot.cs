﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VirtoCommerce.StateMachineModule.Data.Repositories;

#nullable disable

namespace VirtoCommerce.StateMachineModule.Data.MySql.Migrations
{
    [DbContext(typeof(StateMachineDbContext))]
    partial class StateMachineDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("VirtoCommerce.StateMachineModule.Data.Models.StateMachineDefinitionEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("EntityType")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("varchar(512)");

                    b.Property<string>("StatesCapture")
                        .HasColumnType("longtext");

                    b.Property<string>("StatesGraph")
                        .HasColumnType("longtext");

                    b.Property<string>("StatesSerialized")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Version")
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)");

                    b.HasKey("Id");

                    b.ToTable("StateMachineDefinition", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.StateMachineModule.Data.Models.StateMachineInstanceEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("EntityId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("EntityType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("State")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("StateMachineId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("StateMachineId");

                    b.ToTable("StateMachineInstance", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.StateMachineModule.Data.Models.StateMachineInstanceEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StateMachineModule.Data.Models.StateMachineDefinitionEntity", "StateMachineDefinition")
                        .WithMany()
                        .HasForeignKey("StateMachineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("StateMachineDefinition");
                });
#pragma warning restore 612, 618
        }
    }
}
