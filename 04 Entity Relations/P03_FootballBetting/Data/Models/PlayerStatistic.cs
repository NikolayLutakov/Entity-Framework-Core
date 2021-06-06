//using System.ComponentModel.DataAnnotations;

//namespace P03_FootballBetting.Data.Models
//{
//    public class PlayerStatistic
//    {
//        [Required]
//        public int PlayerId { get; set; }
//        [Required]
//        public int GameId { get; set; }
//        [Required]
//        public int Assists { get; set; }
//        [Required]
//        public int MinutesPlayed { get; set; }
//        [Required]
//        public int ScoredGoals { get; set; }

//        public Player Player { get; set; }
//        public Game Game { get; set; }
//    }
//}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace P03_FootballBetting.Data.Models
{
    public class PlayerStatistic
    {
        //GameId, PlayerId, ScoredGoals, Assists, MinutesPlayed

        public int GameId { get; set; }
        [Required]
        public int PlayerId { get; set; }
        [Required]
        public int ScoredGoals { get; set; }
        [Required]
        public int Assists { get; set; }
        [Required]
        public int MinutesPlayed { get; set; }

        public virtual Player Player { get; set; }
        public virtual Game Game { get; set; }
    }
}
