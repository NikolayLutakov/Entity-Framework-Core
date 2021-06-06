using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P01_HospitalDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P01_HospitalDatabase.Data.Configurations
{
    public class PatientCofiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> patient)
        {

            patient.HasKey(p => p.PatientId);

            patient.Property(p => p.FirstName)
            .HasMaxLength(50)
            .IsUnicode()
            .IsRequired(true);


            patient.Property(p => p.LastName)
            .HasMaxLength(50)
            .IsUnicode()
            .IsRequired(true);


            patient.Property(p => p.Address)
            .HasMaxLength(250)
            .IsUnicode()
            .IsRequired(true);


            patient.Property(p => p.Email)
            .HasMaxLength(80)
            .IsUnicode(false)
            .IsRequired(true);

        }
    }
}
