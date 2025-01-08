using System.Diagnostics;
using LabCommunictionProj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LabCommunictionProj.Controllers
{
    public class HomeController : Controller
    {
        private readonly string connectionString;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            connectionString = configuration.GetConnectionString("myConnect");
            _logger = logger;
        }

        public IActionResult SignOut()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            List<BookModel> books = new List<BookModel>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT * FROM tblBooks";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BookModel book = new BookModel
                            {
                                Title = reader.GetString(0),
                                Author = reader.GetString(1),
                                Publisher = reader.GetString(2),
                                Year = reader.GetString(3),
                                BorrowPrice = (float)reader.GetDouble(4),
                                BuyPrice = (float)reader.GetDouble(5),
                                CopiesNum = reader.GetInt32(6),
                                AgeLimit = reader.GetInt32(7),
                                Genre = reader.GetString(8),
                                Cover = reader.GetString(9),
                                Isbn = reader.GetString(10),
                                IsForSale = reader.GetString(11)
                            };
                            books.Add(book);
                        }
                    }
                }
            }
            return View(books);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}