using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P03_FootballBetting.Data.Models
{
    public class Color
    {
        public Color()
        {
            this.PrimaryKitTeams = new HashSet<Team>();
            this.SecondaryKitTeams = new HashSet<Team>();
        }
        public int ColorId { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public ICollection<Team> PrimaryKitTeams { get; set; }
        [Required]
        public ICollection<Team> SecondaryKitTeams { get; set; }
    }
}

//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Text;

//namespace P03_FootballBetting.Data.Models
//{
//    public class Color
//    {
//        public Color()
//        {
//            this.PrimaryKitTeams = new HashSet<Team>();
//            this.SecondaryKitTeams = new HashSet<Team>();
//        }

//        public int ColorId { get; set; }
//        [Required]
//        public string Name { get; set; }

//        public virtual ICollection<Team> PrimaryKitTeams { get; set; }
//        public virtual ICollection<Team> SecondaryKitTeams { get; set; }
//    }
//}
