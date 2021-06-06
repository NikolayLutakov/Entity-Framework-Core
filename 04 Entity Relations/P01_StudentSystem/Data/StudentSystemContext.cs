using Microsoft.EntityFrameworkCore;
using P01_StudentSystem.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P01_StudentSystem.Data
{
    public class StudentSystemContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Homework> HomeworkSubmissions { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }


        public StudentSystemContext()
        {
        }

        public StudentSystemContext(DbContextOptions<StudentSystemContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=192.168.0.105,1433;Database=StudentSystem;User ID=nlyutakov;Password=A123456a;");
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(student =>
            {

                student.HasKey(s => s.StudentId);

                student.Property(s => s.Name)
                .HasMaxLength(100)
                .IsRequired(true)
                .IsUnicode(true);

                student.Property(s => s.PhoneNumber)
                    .IsUnicode(false)
                    .IsRequired(false)
                    .HasColumnType("CHAR(10)");

                student.Property(s => s.RegisteredOn)
                .IsRequired(true);

                student.Property(s => s.Birthday)
                .IsRequired(false);
            });

            modelBuilder.Entity<Course>(course =>
            {


                course.HasKey(c => c.CourseId);

                course.Property(c => c.Name)
                .HasMaxLength(80)
                .IsRequired(true)
                .IsUnicode(true);

                course.Property(c => c.Description)
                .IsRequired(false)
                .IsUnicode(true);

                course.Property(c => c.StartDate)
                .IsRequired(true);

                course.Property(c => c.EndDate)
                    .IsRequired(true);

                course.Property(c => c.Price)
                    .IsRequired(true);
            });

            modelBuilder.Entity<StudentCourse>(studentCourse =>
            {


                studentCourse.HasKey(st => new { st.StudentId, st.CourseId });

                studentCourse.HasOne(st => st.Course)
               .WithMany(c => c.StudentsEnrolled)
               .HasForeignKey(st => st.CourseId)
               .OnDelete(DeleteBehavior.Restrict);

                studentCourse.HasOne(st => st.Student)
                .WithMany(s => s.CourseEnrollments)
                .HasForeignKey(st => st.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<Resource>(resource =>
            {



                resource.HasKey(r => r.ResourceId);

                resource.Property(r => r.Name)
                .HasMaxLength(50)
                .IsRequired(true)
                .IsUnicode(true);

                resource.Property(r => r.Url)
                .IsUnicode(false)
                .IsRequired(true);

                resource.HasOne(r => r.Course)
               .WithMany(c => c.Resources)
               .HasForeignKey(r => r.CourseId)
               .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<Homework>(homework =>
            {


                homework.HasKey(h => h.HomeworkId);

                homework.Property(h => h.Content).IsUnicode(false).IsRequired(true);

                homework.Property(h => h.SubmissionTime)
                .IsRequired(true);

                homework.HasOne(h => h.Student)
                .WithMany(s => s.HomeworkSubmissions)
                .HasForeignKey(h => h.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

                homework.HasOne(h => h.Course)
               .WithMany(c => c.HomeworkSubmissions)
               .HasForeignKey(h => h.CourseId)
               .OnDelete(DeleteBehavior.Restrict);

            });
        }
    }
}
