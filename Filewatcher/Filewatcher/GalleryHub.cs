using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Filewatcher
{
    public class GalleryHub : Hub
    {
        public static readonly string FolderPath = ConfigurationManager.AppSettings["imagepath"];
        public static Vechicle objectVechicle = new Vechicle();

        static GalleryHub()
        {
            gallerywatcher();
        }

        public void GetImagesList()
        {          
            string[] imgfiles = Directory.GetFiles(FolderPath, "*.jpg", SearchOption.TopDirectoryOnly);
            string[] images = (from n in imgfiles select Path.GetFileName(n) ).ToArray();
            Clients.Caller.LoadGallery(images);
        }
        public void GetImage(string name)
        {
            string imagePath = Path.Combine(FolderPath, name);
            Clients.Caller.LoadGallery(imagePath);
        }
        public static void UpdatePages(string name)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<GalleryHub>();        
            string imagePath = Path.Combine(FolderPath, name);
            invokeVehicleApi(imagePath);
            context.Clients.All.LoadGallery(objectVechicle);
        }

        private static void invokeVehicleApi(string imagePath)
        {
            try
            {
                Task<string> recognizeTask = Task.Run(() => ProcessImage(imagePath));
                recognizeTask.Wait();
                string task_result = recognizeTask.Result;
                dynamic jsonResult = JsonConvert.DeserializeObject(task_result);


                if (!string.IsNullOrWhiteSpace(objectVechicle.Number))
                {
                    objectVechicle.previous.Add(new PreviousVechicle
                    {
                        Number = objectVechicle.Number,
                        Make = objectVechicle.Make,
                        Model = objectVechicle.Model,
                        Color = objectVechicle.Color,
                        Year = objectVechicle.Year,
                        Alerts = objectVechicle.Alerts,
                        imgPath = objectVechicle.imgPath,
                        imgName = objectVechicle.imgName,
                    });
                }
                foreach (JProperty app in jsonResult)
                {
                    if (app.Name == "results")
                    {
                        objectVechicle.Number = app.Value[0]["plate"].ToString();
                    }
                }

                objectVechicle.Make = "BMW";
                objectVechicle.Model = "Z4";
                objectVechicle.Color = "Gray";
                objectVechicle.Year = "2018";
                objectVechicle.Alerts = "New Car";
                objectVechicle.imgPath = imagePath;
                objectVechicle.imgName = Path.GetFileName(imagePath);
            }
            catch (Exception ex) { var aa = ex; };
        }
        private static readonly HttpClient client = new HttpClient();
        public static async Task<string> ProcessImage(string image_path)
        {
            string SECRET_KEY = "sk_f9a2f3823b876e7b75a4ca6f";

            Byte[] bytes = System.IO.File.ReadAllBytes(image_path);
            string imagebase64 = Convert.ToBase64String(bytes);

            var content = new StringContent(imagebase64);

            var response = await client.PostAsync("https://api.openalpr.com/v2/recognize_bytes?recognize_vehicle=1&country=us&secret_key=" + SECRET_KEY, content).ConfigureAwait(false);

            var buffer = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var byteArray = buffer.ToArray();
            var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);

            return responseString;
        }

        static private void gallerywatcher()
        {
            FileSystemWatcher Watcher = new FileSystemWatcher(FolderPath);
            Watcher.EnableRaisingEvents = true;
            Watcher.IncludeSubdirectories = false;
            //Watcher.Changed += GalleryChanged;
            Watcher.Created += GalleryCreated;
            //Watcher.Deleted += GalleryDeleted;
        }
        static void GalleryChanged(Object sender,FileSystemEventArgs e)
        {
            UpdatePages(e.Name);
        }
        static void GalleryCreated(Object sender, FileSystemEventArgs e)
        {
            UpdatePages(e.Name);
        }
        static void GalleryDeleted(Object sender, FileSystemEventArgs e)
        {
            UpdatePages(e.Name);
        }

    }
    public class Vechicle
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string Color { get; set; }
        public string Alerts { get; set; }
        public string imgPath { get; set; }
        public string imgName { get; set; }
        public List<PreviousVechicle> previous = new List<PreviousVechicle>();

    }
    public class PreviousVechicle
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string Color { get; set; }
        public string Alerts { get; set; }
        public string imgPath { get; set; }
        public string imgName { get; set; }

    }
}