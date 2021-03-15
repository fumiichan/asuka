﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using asukav2.Models;

namespace asukav2.Migrations
{
    [DbContext(typeof(CacheModelContext))]
    partial class CacheModelContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.3");

            modelBuilder.Entity("asukav2.Models.CacheModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DoujinCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("JsonData")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("CacheData");
                });

            modelBuilder.Entity("asukav2.Models.DoujinCounter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Count")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Date")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("DoujinTotalCounter");
                });

            modelBuilder.Entity("asukav2.Models.ImageHash", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DoujinCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Hash")
                        .HasColumnType("TEXT");

                    b.Property<string>("Image")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ImageHashes");
                });
#pragma warning restore 612, 618
        }
    }
}
