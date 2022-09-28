using System.ComponentModel.DataAnnotations;

namespace ASP_Webapp.Models.ViewModels
{
    public class EmployeeViewModel
    {
        public Employee Employee { get; set; }

        public int? HighestSalary { get; set; }
    }
}
