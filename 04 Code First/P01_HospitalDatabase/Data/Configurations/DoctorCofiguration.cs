using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P01_HospitalDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P01_HospitalDatabase.Data.Configurations
{
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> doctor)
        {

            doctor.HasKey(p => p.DoctorId);

            doctor.Property(p => p.Name)
            .HasMaxLength(100)
            .IsUnicode()
            .IsRequired(true);


            doctor.Property(p => p.Specialty)
            .HasMaxLength(100)
            .IsUnicode()
            .IsRequired(true);
        }
    }
}
