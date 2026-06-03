using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KTGK_LapTrinhWeb.Data;
using KTGK_LapTrinhWeb.Models;

namespace KTGK_LapTrinhWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            if (page < 1) page = 1;
            const int pageSize = 5;

            var query = _context.Courses.Include(c => c.Category).AsQueryable();

            // 1. Tìm kiếm theo tên học phần
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(term));
            }

            var totalCourses = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCourses / (double)pageSize);

            if (page > totalPages && totalPages > 0) page = totalPages;

            var courses = await query
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 2. Lấy danh sách các ID học phần mà sinh viên đăng nhập hiện tại đã đăng ký
            var enrolledCourseIds = new List<int>();
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("STUDENT"))
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    enrolledCourseIds = await _context.Enrollments
                        .Where(e => e.UserId == userId)
                        .Select(e => e.CourseId)
                        .ToListAsync();
                }
            }

            var viewModel = new HomeViewModel
            {
                Courses = courses,
                SearchTerm = searchTerm ?? string.Empty,
                CurrentPage = page,
                TotalPages = totalPages,
                EnrolledCourseIds = enrolledCourseIds
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
