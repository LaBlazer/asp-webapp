using ASP_Webapp.Data;
using ASP_Webapp.Models;
using ASP_Webapp.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;

namespace ASP_Webapp.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly string URL_EMPLOYEES = "https://xevos.store/domaci-ukol/Jmena.json";
        private readonly string URL_EMPLOYERS = "https://xevos.store/domaci-ukol/Zamestnavatel{0:d}.json";

        private readonly ILogger<HomeController> _logger;
        private readonly CompanyContext _context;

        public EmployeeController(ILogger<HomeController> logger, CompanyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public ActionResult Update()
        {
            var employees = new List<Employee>();

            _logger.LogInformation("Updating the employee data...");

            // Clear the database
            _context.ClearDatabase();

            using (var httpClient = new HttpClient())
            {
                // Download employees
                var response = httpClient.GetStringAsync(URL_EMPLOYEES);
                response.Wait();

                if (response.IsCompletedSuccessfully)
                {
                    // Parse the json
                    var json = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(response.Result);

                    foreach(var user in json)
                    {
                        employees.Add(new Employee
                        {
                            EmployeeId = Int32.Parse(user["id"]),
                            FirstName = user["jmeno"],
                            LastName = user["prijmeni"],
                            Date = DateTime.ParseExact(user["date"], "yyyy-MM-dd", CultureInfo.InvariantCulture)
                        });
                    }

                    _logger.LogDebug(response.Result);
                }else
                {
                    _logger.LogError($"Unable to download data from '{URL_EMPLOYEES}'");
                }

                // Add the users to database
                _context.Employees.AddRange(employees);
                _context.SaveChanges();

                // Download salaries
                for(int i = 1; i <= 3; i++)
                {
                    // Add employer
                    var employer = new Employer { EmployerId = i };
                    _context.Employers.Add(employer);
                    _context.SaveChanges();

                    response = httpClient.GetStringAsync(String.Format(URL_EMPLOYERS, i));
                    response.Wait();

                    if (response.IsCompletedSuccessfully)
                    {
                        // Parse the json
                        var json = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(response.Result);

                        foreach (var s in json)
                        {
                            var salary = new Salary
                            {
                                Amount = Int32.Parse(s["plat"])
                            };                       

                            // Add salary to correct employer and employee
                            employer.Salaries.Add(salary);
                            
                            var employee = _context.Employees.First(e => e.FirstName == s["jmeno"] && e.LastName == s["prijmeni"]);
                            if (employee != null)
                            {
                                employee.Salaries.Add(salary);
                            }
                            else
                            {
                                _logger.LogError($"Employee with name {s["jmeno"]} {s["prijmeni"]} could not be found");
                            }

                            // Save changes
                            _context.Salaries.Add(salary);
                            _context.SaveChanges();
                        }

                        _logger.LogDebug(response.Result);
                    }
                    else
                    {
                        _logger.LogError($"Unable to download data from '{URL_EMPLOYEES}'");
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: EmployeeController
        public ActionResult Index()
        {
            var view = from e in _context.Employees
                       select new EmployeeViewModel
                       {
                           Employee = e,
                           HighestSalary = e.Salaries.OrderByDescending(s => s.Amount).First().Amount
                       };

            return View(view.ToList());
        }

        // GET: EmployeeController/Edit/5
        public ActionResult Edit(int id)
        {
            var employee = _context.Employees.Find(id);

            if (employee == null)
            {
                return new NotFoundResult();
            }

            return View(employee);
        }

        // POST: EmployeeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Employee employee)
        {
            //var employee = _context.Employees.Find(id);

            try
            {
                _logger.LogInformation(employee.Date.ToString());
                if (ModelState.IsValid)
                {
                    _context.Update(employee);
                    _context.SaveChanges();
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewBag.Message = "Unable to edit this entry";
                return View(employee);
            }
        }

        // GET: EmployeeController/Delete/5
        public ActionResult Delete(int id)
        {
            var employee = _context.Employees.Find(id);

            if (employee == null)
            {
                return new NotFoundResult();
            }

            return View(employee);
        }

        // POST: EmployeeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            var employee = _context.Employees.Include(e => e.Salaries).Where(e => e.EmployeeId == id).First();
            try
            {

                if (employee == null)
                {
                    return new NotFoundResult();
                }

                _context.RemoveRange(employee.Salaries);
                _context.Remove(employee);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewBag.Message = "Unable to delete this entry";
                return View(employee);
            }
        }
    }
}
