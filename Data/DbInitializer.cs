using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KTGK_LapTrinhWeb.Models;

namespace KTGK_LapTrinhWeb.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Tự động chạy Migration để tạo database nếu chưa có
            await context.Database.MigrateAsync();

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Seed Roles
            string[] roleNames = { "ADMIN", "STUDENT" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Admin User
            var adminEmail = "admin@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var createAdmin = await userManager.CreateAsync(adminUser, "Admin@123");
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "ADMIN");
                }
            }

            // 3. Seed Student User
            var studentEmail = "student@gmail.com";
            var studentUser = await userManager.FindByEmailAsync(studentEmail);
            if (studentUser == null)
            {
                studentUser = new IdentityUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    EmailConfirmed = true
                };
                var createStudent = await userManager.CreateAsync(studentUser, "Student@123");
                if (createStudent.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "STUDENT");
                }
            }

            // 4. Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Công nghệ phần mềm" },
                    new Category { Name = "Hệ thống thông tin" },
                    new Category { Name = "Khoa học máy tính" },
                    new Category { Name = "An toàn thông tin" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // 5. Seed Courses
            if (!await context.Courses.AnyAsync())
            {
                var se = await context.Categories.FirstAsync(c => c.Name == "Công nghệ phần mềm");
                var isCategory = await context.Categories.FirstAsync(c => c.Name == "Hệ thống thông tin");
                var cs = await context.Categories.FirstAsync(c => c.Name == "Khoa học máy tính");
                var security = await context.Categories.FirstAsync(c => c.Name == "An toàn thông tin");

                var courses = new List<Course>
                {
                    new Course
                    {
                        Name = "Lập trình Web ASP.NET Core",
                        Credits = 3,
                        Lecturer = "Nguyễn Văn A",
                        CategoryId = se.Id,
                        Image = "/images/aspnetcore.jpg"
                    },
                    new Course
                    {
                        Name = "Phân tích và Thiết kế hệ thống",
                        Credits = 4,
                        Lecturer = "Trần Thị B",
                        CategoryId = isCategory.Id,
                        Image = "/images/system_analysis.jpg"
                    },
                    new Course
                    {
                        Name = "Trí tuệ nhân tạo (AI)",
                        Credits = 3,
                        Lecturer = "Lê Hoàng C",
                        CategoryId = cs.Id,
                        Image = "/images/artificial_intelligence.jpg"
                    },
                    new Course
                    {
                        Name = "Mạng máy tính và Truyền thông",
                        Credits = 3,
                        Lecturer = "Phạm Minh D",
                        CategoryId = cs.Id,
                        Image = "/images/computer_network.jpg"
                    },
                    new Course
                    {
                        Name = "An toàn bảo mật thông tin",
                        Credits = 3,
                        Lecturer = "Đỗ Hoàng E",
                        CategoryId = security.Id,
                        Image = "/images/security.jpg"
                    },
                    new Course
                    {
                        Name = "Lập trình ứng dụng di động",
                        Credits = 4,
                        Lecturer = "Bùi Văn F",
                        CategoryId = se.Id,
                        Image = "/images/mobile_dev.jpg"
                    },
                    new Course
                    {
                        Name = "Cơ sở dữ liệu nâng cao",
                        Credits = 3,
                        Lecturer = "Nguyễn Thị G",
                        CategoryId = isCategory.Id,
                        Image = "/images/advanced_db.jpg"
                    },
                    new Course
                    {
                        Name = "Kiến trúc phần mềm nâng cao",
                        Credits = 3,
                        Lecturer = "Hoàng Văn H",
                        CategoryId = se.Id,
                        Image = "/images/software_architecture.jpg"
                    }
                };

                await context.Courses.AddRangeAsync(courses);
                await context.SaveChangesAsync();
            }
        }
    }
}
