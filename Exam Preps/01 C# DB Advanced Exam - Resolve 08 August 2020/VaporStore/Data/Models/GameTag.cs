﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VaporStore.Data.Models
{
    public class GameTag
    {
        public int GameId { get; set; }

        public virtual Game Game { get; set; }

        public int TagId { get; set; }

        public virtual Tag Tag { get; set; }
    }
}

//•	GameId – integer, Primary Key, foreign key (required)
//•	Game – Game
//•	TagId – integer, Primary Key, foreign key (required)
//•	Tag – Tag

