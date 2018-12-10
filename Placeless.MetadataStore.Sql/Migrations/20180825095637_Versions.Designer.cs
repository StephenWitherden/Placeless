﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Placeless.MetadataStore.Sql;
using System;

namespace Placeless.MetadataStore.Sql.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20180825095637_Versions")]
    partial class Versions
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Placeless.MetadataStore.Sql.Attribute", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Attributes");
                });

            modelBuilder.Entity("Placeless.MetadataStore.Sql.AttributeValue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AttributeId");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("AttributeId");

                    b.ToTable("AttributeValues");
                });

            modelBuilder.Entity("Placeless.MetadataStore.Sql.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("Contents");

                    b.Property<Guid>("FileGuid");

                    b.Property<string>("Hash");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Placeless.MetadataStore.Sql.FileAttributeValue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AssignmentType");

                    b.Property<int>("AttributeValueId");

                    b.Property<int>("FileId");

                    b.HasKey("Id");

                    b.HasIndex("AttributeValueId");

                    b.HasIndex("FileId");

                    b.ToTable("FileAttributeValues");
                });

            modelBuilder.Entity("Placeless.MetadataStore.Sql.FileSource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("FileId");

                    b.Property<string>("Metadata");

                    b.Property<int>("SourceId");

                    b.Property<string>("SourceUri");

                    b.HasKey("Id");

                    b.HasIndex("FileId");

                    b.HasIndex("SourceId");

                    b.ToTable("FileSources");
                });

            modelBuilder.Entity("Placeless.MetadataStore.Sql.Source", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Sources");
                });

            modelBuilder.Entity("Placeless.MetadataStore.Sql.Version", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Contents");

                    b.Property<int>("FileId");

                    b.Property<int>("VersionTypeId");

                    b.HasKey("Id");

                    b.HasIndex("FileId");

                    b.HasIndex("VersionTypeId");

                    b.ToTable("Versions");
                });

            modelBuilder.Entity("Placeless.MetadataStore.Sql.VersionType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("VersionTypes");
                });

            modelBuilder.Entity("Placeless.MetadataStore.Sql.AttributeValue", b =>
                {
                    b.HasOne("Placeless.MetadataStore.Sql.Attribute", "Attribute")
                        .WithMany()
                        .HasForeignKey("AttributeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Placeless.MetadataStore.Sql.FileAttributeValue", b =>
                {
                    b.HasOne("Placeless.MetadataStore.Sql.AttributeValue", "AttributeValue")
                        .WithMany()
                        .HasForeignKey("AttributeValueId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Placeless.MetadataStore.Sql.File", "File")
                        .WithMany("FileAttributeValues")
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Placeless.MetadataStore.Sql.FileSource", b =>
                {
                    b.HasOne("Placeless.MetadataStore.Sql.File", "File")
                        .WithMany("FileSources")
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Placeless.MetadataStore.Sql.Source", "Source")
                        .WithMany("FileSources")
                        .HasForeignKey("SourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Placeless.MetadataStore.Sql.Version", b =>
                {
                    b.HasOne("Placeless.MetadataStore.Sql.File", "File")
                        .WithMany("Versions")
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Placeless.MetadataStore.Sql.VersionType", "VersionType")
                        .WithMany()
                        .HasForeignKey("VersionTypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
