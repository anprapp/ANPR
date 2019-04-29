using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Filewatcher.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public static readonly string FolderPath = ConfigurationManager.AppSettings["imagepath"];
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult getimageslist()
        {
            //string[] imgfiles = Directory.GetFiles(FolderPath, "*.jpg", SearchOption.TopDirectoryOnly);
            string[] imgfiles = Directory.EnumerateFiles(FolderPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".jpeg") || s.EndsWith(".jpg")).ToArray();
            string images = (from n in imgfiles select Url.Action("getimages", "home", new { filename = Path.GetFileName(n) })).FirstOrDefault();

            
            return this.Json(images, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getimages(string filename)
        {
            if (Path.GetExtension(filename) == ".jpg" || Path.GetExtension(filename) == ".jpeg")
            {
                var path = Path.Combine(FolderPath, filename);
                return base.File(path, "image");
            }
            return null;
                
        }
       
    }

}