using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Models;
using WebApplication.ViewModels;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [NonAction]
        private async Task<bool> UpdateDateTimeLastLogin()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                if (user.LockoutEnabled)
                {
                    await _signInManager.SignOutAsync();
                    return false;
                }

                user.LastLogin = DateTime.Now;
                var lastLoginResult = await _userManager.UpdateAsync(user);
                if (!lastLoginResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unexpected error occurred setting the last login date" +
                        $" ({lastLoginResult.ToString()}) for user with ID '{user.Id}'.");
                }
            }

            return true;
        }

        public async Task<IActionResult> Index()
        {
            if (await UpdateDateTimeLastLogin())
            {
                return View(_userManager.Users.ToList());
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    await UpdateDateTimeLastLogin();
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "The account is locked out");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect username and / or password");
                }
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

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email, Name = model.Name, DateTimeRegistration = DateTime.Now };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    await UpdateDateTimeLastLogin();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
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

        [HttpPost]
        public async Task<IActionResult> Block(string[] ids)
        {
            foreach (var id in ids)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user.Email == User.Identity.Name)
                {
                    await _signInManager.SignOutAsync();
                }

                if (user != null)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTime.Now.AddYears(100);
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(string[] ids)
        {
            foreach (var id in ids)
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user != null)
                {
                    user.LockoutEnabled = false;
                    user.LockoutEnd = null;
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string[] ids)
        {
            IActionResult redirectPage = RedirectToAction(nameof(Index));
            foreach (var id in ids)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user.Email == User.Identity.Name)
                {
                    redirectPage = RedirectToAction(nameof(Login));
                    await _signInManager.SignOutAsync();
                }

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            return redirectPage;
        }
    }
}
