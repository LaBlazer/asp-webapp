using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_Webapp.Models
{
    public class Employer
    {
        public Employer()
        {
           Salaries = new List<Salary>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EmployerId { get; set; }

        public ICollection<Salary> Salaries { get; set; }
    }
}
