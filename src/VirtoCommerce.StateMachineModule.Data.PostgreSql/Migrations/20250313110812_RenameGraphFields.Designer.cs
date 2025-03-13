﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VirtoCommerce.StateMachineModule.Data.Repositories;

#nullable disable

namespace VirtoCommerce.StateMachineModule.Data.PostgreSql.Migrations
{
    [DbContext(typeof(StateMachineDbContext))]
    [Migration("20250313110812_RenameGraphFields")]
    partial class RenameGraphFields
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("VirtoCommerce.StateMachineModule.Data.Models.StateMachineDefinitionEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EntityType")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("StatesCapture")
                        .HasColumnType("text");

                    b.Property<string>("StatesGraph")
                        .HasColumnType("text");

                    b.Property<string>("StatesSerialized")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Version")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.HasKey("Id");

                    b.ToTable("StateMachineDefinition", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.StateMachineModule.Data.Models.StateMachineInstanceEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EntityId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("EntityType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("State")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("StateMachineId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

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
