﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using UniversityOfRoommates.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace UniversityOfRoommates.Controllers
{
    public class FotoCasasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FotoCasas
        public async Task<ActionResult> Index()
        {
            var fotoCase = db.FotoCase.Include(f => f.Casa);
            return View(await fotoCase.ToListAsync());
        }

        // GET: FotoCasas/Details/5
        public async Task<ActionResult> Details(string nomeCasa, double lon, double lat, int id)
        {
            if (nomeCasa=="")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FotoCasa fotoCasa = await db.FotoCase.FindAsync(nomeCasa, lon, lat, id);
            if (fotoCasa == null)
            {
                return HttpNotFound();
            }
            return View(fotoCasa);
        }

        // GET: FotoCasas/Create
        public ActionResult Create(string nomeCasa, double lon, double lat)
        {
            string lon1 = lon.ToString();
            string lat1 = lat.ToString();
            int id = 0;
            List<FotoCasa> list = db.Case.Where(m => m.nomeCasa == nomeCasa && m.longitudine == lon && m.latitudine == lat).First().FotoCasa.ToList();
            foreach (FotoCasa fc in list)
            {
                id++;
            }
            ViewBag.nomeCasa = nomeCasa;
            ViewBag.lon = lon1;
            ViewBag.lat = lat1;
            ViewBag.id = id;
            return View();
        }

        // POST: FotoCasas/Create
        // Per proteggere da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per ulteriori dettagli, vedere http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "nomeCasa,longitude,latitude,idFoto,linkFoto")] FotoCasa fotoCasa)
        {
           // ModelState.Values[1].Value.Att = Convert.ToDouble(fotoCasa.longitude.ToString().Replace('.', ','));
            //fotoCasa.latitude = Convert.ToDouble(fotoCasa.latitude.ToString().Replace('.', ','));
            if (ModelState.IsValid)
            {
                var result = UploadBlob(fotoCasa.nomeCasa, fotoCasa.longitude, fotoCasa.latitude, fotoCasa.idFoto, fotoCasa.linkFoto);
                db.FotoCase.Add(fotoCasa);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.nomeCasa = new SelectList(db.Case, "nomeCasa", "UserName", fotoCasa.nomeCasa);
            return View(fotoCasa);
        }

        // GET: FotoCasas/Edit/5
        public async Task<ActionResult> Edit(string nomeCasa, double lon, double lat, int id)
        {
            if (nomeCasa=="")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FotoCasa fotoCasa = await db.FotoCase.FindAsync(nomeCasa, lon, lat, id);
            if (fotoCasa == null)
            {
                return HttpNotFound();
            }
            ViewBag.nomeCasa = new SelectList(db.Case, "nomeCasa", "UserName", fotoCasa.nomeCasa);
            return View(fotoCasa);
        }

        // POST: FotoCasas/Edit/5
        // Per proteggere da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per ulteriori dettagli, vedere http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "nomeCasa,longitude,latitude,idFoto,linkFoto")] FotoCasa fotoCasa)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fotoCasa).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.nomeCasa = new SelectList(db.Case, "nomeCasa", "UserName", fotoCasa.nomeCasa);
            return View(fotoCasa);
        }

        // GET: FotoCasas/Delete/5
        public async Task<ActionResult> Delete(string nomeCasa, double lon, double lat, int id)
        {
            if (nomeCasa=="")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FotoCasa fotoCasa = await db.FotoCase.FindAsync(nomeCasa, lon, lat, id);
            if (fotoCasa == null)
            {
                return HttpNotFound();
            }
            return View(fotoCasa);
        }

        // POST: FotoCasas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string nomeCasa, double lon, double lat, int id)
        {
            FotoCasa fotoCasa = await db.FotoCase.FindAsync(nomeCasa, lon, lat, id);
            db.FotoCase.Remove(fotoCasa);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public EmptyResult UploadBlob(string nomeCasa, double lon, double lat, int id , string path)
        {
            // The code in this section goes here.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("universityofroommates_AzureStorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("fotocasa");
            string blobName = nomeCasa + lon.ToString() + lat.ToString() + id.ToString();
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            using (var fileStream = System.IO.File.OpenRead(path))
            {
                blob.UploadFromStream(fileStream);
            }
            return new EmptyResult();
        }

        public EmptyResult DownloadBlob(string nomeCasa, double lon, double lat, int id)
        {
            // The code in this section goes here.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("universityofroommates_AzureStorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("fotocasa");
            string blobName = nomeCasa + lon.ToString() + lat.ToString() + id.ToString();
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            using (var fileStream = System.IO.File.OpenWrite("../Content/imgCase"))
            {
                blob.DownloadToStream(fileStream);
            }
            return new EmptyResult();
        }
    }
}