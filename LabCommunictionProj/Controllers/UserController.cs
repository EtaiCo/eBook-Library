using iText.Kernel.Pdf;
using iText.Layout.Element;
using LabCommunictionProj.Models;
using LabCommunictionProj.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.XMP.Impl;
using LabCommunictionProj.Utils;

namespace LabCommunictionProj.Controllers
{
    public class UserController : Controller
    {

        private readonly IConfiguration _configuration;
        string connectionString = "";
        int totalPrice = 0;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("myConnect");
        }
        public IActionResult SignUp()
        {
            return View("SignUp", new UserModel());
        }
        public IActionResult ViewUser(UserModel user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if ID or email already exists
                string checkQuery = "SELECT COUNT(*) FROM tblUser WHERE id = @id OR mail = @mail";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@id", user.Id);
                    checkCommand.Parameters.AddWithValue("@mail", user.Mail);
                    if (string.IsNullOrWhiteSpace(user.Id) || string.IsNullOrWhiteSpace(user.Mail))
                    {                      
                        return View("SignUp", user);
                    }

                    int existingCount = (int)checkCommand.ExecuteScalar();

                    if (existingCount > 0)
                    {
                        // Store a flag in ModelState to indicate duplicate exists
                        ModelState.AddModelError("DuplicateEntry", "The ID or email is already in use. Please enter different details.");

                        // Return the same view with existing data and error
                        return View("SignUp", user);
                    }
                }

                // If no duplicates, proceed with inserting the user
                string sqlQuery = "INSERT INTO tblUser VALUES (@id, @firstName, @lastName, @phone, @mail, @password, @isAdmin)";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    string hashedPassword = Utils.PasswordHasher.Hash(user.Password);
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@firstName", user.FirstName);
                    command.Parameters.AddWithValue("@lastName", user.LastName);
                    command.Parameters.AddWithValue("@phone", user.PhoneNumber);
                    command.Parameters.AddWithValue("@mail", user.Mail);
                    command.Parameters.AddWithValue("@password", hashedPassword);
                    command.Parameters.AddWithValue("@isAdmin", user.IsAdmin);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return RedirectToAction("SignIn");
                    }
                    else
                    {
                        ModelState.AddModelError("DatabaseError", "An error occurred while adding the user. Please try again.");
                        return View("SignUp", user);
                    }
                }
            }
        }

        public IActionResult SignIn(SignInViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string hashedPassword = Utils.PasswordHasher.Hash(model.Password);
                    string sqlQuery = "SELECT * FROM tblUser WHERE mail = @mail AND password = @password";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@mail", model.Mail);
                        command.Parameters.AddWithValue("@password",hashedPassword);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                var user = new UserModel
                                {
                                    Id = reader["Id"].ToString(),
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    PhoneNumber = reader["Phone"].ToString(),
                                    Mail = reader["Mail"].ToString(),
                                    Password = reader["Password"].ToString(),
                                    IsAdmin = reader["IsAdmin"].ToString()
                                };                               
                                HttpContext.Session.SetString("UserId", user.Id);
                                HttpContext.Session.SetString("FirstName", user.FirstName);
                                HttpContext.Session.SetString("LastName", user.LastName);
                                HttpContext.Session.SetString("PhoneNumber", user.PhoneNumber);
                                HttpContext.Session.SetString("Mail", user.Mail);
                                HttpContext.Session.SetString("Password", user.Password);
                                HttpContext.Session.SetString("IsAdmin", user.IsAdmin);
                                if(user.IsAdmin == "yes")
                                {
                                    return RedirectToAction("Index","Home");
                                }
                                return RedirectToAction("UserConnection", user);
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Invalid Email or Password!";
                                return View(model);
                            }
                        }
                    }
                }
            }

            return View(model);
        }

        public IActionResult SearchUsers(string searchUsers)
        {
            UserViewModel users = new UserViewModel
            {
                Users = new List<UserModel>(),
                SearchTerm = searchUsers // Add this to maintain search term in view
            };

            string sqlQuery = "SELECT * FROM tblUser";
            // If there is a search term, modify the query to filter users
            if (!string.IsNullOrWhiteSpace(searchUsers))
            {
                sqlQuery += " WHERE firstName LIKE @param OR lastName LIKE @param";
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    if (!string.IsNullOrWhiteSpace(searchUsers))
                    {
                        command.Parameters.AddWithValue("@param", $"%{searchUsers}%");
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UserModel user = new UserModel
                            {
                                Id = reader.GetString(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                PhoneNumber = reader.GetString(3),
                                Mail = reader.GetString(4),
                                Password = reader.GetString(5)
                            };
                            users.Users.Add(user);
                        }
                    }
                }
            }
            return View(users);
        }
        public IActionResult RemoveUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Message"] = "User ID cannot be empty.";
                return RedirectToAction("SearchUsers");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "DELETE FROM tblCart WHERE id = @id " +
                                 "DELETE FROM tblUserBooks WHERE userId = @id " +
                                 "DELETE FROM tblWaitingList WHERE id = @id " +
                                 "DELETE FROM tblUser WHERE id = @id";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        TempData["Message"] = "User removed successfully.";
                    }
                    else
                    {
                        TempData["Message"] = "Failed to remove user. User not found.";
                    }
                }
            }
            return RedirectToAction("SearchUsers");
        }

        public IActionResult UserConnection()
        {
            var user = new UserModel
            {
                Id = HttpContext.Session.GetString("UserId"),
                FirstName = HttpContext.Session.GetString("FirstName"),
                LastName = HttpContext.Session.GetString("LastName"),
                PhoneNumber = HttpContext.Session.GetString("PhoneNumber"),
                Mail = HttpContext.Session.GetString("Mail"),
                Password = HttpContext.Session.GetString("Password") 
            };

            var purchasedBooks = new BooksViewModel { Books = GetPurchasedBooks(user.Id) };
            var borrowedBooks = new BooksViewModel { Books = GetBorrowedBooks(user.Id) };

            var viewModel = new UserAndBooksViewModel
            {
                User = user,
                purchasedBooks = purchasedBooks.Books,
                borrowedBooks = borrowedBooks.Books
            };

            return View(viewModel);
        }

        public List<BookModel> GetBooksFromDb(string userId, string sqlQuery)
        {
            List<BookModel> books = new List<BookModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

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
                                Cover = reader.IsDBNull(9) ? null : reader.GetString(9),
                                Isbn = reader.IsDBNull(10) ? null : reader.GetString(10),
                                IsForSale = reader.IsDBNull(11) ? null : reader.GetString(11),
                            };

                            books.Add(book);
                        }
                    }
                }
            }

            return books;
        }

        private List<BookModel> GetBorrowedBooks(string userId)
        {
            string sqlQuery = @"
            SELECT * 
            FROM tblBooks b 
            WHERE b.isbn IN (
                SELECT isbn 
                FROM tblUserBooks 
                WHERE userId = @userId AND book_status = 'Borrowed')";

            return GetBooksFromDb(userId, sqlQuery);
        }
        private List<BookModel> GetPurchasedBooks(string userId)
        {
            string sqlQuery = @"
            SELECT * 
            FROM tblBooks b 
            WHERE b.isbn IN (
                SELECT isbn 
                FROM tblUserBooks 
                WHERE userId = @userId AND book_status = 'Purchased')";           

            return GetBooksFromDb(userId, sqlQuery);
        }

        public IActionResult DownloadBookPdf(string bookTitle)
        {
            try
            {
                // Get book details from database
                BookModel book = null;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlQuery = "SELECT * FROM tblBooks WHERE title = @title";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@title", bookTitle);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                book = new BookModel
                                {
                                    Title = reader.GetString(0),
                                    Author = reader.GetString(1),
                                    Publisher = reader.GetString(2),
                                    Year = reader.GetString(3),
                                    Genre = reader.GetString(8),
                                    Isbn = reader.IsDBNull(10) ? null : reader.GetString(10)
                                };
                            }
                        }
                    }
                }

                if (book == null)
                {
                    return NotFound("Book not found");
                }

                byte[] pdfBytes;
                using (var memoryStream = new MemoryStream())
                {
                    var writer = new PdfWriter(memoryStream);
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf);

                    // Add book details to PDF
                    document.Add(new Paragraph($"Title: {book.Title}").SetFontSize(16));
                    document.Add(new Paragraph($"Author: {book.Author}"));
                    document.Add(new Paragraph($"Publisher: {book.Publisher}"));
                    document.Add(new Paragraph($"Year: {book.Year}"));
                    document.Add(new Paragraph($"Genre: {book.Genre}"));
                    if (!string.IsNullOrEmpty(book.Isbn))
                    {
                        document.Add(new Paragraph($"ISBN: {book.Isbn}"));
                    }

                    // Close the document
                    document.Close();
                    pdfBytes = memoryStream.ToArray();
                }

                // Return the file for download
                return File(pdfBytes, "application/pdf", $"{bookTitle}.pdf");
            }
            catch (Exception ex)
            {
                // Log the error
                return BadRequest($"Error generating PDF: {ex.Message}");
            }
        }
        public IActionResult AddToCart(string isbn, double price, string type)
        {
            
            try
            {
                string userMail = HttpContext.Session.GetString("Mail");
                string userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userMail))
                {
                    ViewBag.ErrorMessage = "You must be logged in to purchase a book.";
                    return RedirectToAction("SignIn");
                }
               
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = "INSERT INTO tblCart (id, mail, isbn, type, price) VALUES (@id, @mail, @isbn, @type, @price)";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", userId);
                        command.Parameters.AddWithValue("@mail", userMail);
                        command.Parameters.AddWithValue("@isbn", isbn);
                        command.Parameters.AddWithValue("@type", type);
                        command.Parameters.AddWithValue("@price", price);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            ViewBag.SuccessMessage = "Book successfully added to cart.";
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "An error occurred while adding the book to the cart.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An unexpected error occurred: " + ex.Message;
            }

            return RedirectToAction("SearchBooks","Book");
        }

        public IActionResult ViewCart()
        {
            string userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                ViewBag.ErrorMessage = "You must be logged in to view your cart.";
                return RedirectToAction("SignIn");
            }

            // Query to fetch basic book details
            string sqlQuery = @"
            SELECT * 
            FROM tblBooks b 
            WHERE b.isbn IN (
                SELECT isbn 
                FROM tblCart 
                WHERE id = @userId)";

            // Fetch books from database
            List<BookModel> books = GetBooksFromDb(userId, sqlQuery);

            // Now update the books with correct type and price (Purchase/Borrow)
            foreach (var book in books)
            {
                // Fetch type and price for each book from the cart
                string cartQuery = @"
                SELECT type, price
                FROM tblCart
                WHERE id = @userId AND isbn = @isbn";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(cartQuery, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@isbn", book.Isbn);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {                                
                                book.Type = reader.GetString(0);                            
                                book.Price = (float)reader.GetDouble(1);
                                totalPrice += (int)book.Price;
                            }
                        }
                    }
                }
            }

            BooksViewModel cartViewModel = new BooksViewModel
            {
                Books = books
            };

            HttpContext.Session.SetInt32("PaymentAmount",totalPrice);
            ViewData["Amount"] = totalPrice;
            return View(cartViewModel);
        }

        [HttpGet]
        [HttpGet]
        public IActionResult Payment()
        {
            
            HttpContext.Session.Get("PaymentAmount");

            if (HttpContext.Session.Get("PaymentAmount") == null)
            {
                totalPrice = 0; 
            }
            else
            {
                totalPrice = (int)HttpContext.Session.GetInt32("PaymentAmount"); 
            }

            ViewData["Amount"] = totalPrice;

            return View();
        }


        [HttpPost]
        public IActionResult PaymentProcess(string paymentMethod, double amount)
        {
            try
            {
                if (paymentMethod == "PayPal")
                {
                    ConfirmPayment();
                    string payPalUrl = $"https://www.paypal.com/cgi-bin/webscr?cmd=_xclick&business=your-paypal-email@example.com&item_name=BookPurchase&amount={amount}&currency_code=USD&return=https://yourdomain.com/User/UserConnection&cancel_return=https://yourdomain.com/User/Payment";
                    return Redirect(payPalUrl);
                }
                else 
                {
                    // Simulate payment processing
                    // SSL Certificate ensures secure transmission
                    bool paymentSuccessful = ProcessCardPayment(amount);

                    if (paymentSuccessful)
                    {
                        ConfirmPayment();
                        TempData["PaymentMessage"] = "Payment successful!";
                        return RedirectToAction("Payment");
                    }
                    else
                    {
                        TempData["PaymentMessage"] = "Payment failed. Please try again.";
                        return RedirectToAction("Payment");
                    }
                }               
            }
            catch (Exception ex)
            {
                TempData["PaymentMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Payment");
            }
        }

        private bool ProcessCardPayment(double amount)
        {        
            return true;
        }
        [HttpPost]
        public void ConfirmPayment()
        {
            try
            {
                string userId = HttpContext.Session.GetString("UserId");
                string userMail = HttpContext.Session.GetString("Mail");              

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();                   
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        //Fetch books from tblCart
                        string fetchCartQuery = "SELECT isbn, type FROM tblCart WHERE id = @userId";
                        List<(string isbn, string type)> cartBooks = new List<(string isbn, string type)>();

                        using (SqlCommand fetchCartCommand = new SqlCommand(fetchCartQuery, connection, transaction))
                        {
                            fetchCartCommand.Parameters.AddWithValue("@userId", userId);
                            using (SqlDataReader reader = fetchCartCommand.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    cartBooks.Add((reader.GetString(0), reader.GetString(1)));
                                }
                            }
                        }

                        //Insert into tblUserBooks and update tblBooks
                        foreach (var (isbn, type) in cartBooks)
                        {
                            // Insert into tblUserBooks
                            string insertUserBooksQuery = @"
                        INSERT INTO tblUserBooks (userId, userMail, isbn, book_status)
                        VALUES (@userId, @userMail, @isbn, @bookStatus)";

                            using (SqlCommand insertCommand = new SqlCommand(insertUserBooksQuery, connection, transaction))
                            {
                                insertCommand.Parameters.AddWithValue("@userId", userId);
                                insertCommand.Parameters.AddWithValue("@userMail", userMail);
                                insertCommand.Parameters.AddWithValue("@isbn", isbn);
                                insertCommand.Parameters.AddWithValue("@bookStatus", type == "Borrow" ? "Borrowed" : "Purchased");
                                insertCommand.ExecuteNonQuery();
                            }

                            // If borrowed, reduce copies in tblBooks
                            if (type == "Borrow")
                            {
                                string updateBooksQuery = "UPDATE tblBooks SET copiesNum = copiesNum - 1 WHERE isbn = @isbn AND copiesNum > 0";
                                using (SqlCommand updateCommand = new SqlCommand(updateBooksQuery, connection, transaction))
                                {
                                    updateCommand.Parameters.AddWithValue("@isbn", isbn);
                                    int rowsAffected = updateCommand.ExecuteNonQuery();

                                    if (rowsAffected == 0)
                                    {
                                        throw new Exception($"Book with ISBN {isbn} is out of stock.");
                                    }
                                }
                            }
                        }

                        //Clear tblCart
                        string clearCartQuery = "DELETE FROM tblCart WHERE id = @userId";
                        using (SqlCommand clearCommand = new SqlCommand(clearCartQuery, connection, transaction))
                        {
                            clearCommand.Parameters.AddWithValue("@userId", userId);
                            clearCommand.ExecuteNonQuery();
                        }

                        // Commit transaction
                        transaction.Commit();
                        TempData["PaymentMessage"] = "Payment confirmed and books added successfully!";
                    }
                    catch (Exception ex)
                    {
                        // Rollback transaction in case of error
                        transaction.Rollback();
                        TempData["PaymentMessage"] = $"An error occurred: {ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["PaymentMessage"] = $"An unexpected error occurred: {ex.Message}";
            }           
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
                return View(model);
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string checkEmailQuery = "SELECT * FROM tblUser WHERE mail = @mail";
                using (SqlCommand command = new SqlCommand(checkEmailQuery, connection))
                {
                    command.Parameters.AddWithValue("@mail", model.Email);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            // Found user, redirect to reset password page
                            return RedirectToAction("ResetPassword", new { email = model.Email });
                        }
                        else
                        {
                            ViewBag.Message = "No user found with that email.";
                            return View(model);
                        }
                    }
                }
            }
        }
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "Password is required.");
                return View(model);
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string hashedPassword = Utils.PasswordHasher.Hash(model.NewPassword);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string updateQuery = "UPDATE tblUser SET password = @password WHERE mail = @mail";
                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@password", hashedPassword);
                    command.Parameters.AddWithValue("@mail", model.Email);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        ViewBag.Message = "Password has been reset successfully.";
                        return RedirectToAction("SignIn");
                    }
                    else
                    {
                        ViewBag.Message = "An error occurred. Please try again.";
                        return View(model);
                    }
                }
            }
        }


    }
}
