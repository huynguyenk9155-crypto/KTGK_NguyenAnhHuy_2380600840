using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KTGK_LapTrinhWeb.Data;
using KTGK_LapTrinhWeb.Models;

namespace KTGK_LapTrinhWeb.Controllers
{
    [Authorize(Roles = "STUDENT")]
    [Route("enroll")]
    public class EnrollController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EnrollController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: /enroll/Enroll
        [HttpPost("Enroll")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId, string? returnUrl = null)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            // Kiểm tra xem học phần có tồn tại hay không
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
            if (!courseExists) return NotFound();

            // Kiểm tra xem đã đăng ký chưa
            var isEnrolled = await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
            if (!isEnrolled)
            {
                var enrollment = new Enrollment
                {
                    UserId = userId,
                    CourseId = courseId,
                    EnrollDate = DateTime.Now
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(MyCourses));
        }

        // POST: /enroll/Unenroll
        [HttpPost("Unenroll")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(int courseId, string? returnUrl = null)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            // Tìm bản ghi đăng ký
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(MyCourses));
        }

        // GET: /enroll/mycourses
        [HttpGet("mycourses")]
        public async Task<IActionResult> MyCourses()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            // Lấy danh sách các đăng ký của sinh viên hiện tại kèm theo Course và Category
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c!.Category)
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.EnrollDate)
                .ToListAsync();

            return View(enrollments);
        }
    }
}
