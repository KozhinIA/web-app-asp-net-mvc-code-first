﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppAspNetMvcCodeFirst.Models;

namespace WebAppAspNetMvcCodeFirst.Controllers
{
    public class BooksController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var db = new LibraryContext();
            var books = db.Books.ToList();

            return View(books);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var book = new Book();
            return View(book);
        }

        [HttpPost]
        public ActionResult Create(Book model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var db = new LibraryContext();
            model.CreateAt = DateTime.Now;

            if (model.BookImageFile != null)
            {
                var data = new byte[model.BookImageFile.ContentLength];
                model.BookImageFile.InputStream.Read(data, 0, model.BookImageFile.ContentLength);

                model.BookImage = new BookImage()
                {
                    Guid = Guid.NewGuid(),
                    DateChanged = DateTime.Now,
                    Data = data,
                    ContentType = model.BookImageFile.ContentType,
                    FileName = model.BookImageFile.FileName
                };
            }

            if(model.AuthorIds != null && model.AuthorIds.Any())
            {
                var author = db.Authors.Where(s => model.AuthorIds.Contains(s.Id)).ToList();
                model.Authors = author;
            }

            db.Books.Add(model);
            db.SaveChanges();

            return RedirectPermanent("/Books/Index");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var db = new LibraryContext();
            var book = db.Books.FirstOrDefault(x => x.Id == id);
            if (book == null)
                return RedirectPermanent("/Books/Index");

            db.Books.Remove(book);
            db.SaveChanges();

            return RedirectPermanent("/Books/Index");
        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
            var db = new LibraryContext();
            var book = db.Books.FirstOrDefault(x => x.Id == id);
            if (book == null)
                return RedirectPermanent("/Books/Index");

            return View(book);
        }

        [HttpPost]
        public ActionResult Edit(Book model)
        {
            var db = new LibraryContext();
            var book = db.Books.FirstOrDefault(x => x.Id == model.Id);
            if (book == null)
                ModelState.AddModelError("Id", "Книга не найдена");

            if (!ModelState.IsValid)
                return View(model);

            MappingBook(model, book, db);

            db.Entry(book).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectPermanent("/Books/Index");
        }

        private void MappingBook(Book sourse, Book destination, LibraryContext db)
        {
            destination.Name = sourse.Name;
            destination.Isbn = sourse.Isbn;
            destination.Year = sourse.Year;
            destination.Cost = sourse.Cost;
            destination.GenreId = sourse.GenreId;
            destination.Genre = sourse.Genre;


            if (destination.Authors != null)
                destination.Authors.Clear();

            if (sourse.AuthorIds != null && sourse.AuthorIds.Any())
                destination.Authors = db.Authors.Where(s => sourse.AuthorIds.Contains(s.Id)).ToList();



            if (sourse.BookImageFile != null)
            {
                var image = db.BookImages.FirstOrDefault(x => x.Id == sourse.Id);
                db.BookImages.Remove(image);

                var data = new byte[sourse.BookImageFile.ContentLength];
                sourse.BookImageFile.InputStream.Read(data, 0, sourse.BookImageFile.ContentLength);

                destination.BookImage = new BookImage()
                {
                    Guid = Guid.NewGuid(),
                    DateChanged = DateTime.Now,
                    Data = data,
                    ContentType = sourse.BookImageFile.ContentType,
                    FileName = sourse.BookImageFile.FileName
                };
            }
        }

        [HttpGet]
        public ActionResult GetImage(int id)
        {
            var db = new LibraryContext();
            var image = db.BookImages.FirstOrDefault(x => x.Id == id);
            if (image == null)
            {
                FileStream fs = System.IO.File.OpenRead(Server.MapPath(@"~/Content/Images/not-foto.png"));
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                fs.Close();

                return File(new MemoryStream(fileData), "image/jpeg");
            }

            return File(new MemoryStream(image.Data), image.ContentType);
        }
    }
}