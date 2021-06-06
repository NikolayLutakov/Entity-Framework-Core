using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P01_HospitalDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P01_HospitalDatabase.Data.Configurations
{
    public class MedicamentConfiguration : IEntityTypeConfiguration<Medicament>
    {
        public void Configure(EntityTypeBuilder<Medicament> medicament)
        {
            medicament.HasKey(m => m.MedicamentId);

            medicament.Property(m => m.Name)
                .HasMaxLength(50)
                .IsRequired(true);


        }
    }
}
