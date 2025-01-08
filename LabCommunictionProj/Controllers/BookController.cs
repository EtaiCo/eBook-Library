using LabCommunictionProj.Models;
using LabCommunictionProj.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LabCommunictionProj.Controllers
{
    public class BookController : Controller
    {
        private readonly IConfiguration _configuration;
        string connectionString = "";

        public BookController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("myConnect");
        }

        public IActionResult AddBook()
        {
            return View("AddBook", new BookModel());
        }


        [HttpPost]
        public IActionResult RemoveBook(string isbn)
        {
            if (string.IsNullOrEmpty(isbn))
            {
                TempData["Message"] = "Title cannot be empty.";
                return RedirectToAction("RemoveBook");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "DELETE FROM tblBooks WHERE isbn = @isbn " +
                    "DELETE FROM tblCart WHERE isbn = @isbn "+
                    "DELETE FROM tblUserBooks WHERE isbn = @isbn " +
                    "DELETE FROM tblWaitingList WHERE isbn = @isbn"
                    ;

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@isbn", isbn);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        TempData["Message"] = "Book removed successfully.";
                    }
                    else
                    {
                        TempData["Message"] = "Failed to remove book. Title not found.";
                    }
                }
            }

            return RedirectToAction("SearchBooks");
        }


        public IActionResult ViewBook(BookModel book)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlQuery = "INSERT INTO tblBooks VALUES (@title, @author, @publisher, @year, @buyPrice, @borrowPrice, @copiesNum, @ageLimit, @genre,@cover,@isbn,@isForSale)";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@title", book.Title);
                        command.Parameters.AddWithValue("@author", book.Author);
                        command.Parameters.AddWithValue("@publisher", book.Publisher);
                        command.Parameters.AddWithValue("@year", book.Year);
                        command.Parameters.AddWithValue("@buyPrice", book.BuyPrice);
                        command.Parameters.AddWithValue("@borrowPrice", book.BorrowPrice);
                        command.Parameters.AddWithValue("@copiesNum", book.CopiesNum);
                        command.Parameters.AddWithValue("@ageLimit", book.AgeLimit);
                        command.Parameters.AddWithValue("@genre", book.Genre);
                        command.Parameters.AddWithValue("@cover", book.Cover);
                        command.Parameters.AddWithValue("@isbn", book.Isbn);
                        command.Parameters.AddWithValue("@isForSale", book.IsForSale);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return View("ViewBook", book);
                        }
                        else
                        {
                            return View("AddBook", book);
                        }
                    }
                }
            }
            return View("AddBook", book);
        }
        public IActionResult SearchBooks(string searchBooks)
        {
            
            BooksViewModel books = new BooksViewModel
            {
                Books = new List<BookModel>(),
                SearchTerm = searchBooks 
            };

            string sqlQuery = "SELECT * FROM tblBooks";

            // If there is a search term, modify the query to filter books
            if (!string.IsNullOrWhiteSpace(searchBooks))
            {
                sqlQuery += " WHERE title LIKE @param";
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    if (!string.IsNullOrWhiteSpace(searchBooks))
                    {
                        command.Parameters.AddWithValue("@param", $"%{searchBooks}%");
                    }

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
                                IsForSale = reader.GetString(11),
                            };

                            books.Books.Add(book);
                        }
                    }
                }
            }

            return View(books);
        }


    }
}

