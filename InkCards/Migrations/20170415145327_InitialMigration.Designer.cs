using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using InkCards.Services.Storage.Sqlite;
using InkCards.Models.Cards;

namespace InkCards.Migrations
{
    [DbContext(typeof(MainDatabaseContext))]
    [Migration("20170415145327_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("InkCards.Models.Testing.CardImpression", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("BackMillisecondsSpent");

                    b.Property<Guid>("CardId");

                    b.Property<DateTime>("Date");

                    b.Property<long>("FrontMillisecondsSpent");

                    b.Property<bool>("GuessedCorrectly");

                    b.Property<int>("TestedSide");

                    b.HasKey("Id");

                    b.ToTable("CardImpressions");
                });
        }
    }
}
