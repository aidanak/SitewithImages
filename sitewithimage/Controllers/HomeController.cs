using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using sitewithimage.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace sitewithimage.Controllers
{
    public class HomeController : Controller
    {
        ImageService imageService = new ImageService();
        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Upload(HttpPostedFileBase file)
        {
            var model = new UploadImage();
            if (Request != null)
            {
                file = Request.Files["uploadedFile"];
                model = await imageService.CreateUploadedImage(file);
                await imageService.AddImageToBlobStorageAsync(model);
            }
            return View("Index");
        }
        [HttpGet]
        public ActionResult All_Images()
        {
            List<BaseImage> list = imageService.RetrieveFromTable();
            return View(list);
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}