using Microsoft.AspNetCore.Mvc;
using practiceApp.Data;
using practiceApp.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace practiceApp.Controllers
{
    public class UploadController : Controller
    {
        private readonly AppDbContext _db;

        public UploadController(AppDbContext db)
        {
            _db = db;
        }

        // GET: Upload
        public IActionResult Index(string searchEmail, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _db.DataUploads.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchEmail))
                query = query.Where(x => x.SentToEmail.Contains(searchEmail));

            if (dateFrom.HasValue)
                query = query.Where(x => x.UploadedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(x => x.UploadedAt < dateTo.Value.AddDays(1)); // Include full day

            var model = new CombinedUploads
            {
                UploadForm = new FileUploadViewModel(),
                UploadsList = new DataUploadListModel
                {
                    DataUploads = query.OrderByDescending(x => x.UploadedAt).ToList(),
                    SearchEmail = searchEmail,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                }
            };

            return View(model);
        }

        // POST: Upload (Handle file uploads)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CombinedUploads model)
        {
            // Check if model is valid (also print any validation errors for debugging)
            if (ModelState.IsValid)
            {
                if (model.UploadForm.File != null && model.UploadForm.File.Length > 0)
                {
                    // Log the file type and length to see if it's being received properly
                    Console.WriteLine($"Uploading File: {model.UploadForm.File.FileName}, Size: {model.UploadForm.File.Length}");

                    // Check if file type is valid
                    var allowedFileTypes = new[] { "image/jpeg", "image/png", "application/pdf", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" };

                    if (!allowedFileTypes.Contains(model.UploadForm.File.ContentType))
                    {
                        ModelState.AddModelError("", "Invalid file type.");
                        return View(model);
                    }

                    using var ms = new MemoryStream();
                    await model.UploadForm.File.CopyToAsync(ms);

                    var upload = new DataUpload
                    {
                        FileName = model.UploadForm.File.FileName,
                        ContentType = model.UploadForm.File.ContentType,
                        Data = ms.ToArray(),
                        SentToEmail = model.UploadForm.SentToEmail,
                        UploadedAt = DateTime.Now,
                        UploadedBy = User.Identity?.Name ?? "Anonymous"
                    };

                    _db.DataUploads.Add(upload);
                    await _db.SaveChangesAsync();

                    TempData["SuccessMessage"] = "File uploaded successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Please select a file.");
            }

            // Repopulate uploads if form fails
            var query = _db.DataUploads.AsQueryable();
            model.UploadsList = new DataUploadListModel
            {
                DataUploads = query.OrderByDescending(x => x.UploadedAt).ToList()
            };

            return View(model);
        }


        // GET: Upload/Download/5
        public IActionResult Download(int id)
        {
            var file = _db.DataUploads.Find(id);
            if (file == null) return NotFound();

            return File(file.Data, file.ContentType, file.FileName);
        }
    }
}
