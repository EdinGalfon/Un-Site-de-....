﻿using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity.Validation;
using System.Net;
using System.Data.Entity;

namespace SiteInProgress.Models
{
    public class ImageGalleryController : Controller
    {

       
        public ActionResult GalleryCategories()
        {
            using(Entities dc = new Entities())
            {
                GalleryUploadList all = new GalleryUploadList();
                all.Categories = dc.Categories.OrderBy(x => x.CategoryName).ToList();
                all.ImageGalleries = dc.ImageGalleries.ToList();
                return View(all);
            }
        }
        public ActionResult GalleryList(int id )
        {
            
            using (Entities dc = new Entities())
            {
                List<ImageGallery> all = dc.ImageGalleries.ToList();
                List<ImageGallery> needed = all.Where(x => x.CategoryId == id).ToList();
                Category category = dc.Categories.Find(id);
                GalleryViewList GL = new GalleryViewList();
                GL.Category = category;
                GL.ImageGalleries = needed;

                return View(GL);
            }
            

        }
       
        public ActionResult GalleryUpload()
        {
           using(Entities dc = new Entities())
            {
                GalleryUploadView GV = new GalleryUploadView();
                GV.Categories = dc.Categories.ToList();
                GV.ImageGallery = new ImageGallery();

                return View(GV);


            }
            
        }

        [HttpPost]
        public ActionResult GalleryUpload(GalleryUploadView GV)
        {

            //checking the requirements
            if (GV.ImageGallery.File == null)
            {
                ModelState.AddModelError("CustomError", "Please select picture");
            }
            if (GV.ImageGallery.File.ContentLength > (15 * 1024 * 1024))
            {
                ModelState.AddModelError("CustomError", "Picture size must be less than 15 MB");
                return View();
            }
            if (!(GV.ImageGallery.File.ContentType == "image/jpeg" || GV.ImageGallery.File.ContentType == "image/gif"|| GV.ImageGallery.File.ContentType == "image/img"|| GV.ImageGallery.File.ContentType == "image/raw"|| GV.ImageGallery.File.ContentType == "image/tif"))
            {
                ModelState.AddModelError("CustomError", "Picture type allowed : jpeg/gif/raw/tif");
                return View();
            }
            
            //adding file info
            GV.ImageGallery.FileName = GV.ImageGallery.File.FileName;
            GV.ImageGallery.FileSize = GV.ImageGallery.File.ContentLength;
            byte[] data = new byte[GV.ImageGallery.File.ContentLength];
            GV.ImageGallery.File.InputStream.Read(data, 0, GV.ImageGallery.File.ContentLength);
            GV.ImageGallery.FileData = data;

            //adding user post
            if (User.Identity.GetUserId() != null)

            {
                GV.ImageGallery.UserID = User.Identity.GetUserId();
                

            }
            else
            {
               return Redirect("~/Account/Login");
            }
            //adding date of post
            GV.ImageGallery.DateOfPosting = DateTime.Now;
            //adding other info
            GV.ImageGallery.City = GV.ImageGallery.City;
            GV.ImageGallery.Tel = GV.ImageGallery.Tel;
            GV.ImageGallery.Info = GV.ImageGallery.Info;



            using (Entities dc = new Entities())
            {
                
                
                //listing database
                GV.Categories = dc.Categories.ToList();
                dc.ImageGalleries.Add(GV.ImageGallery);
                //changing category count
                Category CurrentCategory = dc.Categories.Find(GV.ImageGallery.CategoryId);
                CurrentCategory.CategoryCount = +1;
                dc.Entry(CurrentCategory).State = EntityState.Modified;
                //saving to base
                dc.SaveChanges();
                
                

                
            }
            //after post info
            ModelState.AddModelError("CustomError", "Uploading was fully successful!");
            return View(GV);
        }
        
        public ActionResult GalleryEdit(int id)
        {
            
            
            using(Entities dc = new Entities())
            {
                //linking the image 
                ImageGallery image = dc.ImageGalleries.Find(id);
                GalleryUploadView GV = new GalleryUploadView();
                //checking for invalid image
                if (id < 0 )
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                if(image.UserID != User.Identity.GetUserId() && User.IsInRole("Admin")!= true)
                {
                    return RedirectToAction("Authorize", "Home");
                }
                
                //pushing view for image
                GV.ImageGallery = image;
                GV.Categories = dc.Categories.ToList();
                //updating category count
                Category CurrentCategory = dc.Categories.Find(image.CategoryId);
                
                dc.Entry(CurrentCategory).State = EntityState.Modified;
                dc.SaveChanges();
                //returning to edit page
                return View(GV);
            }
            
        }

        [HttpPost]
        public ActionResult GalleryEdit(GalleryUploadView GV, string submit)
        {
            if (ModelState.IsValid)
            {
                if(GV.ImageGallery.UserID == User.Identity.GetUserId()|| User.IsInRole("Admin"))
                {
                    if(submit.Equals("Save")&&submit.Equals("Delete The Post")!= true)
                    using (Entities dc = new Entities())
                    {
                        Category CurrentCategory = dc.Categories.Find(GV.ImageGallery.CategoryId);
                            CurrentCategory.CategoryCount =+1;
                        dc.Entry(CurrentCategory).State = EntityState.Modified;
                        dc.Entry(GV.ImageGallery).State = EntityState.Modified;
                        dc.SaveChanges();
                        return RedirectToAction("GalleryList");
                    }
                    else if (submit.Equals("Save")!= true && submit.Equals("Delete The Post"))
                    {
                        return RedirectToAction("GalleryDelete","ImageGallery", new {id = GV.ImageGallery.FileID} );
                    }
                }
                
                else
                {
                    return RedirectToAction("Authorize", "Home");
                }
            }
            
            return RedirectToAction("GalleryList");
        }
        
        public ActionResult GalleryDetails(int id)
        {
            using(Entities dc = new Entities())
            {
                GalleryUploadView GV = new GalleryUploadView();
                GV.Categories = dc.Categories.ToList();
                GV.ImageGallery = dc.ImageGalleries.Find(id);

                return View(GV);
            }
        }

        public ActionResult GalleryUserLatestAdverts()
        {
            using(Entities dc = new Entities())
            {
                List<ImageGallery> GL = new List<ImageGallery>();
                List<ImageGallery> all = dc.ImageGalleries.ToList();
                GL = all.Where(x => x.UserID == User.Identity.GetUserId()).ToList();
                return View(GL);
            }
        }

        
        public ActionResult GalleryDelete(int id)
        {
            using(Entities dc = new Entities())
            {
                ImageGallery IG = dc.ImageGalleries.Find(id);
                dc.ImageGalleries.Remove(IG);
                dc.SaveChanges();

                return RedirectToAction("GalleryUserLatestAdverts");
            }

            
        }
    }
}
