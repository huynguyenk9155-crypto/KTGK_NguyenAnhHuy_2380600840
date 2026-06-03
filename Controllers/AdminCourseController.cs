using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KTGK_LapTrinhWeb.Data;
using KTGK_LapTrinhWeb.Models;

namespace KTGK_LapTrinhWeb.Controllers
{
    [Authorize(Roles = "ADMIN")]
    [Route("admin/courses")]
    public class AdminCourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminCourseController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /admin/courses
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses.Include(c => c.Category).OrderBy(c => c.Id).ToListAsync();
            return View(courses);
        }

        // GET: /admin/courses/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: /admin/courses/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Credits,Lecturer,CategoryId")] Course course, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Tạo thư mục images nếu chưa tồn tại
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    course.Image = "/images/" + uniqueFileName;
                }
                else
                {
                    // Đặt ảnh mặc định nếu không upload
                    course.Image = "/images/placeholder.jpg";
                }

                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // GET: /admin/courses/edit/{id}
        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // POST: /admin/courses/edit/{id}
        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Credits,Lecturer,CategoryId,Image")] Course course, IFormFile? imageFile)
        {
            if (id != course.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Upload ảnh mới
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // Xóa ảnh cũ nếu nó không phải là ảnh mặc định
                        if (!string.IsNullOrEmpty(course.Image) && course.Image.StartsWith("/images/") && !course.Image.Contains("placeholder.jpg"))
                        {
                            var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, course.Image.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        course.Image = "/images/" + uniqueFileName;
                    }

                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // GET: /admin/courses/delete/{id}
        [HttpGet("delete/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null) return NotFound();

            return View(course);
        }

        // POST: /admin/courses/delete/{id}
        [HttpPost("delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                // Xóa ảnh học phần trên đĩa nếu không phải placeholder
                if (!string.IsNullOrEmpty(course.Image) && course.Image.StartsWith("/images/") && !course.Image.Contains("placeholder.jpg"))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, course.Image.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
