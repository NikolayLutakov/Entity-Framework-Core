using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P01_HospitalDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P01_HospitalDatabase.Data.Configurations
{
    public class VisitationConfiguration : IEntityTypeConfiguration<Visitation>
    {
        public void Configure(EntityTypeBuilder<Visitation> visitation)
        {
            visitation.HasKey(v => v.VisitationId);

            visitation.Property(v => v.Comments)
                .HasMaxLength(250)
                .IsRequired(true);

            visitation.Property(v => v.Date)
                .HasMaxLength(250)
                .IsRequired(true);

            visitation.HasOne(v => v.Patient)
                .WithMany(v => v.Visitations)
                .HasForeignKey(v => v.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            visitation.HasOne(v => v.Doctor)
                .WithMany(v => v.Visitations)
                .HasForeignKey(v => v.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
