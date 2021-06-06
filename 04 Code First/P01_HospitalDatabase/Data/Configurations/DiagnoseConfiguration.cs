using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P01_HospitalDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P01_HospitalDatabase.Data.Configurations
{
    public class DiagnoseConfiguration : IEntityTypeConfiguration<Diagnose>
    {
        public void Configure(EntityTypeBuilder<Diagnose> diagnose)
        {
            diagnose.HasKey(d => d.DiagnoseId);

            diagnose.Property(d => d.Name)
                .HasMaxLength(50)
                .IsRequired(true);

            diagnose.Property(d => d.Comments)
                .HasMaxLength(250)
                .IsRequired(true);

            diagnose.HasOne(p => p.Patient)
                .WithMany(d => d.Diagnoses)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
