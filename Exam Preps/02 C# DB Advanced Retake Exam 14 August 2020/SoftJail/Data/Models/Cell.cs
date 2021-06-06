using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoftJail.Data.Models
{
    public class Cell
    {
        public Cell()
        {
            this.Prisoners = new HashSet<Prisoner>();
        }

        public int Id { get; set; }


        [Range(1, 1000)]
        public int CellNumber { get; set; }


        public bool HasWindow { get; set; }

        
        public int DepartmentId { get; set; }


        public virtual Department Department { get; set; }

        public virtual ICollection<Prisoner> Prisoners { get; set; }

    }
}

//•	Id – integer, Primary Key
//•	CellNumber – integer in the range [1, 1000] (required)
//•	HasWindow – bool(required)
//•	DepartmentId - integer, foreign key(required)
//•	Department – the cell's department (required)
//•	Prisoners - collection of type Prisoner
