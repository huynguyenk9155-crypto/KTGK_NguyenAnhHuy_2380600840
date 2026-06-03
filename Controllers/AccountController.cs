using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KTGK_LapTrinhWeb.Models;

namespace KTGK_LapTrinhWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Đảm bảo role STUDENT tồn tại
                    if (!await _roleManager.RoleExistsAsync("STUDENT"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("STUDENT"));
                    }

                    // Gán role STUDENT mặc định cho người dùng đăng ký mới
                    await _userManager.AddToRoleAsync(user, "STUDENT");

                    // Đăng nhập sau khi đăng ký thành công
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Thử tìm người dùng theo username trước, nếu không thấy thử tìm theo email
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null && model.Username.Contains('@'))
                {
                    user = await _userManager.FindByEmailAsync(model.Username);
                }

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Chức năng Đăng nhập bằng Google
        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi từ Google: {remoteError}");
                return View("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Đăng nhập bằng External Login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            // Nếu người dùng chưa có tài khoản, tự động tạo mới tài khoản
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Không lấy được thông tin Email từ tài khoản Google của bạn.");
                return View("Login");
            }

            // Tìm xem email này đã tồn tại trong hệ thống chưa
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Tạo tên đăng nhập tự động từ Email
                var username = email.Split('@')[0];
                var existingUser = await _userManager.FindByNameAsync(username);
                if (existingUser != null)
                {
                    username = username + Guid.NewGuid().ToString().Substring(0, 4);
                }

                user = new IdentityUser { UserName = username, Email = email, EmailConfirmed = true };
                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi khởi tạo tài khoản liên kết Google.");
                    return View("Login");
                }

                // Gán role mặc định: STUDENT
                if (!await _roleManager.RoleExistsAsync("STUDENT"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("STUDENT"));
                }
                await _userManager.AddToRoleAsync(user, "STUDENT");
            }

            // Liên kết External Login với User mới/cũ
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (addLoginResult.Succeeded || await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey) != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Không thể liên kết tài khoản Google.");
            return View("Login");
        }
    }
}
