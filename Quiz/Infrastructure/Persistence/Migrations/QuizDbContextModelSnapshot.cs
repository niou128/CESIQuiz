using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Quiz.Infrastructure.Persistence;
using Quiz.Models;

#nullable disable

namespace Quiz.Infrastructure.Persistence.Migrations;

[DbContext(typeof(QuizDbContext))]
partial class QuizDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity("Quiz.Models.Question", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("Category")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<List<string>>("Choices")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<int>("CorrectAnswerIndex")
                    .HasColumnType("INTEGER");

                b.Property<string>("Text")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("Questions");
            });

        modelBuilder.Entity("Quiz.Models.QuizScore", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("CompletedAtUtc")
                    .HasColumnType("TEXT");

                b.Property<int>("CorrectAnswers")
                    .HasColumnType("INTEGER");

                b.Property<int>("QuestionCount")
                    .HasColumnType("INTEGER");

                b.Property<QuizSourceMode>("QuizMode")
                    .IsRequired()
                    .HasConversion<string>()
                    .HasColumnType("TEXT");

                b.Property<int>("UserId")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.ToTable("Scores");
            });

        modelBuilder.Entity("Quiz.Models.UserAccount", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("CreatedAtUtc")
                    .HasColumnType("TEXT");

                b.Property<bool>("IsAdmin")
                    .HasColumnType("INTEGER");

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("PasswordSalt")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Username")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("Username")
                    .IsUnique();

                b.ToTable("Users");
            });

        modelBuilder.Entity("Quiz.Models.QuizScore", b =>
            {
                b.HasOne("Quiz.Models.UserAccount", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });
#pragma warning restore 612, 618
    }
}
