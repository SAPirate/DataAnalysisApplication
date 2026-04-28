using System.Drawing.Printing;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using practiceApp.Data;
using practiceApp.Models;

namespace practiceApp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _db;

        public CategoryController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(
          string searchUsername,
          string sortOrder,
          string statusFilter,
          DateTime? startDate,
          DateTime? endDate,
          int page = 1)
        {
            const int pageSize = 7;

            var categoriesQuery = _db.categoryModels.AsQueryable();

            // Preserve current filters in ViewData for pagination and UI
            ViewData["CurrentSortOrder"] = sortOrder;
            ViewData["CurrentStatusFilter"] = statusFilter;
            ViewData["CurrentSearchTerm"] = searchUsername;
            ViewData["CurrentStartDate"] = startDate;
            ViewData["CurrentEndDate"] = endDate;
            ViewData["CurrentPage"] = page;

            // Apply filters (ignore deleted filter here or only get active/restored)
            if (!string.IsNullOrWhiteSpace(searchUsername))
                categoriesQuery = categoriesQuery.Where(c => c.Username.Contains(searchUsername));

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                switch (statusFilter.ToLower())
                {
                    case "current":
                        categoriesQuery = categoriesQuery.Where(c => !c.IsDeleted && !c.WasDeleted);
                        break;
                    case "restored":
                        categoriesQuery = categoriesQuery.Where(c => !c.IsDeleted && c.WasDeleted);
                        break;
                    default:
                        // For the Index, exclude deleted categories entirely
                        categoriesQuery = categoriesQuery.Where(c => !c.IsDeleted);
                        break;
                }
            }
            else
            {
                // No status filter, default exclude deleted
                categoriesQuery = categoriesQuery.Where(c => !c.IsDeleted);
            }

            if (startDate.HasValue)
                categoriesQuery = categoriesQuery.Where(c => c.CreatedDateTime.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                categoriesQuery = categoriesQuery.Where(c => c.CreatedDateTime.Date <= endDate.Value.Date);

            // Sorting
            categoriesQuery = sortOrder switch
            {
                "asc" => categoriesQuery.OrderBy(c => c.Id),
                "desc" => categoriesQuery.OrderByDescending(c => c.Id),
                _ => categoriesQuery.OrderByDescending(c => c.Id)
            };

            int totalItems = categoriesQuery.Count();

            var pagedCategories = categoriesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new CategoryIndexViewModel
            {
                ActiveCategories = pagedCategories,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                CurrentPage = page
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CategoryTablePartial", viewModel);
            }

            return View(viewModel);
        }




        // GET: Create new category
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create new category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryModel obj)
        {
            if (obj.Username == obj.DisplayOrder.ToString())
                ModelState.AddModelError("Username", "The displayOrder cannot exactly match the username");

            bool isDuplicate = _db.categoryModels.Any(c => c.Username.ToLower() == obj.Username.ToLower() && !c.IsDeleted);
            if (isDuplicate)
                ModelState.AddModelError("Username", "A category with this username already exists.");

            if (ModelState.IsValid)
            {
                _db.categoryModels.Add(obj);
                _db.SaveChanges();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }


        // GET: Edit category
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var categoryFromDb = _db.categoryModels.Find(id);
            if (categoryFromDb == null)
                return NotFound();

            return View(categoryFromDb);
        }



        // POST: Update category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryModel obj)
        {
            if (obj.Username == obj.DisplayOrder.ToString())
                ModelState.AddModelError("Username", "The displayOrder cannot exactly match the username");

            if (ModelState.IsValid)
            {
                var existingCategory = _db.categoryModels.Find(obj.Id);
                if (existingCategory == null)
                    return NotFound();

                existingCategory.Username = obj.Username;
                existingCategory.DisplayOrder = obj.DisplayOrder;

                _db.categoryModels.Update(existingCategory);
                _db.SaveChanges();

                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }



        // GET: Confirm deletion
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var categoryFromDb = _db.categoryModels.Find(id);
            if (categoryFromDb == null || categoryFromDb.IsDeleted)
                return NotFound();

            return View(categoryFromDb);
        }

       
        
        // POST: Soft delete category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.categoryModels.Find(id);
            if (obj == null)
                return NotFound();

            obj.IsDeleted = true;
            obj.WasDeleted = true;

            _db.categoryModels.Update(obj);
            _db.SaveChanges();

            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }

        //deleted C ategories 

        public IActionResult DeletedCategories(int page = 1)
        {
            const int pageSize = 7;

            var deletedCategoriesQuery = _db.categoryModels.Where(c => c.IsDeleted);

            // You can apply filters, sorting, and pagination here similarly
            var totalItems = deletedCategoriesQuery.Count();
            var pagedDeletedCategories = deletedCategoriesQuery
                .OrderByDescending(c => c.DeletedDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new CategoryIndexViewModel
            {
                DeletedCategories = pagedDeletedCategories,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                CurrentPage = page
            };

            return View(viewModel); // separate view called Deleted.cshtml
        }

        [HttpGet]
        public IActionResult GetDeletedFilteredPartial(string search, string sort, string status, DateTime? startDate, DateTime? endDate)
        {
            var deleted = _db.categoryModels.Where(c => c.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
                deleted = deleted.Where(c => c.Username.Contains(search));

            if (status == "restored")
                deleted = deleted.Where(c => c.WasDeleted == true);

            if (startDate.HasValue)
                deleted = deleted.Where(c => c.CreatedDateTime >= startDate.Value);

            if (endDate.HasValue)
                deleted = deleted.Where(c => c.CreatedDateTime <= endDate.Value);

            if (sort == "asc")
                deleted = deleted.OrderBy(c => c.Id);
            else if (sort == "desc")
                deleted = deleted.OrderByDescending(c => c.Id);

            var model = new CategoryIndexViewModel
            {
                DeletedCategories = deleted.ToList()
            };

            return PartialView("_DeletedCategoriesTablePartial", model); // You create this partial next
        }

        // AJAX: Username search
        [HttpGet]
        public JsonResult SearchByUsername(string term)
        {
            var results = _db.categoryModels
                .Where(c => c.Username.ToLower().Contains(term.ToLower()))
                .Select(c => new
                {
                    id = c.Id,
                    username = c.Username,
                    displayOrder = c.DisplayOrder
                })
                .ToList();

            return Json(results);
        }



        // POST: Restore deleted category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restore(int id)
        {
            var obj = _db.categoryModels.Find(id);
            if (obj == null)
                return NotFound();

            obj.IsDeleted = false;
            obj.WasDeleted = true;
            obj.RestoreCount += 1;

            _db.categoryModels.Update(obj);
            _db.SaveChanges();

            TempData["success"] = "Category restored successfully";
            return Ok();
        }



        // Dashboard summary
        public IActionResult Dashboard()
        {
            var categories = _db.categoryModels.ToList();
            var deletedCategories = categories.Where(c => c.WasDeleted).ToList();
            var currentCategories = categories.Where(c => !c.WasDeleted).ToList();
            var totalRestores = categories.Sum(c => c.RestoreCount);

            var mostRestoredUser = categories
                .OrderByDescending(c => c.RestoreCount)
                .FirstOrDefault();

            var model = new DashboardViewModel
            {
                TotalCategories = categories.Count,
                DeletedCategories = deletedCategories.Count,
                CurrentCategories = currentCategories.Count,
                TotalRestores = totalRestores,
                MostRestoredUsername = mostRestoredUser?.Username ?? "N/A",
                MostRestoredCount = mostRestoredUser?.RestoreCount ?? 0
            };

            return View(model);
        }

        // Export to Excel
        [HttpPost]
        public IActionResult ExportToExcel()
        {
            var categories = GetCategoriesForExport();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Categories");

            // Add title in merged first row
            worksheet.Range("A1:F1").Merge();
            worksheet.Cell(1, 1).Value = "Records for Current Data";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Header row starts from row 2 now
            worksheet.Cell(2, 1).Value = "ID";
            worksheet.Cell(2, 2).Value = "Username";
            worksheet.Cell(2, 3).Value = "Display Order";
            worksheet.Cell(2, 4).Value = "Status";
            worksheet.Cell(2, 5).Value = "Restore Count";
            worksheet.Cell(2, 6).Value = "CreatedDate";

            int row = 3; // Data starts from row 3
            foreach (var cat in categories)
            {
                worksheet.Cell(row, 1).Value = cat.Id;
                worksheet.Cell(row, 2).Value = cat.Username;
                worksheet.Cell(row, 3).Value = cat.DisplayOrder;
                worksheet.Cell(row, 4).Value = cat.WasDeleted
                    ? (cat.RestoreCount > 1 ? $"Current (Restored {cat.RestoreCount} times)" : "Current (Restored)")
                    : "Current";
                worksheet.Cell(row, 5).Value = cat.RestoreCount;
                worksheet.Cell(row, 6).Value = cat.CreatedDateTime.ToString("yyyy-MM-dd");
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Categories.xlsx");
        }


        // Export to PDF
        [HttpPost]
        public IActionResult ExportToPdf()
        {
            var categories = GetCategoriesForExport();

            using var stream = new MemoryStream();

            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var fontRow = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Add title paragraph centered
            var titleParagraph = new Paragraph("Records for Current Data", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(titleParagraph);

            var table = new PdfPTable(6) { WidthPercentage = 100 };
            float[] widths = { 8f, 25f, 15f, 20f, 15f, 20f };
            table.SetWidths(widths);

            // Headers
            table.AddCell(new PdfPCell(new Phrase("ID", font)));
            table.AddCell(new PdfPCell(new Phrase("Username", font)));
            table.AddCell(new PdfPCell(new Phrase("Display Order", font)));
            table.AddCell(new PdfPCell(new Phrase("Status", font)));
            table.AddCell(new PdfPCell(new Phrase("Restore Count", font)));
            table.AddCell(new PdfPCell(new Phrase("CreatedDate", font)));

            foreach (var cat in categories)
            {
                table.AddCell(new PdfPCell(new Phrase(cat.Id.ToString(), fontRow)));
                table.AddCell(new PdfPCell(new Phrase(cat.Username, fontRow)));
                table.AddCell(new PdfPCell(new Phrase(cat.DisplayOrder.ToString(), fontRow)));

                string statusText = cat.WasDeleted
                    ? (cat.RestoreCount > 1 ? $"Current (Restored {cat.RestoreCount} times)" : "Current (Restored)")
                    : "Current";
                table.AddCell(new PdfPCell(new Phrase(statusText, fontRow)));

                table.AddCell(new PdfPCell(new Phrase(cat.RestoreCount.ToString(), fontRow)));
                table.AddCell(new PdfPCell(new Phrase(cat.CreatedDateTime.ToString("yyyy-MM-dd"), fontRow)));
            }

            document.Add(table);
            document.Close();

            var bytes = stream.ToArray();
            return File(bytes, "application/pdf", "Categories.pdf");
        }





        // Helper: Fetch categories for export (non-deleted)
        private List<CategoryModel> GetCategoriesForExport()
        {
            return _db.categoryModels
                      .Where(c => !c.IsDeleted)
                      .ToList();
        }

        [HttpPost]
        public IActionResult ExportDeletedToExcel()
        {
            var deletedCategories = _db.categoryModels
                .Where(c => c.IsDeleted)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Deleted Categories");

            // Add title row above headers
            worksheet.Cell(1, 1).Value = "Records for Deleted Data";
            worksheet.Range(1, 1, 1, 5).Merge(); // Merge across all header columns
            var titleCell = worksheet.Cell(1, 1);
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 16;
            titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleCell.Style.Fill.BackgroundColor = XLColor.LightGray; // Optional background

            // Header row (now on row 2)
            worksheet.Cell(2, 1).Value = "ID";
            worksheet.Cell(2, 2).Value = "Username";
            worksheet.Cell(2, 3).Value = "Display Order";
            worksheet.Cell(2, 4).Value = "Restore Count";
            worksheet.Cell(2, 5).Value = "CreatedDate";

            int row = 3; // Data starts at row 3 now
            foreach (var cat in deletedCategories)
            {
                worksheet.Cell(row, 1).Value = cat.Id;
                worksheet.Cell(row, 2).Value = cat.Username;
                worksheet.Cell(row, 3).Value = cat.DisplayOrder;
                worksheet.Cell(row, 4).Value = cat.RestoreCount;
                worksheet.Cell(row, 5).Value = cat.CreatedDateTime.ToString("yyyy-MM-dd");
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DeletedCategories.xlsx");
        }




        [HttpPost]
        public IActionResult ExportDeletedToPdf()
        {
            var deletedCategories = _db.categoryModels
                .Where(c => c.IsDeleted)
                .ToList();

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            PdfWriter.GetInstance(document, stream);
            document.Open();

            // 🔴 PDF Title/Header
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.Red);
            var title = new Paragraph("Records for Deleted Data", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(title);

            // Fonts
            var font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var fontRow = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Table setup
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 10f, 30f, 20f, 20f, 20f });

            // Headers
            table.AddCell(new PdfPCell(new Phrase("ID", font)));
            table.AddCell(new PdfPCell(new Phrase("Username", font)));
            table.AddCell(new PdfPCell(new Phrase("Display Order", font)));
            table.AddCell(new PdfPCell(new Phrase("Restore Count", font)));
            table.AddCell(new PdfPCell(new Phrase("CreatedDate", font)));

            // Rows
            foreach (var cat in deletedCategories)
            {
                table.AddCell(new PdfPCell(new Phrase(cat.Id.ToString(), fontRow)));
                table.AddCell(new PdfPCell(new Phrase(cat.Username, fontRow)));
                table.AddCell(new PdfPCell(new Phrase(cat.DisplayOrder.ToString(), fontRow)));
                table.AddCell(new PdfPCell(new Phrase(cat.RestoreCount.ToString(), fontRow)));
                table.AddCell(new PdfPCell(new Phrase(cat.CreatedDateTime.ToString("yyyy-MM-dd"), fontRow)));
            }

            document.Add(table);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "DeletedCategories.pdf");
        }


        //bulks 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BulkRestore(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["Message"] = "No categories selected for restore.";
                return RedirectToAction("DeletedCategories"); // Redirect to deleted page
            }

            var categoriesToRestore = _db.categoryModels
                .Where(c => selectedIds.Contains(c.Id) && c.IsDeleted)
                .ToList();

            if (!categoriesToRestore.Any())
            {
                TempData["Message"] = "No matching deleted categories found to restore.";
                return RedirectToAction("DeletedCategories");
            }

            foreach (var category in categoriesToRestore)
            {
                category.IsDeleted = false;
                category.WasDeleted = true;
                category.RestoreCount += 1;
                category.UpdatedDateTime = DateTime.Now;
            }

            _db.categoryModels.UpdateRange(categoriesToRestore);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"{categoriesToRestore.Count} categories successfully restored.";
            return RedirectToAction("DeletedCategories");
        }




        [HttpPost]
        public async Task<IActionResult> BulkDelete(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["Message"] = "No categories selected for deletion.";
                return RedirectToAction(nameof(Index));
            }

            var categories = _db.categoryModels.Where(c => selectedIds.Contains(c.Id)).ToList();

            foreach (var cat in categories)
            {
                cat.IsDeleted = true;           // soft delete
                cat.WasDeleted = true;          // mark that it was deleted once
                cat.DeletedDateTime = DateTime.Now;
                cat.UpdatedDateTime = DateTime.Now;
            }

            await _db.SaveChangesAsync();

            TempData["Message"] = $"Soft deleted {categories.Count} categories successfully.";
            TempData["success"] = "Categories deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAction(List<int> selectedIds, string bulkAction)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["ErrorMessage"] = "Please select at least one category.";
                return RedirectToAction("Index");
            }

            switch (bulkAction?.ToLower())
            {
                case "delete":
                    var categoriesToDelete = _db.categoryModels
                        .Where(c => selectedIds.Contains(c.Id) && !c.IsDeleted)
                        .ToList();

                    foreach (var category in categoriesToDelete)
                    {
                        category.IsDeleted = true;
                        category.WasDeleted = true;
                        category.DeletedDateTime = DateTime.Now;
                        category.UpdatedDateTime = DateTime.Now;
                    }

                    await _db.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"{categoriesToDelete.Count} categories have been soft deleted.";
                    break;

                case "edit":
                    var idsQuery = string.Join(",", selectedIds);
                    return RedirectToAction("BulkEdit", new { ids = idsQuery });

                default:
                    TempData["ErrorMessage"] = "Invalid bulk action selected.";
                    break;
            }
            TempData["success"] = "Category modified successfully";

            return RedirectToAction("Index");
        }




        // GET: Category/BulkEdit?ids=1,2,3
        public IActionResult BulkEdit(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                TempData["ErrorMessage"] = "No categories selected for editing.";
                return RedirectToAction("Index");
            }

            var idList = ids.Split(',').Select(int.Parse).ToList();

            var categories = _db.categoryModels
                .Where(c => idList.Contains(c.Id) && !c.IsDeleted)
                .ToList();

            if (!categories.Any())
            {
                TempData["ErrorMessage"] = "Selected categories not found or are deleted.";
                return RedirectToAction("Index");
            }

            return View(categories); // pass the list to the view
        }

        
        // POST: Category/BulkEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BulkEdit(List<CategoryModel> categories)
        {
            if (categories == null || !categories.Any())
            {
                TempData["ErrorMessage"] = "No categories submitted.";
                return RedirectToAction("Index");
            }

            foreach (var cat in categories)
            {
                var categoryInDb = _db.categoryModels.FirstOrDefault(c => c.Id == cat.Id);
                if (categoryInDb != null && !categoryInDb.IsDeleted)
                {
                    // Update fields that you want to allow editing in bulk
                    categoryInDb.Username = cat.Username;
                    categoryInDb.DisplayOrder = cat.DisplayOrder;
                    categoryInDb.UpdatedDateTime = DateTime.Now;
                }
            }

            _db.SaveChanges();
            TempData["SuccessMessage"] = $"{categories.Count} categories updated successfully.";

            return RedirectToAction("Index");
        }


    }
}
