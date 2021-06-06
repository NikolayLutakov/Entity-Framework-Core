using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VaporStore.DataProcessor.Dto.Import
{
    public class ImportUserDto
    {
        [Required]
        [RegularExpression(@"^[A-Z]{1}[a-z]{1,} [A-Z]{1}[a-z]{1,}$")]
        public string FullName { get; set; }

        [Required]
        [MaxLength(20), MinLength(3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Range(typeof(Int32), "3", "103")]
        public int Age { get; set; }

        [Required]
        public ICollection<ImportCardDto> Cards { get; set; }
    }
}
