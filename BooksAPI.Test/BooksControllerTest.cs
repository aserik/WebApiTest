using BooksAPI.DTOs;
using BooksAPI.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BooksAPI.Test
{
    [TestClass]
    public class BooksControllerTest

    {
        private BooksAPIContext db = new BooksAPIContext();

        private int GetMaxBookExternalId()
        {
            return db.Books.Max(b => b.ExternalId);
        }

        private BookDetailDto addNewBookWithAllFields()
        {
            int bookId = GetMaxBookExternalId() + 1;
            BookDetailDto newBook = new BookDetailDto
            {
                Id = bookId,
                Author = "Ralls, Kim",
                Title = "Integration testing",
                Price = 100.1m,
                Genre = "Genre",
                Description = "Description",
                PublishDate = DateTime.Now
            };
            db.Books.Add(newBook.ToModel());
            db.SaveChanges();
            return newBook;
        }

        private BookDetailDto getDetailBookByExternalId(int bookId)
        {
            try
            {
                return (from b in db.Books
                        join a in db.Authors on b.AuthorId equals a.AuthorId
                        where b.ExternalId == bookId
                        select new BookDetailDto
                        {
                            Id = b.ExternalId,
                            Title = b.Title,
                            Genre = b.Genre,
                            PublishDate = b.PublishDate,
                            Price = b.Price,
                            Description = b.Description,
                            Author = b.Author.Name
                        }).First();
            }
            catch (Exception E) { return null; }
        }

        private int GetBooksCount()
        {
            return db.Books.Count();
        }

        private BookDto BookDetailToBook(BookDetailDto book)
        {
            return new BookDto { Id = book.Id, Author = book.Author, Genre = book.Genre, Title = book.Title };
        }


        private HttpClient NewHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost/libraryAPI/api");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        [TestMethod]
        public async Task GetBooks_ShouldReturnAllBooks_IfBookIdIsNotProvided()
        {
                using (var client = NewHttpClient())
                {
                    var resp = await client.GetAsync("api/books");
                    resp.EnsureSuccessStatusCode();
                    var books = await resp.Content.ReadAsAsync<List<BookDto>>();
                    Assert.IsTrue(books.Count > 0);
                }
           
        }

        //[TestMethod]
        //public async Task GetBook_ShouldReturnOneBook_IfBookIdValid()
        //{
        //    using (var client = NewHttpClient())
        //    {
        //        var resp = await client.GetAsync("api/books/1");
        //        resp.EnsureSuccessStatusCode();
        //        var book = await resp.Content.ReadAsAsync<BookDto>();
        //        Assert.AreEqual("Midnight Rain", book.Title);
        //    }
        //}


        [TestMethod]
        public async Task GetBook_ShouldReturnOneBook_IfBookIdValid()
        {

            var expectedBook = addNewBookWithAllFields();
            using (var client = NewHttpClient())
            {
                var resp = await client.GetAsync($"api/books/{expectedBook.Id}" );
                resp.EnsureSuccessStatusCode();
                var actualBook = await resp.Content.ReadAsAsync<BookDto>();
                Assert.AreEqual(expectedBook.Title, actualBook.Title);
                Assert.AreEqual(expectedBook.Id, actualBook.Id);
                Assert.AreEqual(expectedBook.Author, actualBook.Author);
            }
        }


        //[TestMethod]
        //public async Task GetBook_ShouldReturnDetailsOneBook_IfBookIdValid()
        //{

        //    var expectedBook = addNewBookWithAllFields();
        //    int bookID = addNewBookWithAllFields().Id;
        //    using (var client = NewHttpClient())
        //    {
        //        var resp = await client.GetAsync("api/books/" + bookID.ToString() + "/details" );
        //        resp.EnsureSuccessStatusCode();
        //        var actualBook = await resp.Content.ReadAsAsync<BookDto>();
        //        Assert.AreEqual(expectedBook.Title, actualBook.Title);
        //        Assert.AreEqual(expectedBook.Id, actualBook.Id);
        //        Assert.AreEqual(expectedBook.Author, actualBook.Author);
        //        Assert.AreEqual(expectedBook.Genre, actualBook.Genre);
        //    }
        //}


        public async Task GetBook_ShouldReturnOneBook_IfBookIdInvalid()
        {
            int bookID = GetMaxBookExternalId() + 1000;
            using (var client = NewHttpClient())
            {
                var resp = await client.GetAsync("api/books/" + bookID.ToString());
                Assert.AreEqual(resp.StatusCode, System.Net.HttpStatusCode.NotFound);
            }
        }



        [TestMethod]
        public async Task PostBook_ShouldCreateNewBookAndRetutnItsDto()
        {
            var book = new BookDetailDto { Id = 105, Author = "Ralls, Kim", Title = "Integration testing", Price = 100.1m, PublishDate = DateTime.Now };

            using (var client = NewHttpClient())
            {
                var resp = await client.PostAsJsonAsync("api/books", book);
                resp.EnsureSuccessStatusCode();
                var result = await resp.Content.ReadAsAsync<BookDetailDto>();

                Assert.AreEqual(result.Title, book.Title);
            }
        }

        [TestMethod]
        public async Task PutBook_ShouldUpdateBook_IfBookExistsInDatabase()
        {
            var book = new BookDetailDto { Id = 103, Author = "Ralls, Kim", Title = "New testing", Price = 100.1m, PublishDate = DateTime.Now };

            using (var client = NewHttpClient())
            {
                var resp = await client.PutAsJsonAsync("api/books/103", book);
                resp.EnsureSuccessStatusCode();

            }
        }

        [TestMethod]
        public async Task DeleteBook_ShouldDelBook_ifIDValid()
        {
            int bookID = addNewBookWithAllFields().Id;
            int countBooksBeforeTest = GetBooksCount();

            using (var client = NewHttpClient())
            {
              var resp = await client.DeleteAsync("api/books/" + bookID.ToString());
              resp.EnsureSuccessStatusCode();

              Assert.AreEqual(countBooksBeforeTest - 1, GetBooksCount());
              Assert.AreEqual(null, getDetailBookByExternalId(bookID)); 
             }
        }

        [TestMethod]
        public async Task DeleteBook_ShouldException_ifIDinvalid()
        {
            int bookID = GetMaxBookExternalId() + 1;
            using (var client = NewHttpClient())
            {
                var resp = await client.DeleteAsync("api/books/" + bookID.ToString());
                Assert.AreEqual(resp.StatusCode, System.Net.HttpStatusCode.NotFound);
            }
        }


        //[TestMethod]
        //public async Task GetBooks_ShouldRetutnAllBooks_ViaInMemoryHttpServer()
        //{
        //    using (var config = new HttpConfiguration())
        //    {
        //        WebApiConfig.Register(config);
        //        config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        //        var controller = typeof(BooksController);
        //        using (var server = new HttpServer(config))
        //        {
        //            var count = Enumerable.Range(0, 560);
        //            foreach (var attempt in count)
        //            {
        //                var client = new HttpClient(server);
        //                client.BaseAddress = new Uri("http://localhost/api/");
        //                client.DefaultRequestHeaders.Accept.Clear();
        //                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //                var resp = await client.GetAsync("books");
        //                resp.EnsureSuccessStatusCode();
        //                var books = await resp.Content.ReadAsAsync<List<BookDto>>();
        //                Assert.IsTrue(books.Count > 0);
        //            }

        //        }
        //    }
        //}

        //[TestMethod]
        //public void SelfHostingServiceTest()
        //{
        //    var config = new HttpSelfHostConfiguration("http://localhost:3000");

        //    var controllerType = typeof(Controllers.BooksController);
        //    config.Routes.MapHttpRoute("default", "api/{controller}/{id}", 
        //        new { id = RouteParameter.Optional });
        //    var server = new HttpSelfHostServer(config);
        //    server.OpenAsync().Wait();

        //    var client = new HttpClient(server);
        //    var response = client.GetAsync("http://localhost:3000/api/Books").Result;
        //    response.EnsureSuccessStatusCode();
        //    var books = response.Content.ReadAsAsync<List<BookDto>>().Result;
        //    Assert.IsTrue(books.Count > 0);
        //}
    }
}
