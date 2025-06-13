using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using Rospand_IMS.Services;
using System.Threading.Tasks;

namespace Rospand_IMS.Controllers
{

    public  class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public AuthController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }


        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }

        // Changed from [FromBody] to regular form submission
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var token = await _authService.Login(model.Username, model.Password);
            if (token == null)
            {
                TempData["ErrorMessage"] = "Invalid credentials";
                return View();
            }

            // Store the token in a cookie or session
            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Enable in production with HTTPS
                SameSite = SameSiteMode.Strict
            });

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("Register")]

        public IActionResult Register()
        {
            var roles = _context.Roles.ToList();
            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost("Register")]
      
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _context.Roles.ToList();
                return View(model);
            }

            var role = _context.Roles.FirstOrDefault(r => r.Id == model.RoleId);
            if (role == null)
            {
                ModelState.AddModelError("RoleId", "Invalid role selected");
                ViewBag.Roles = _context.Roles.ToList();
                return View(model);
            }

            var result = await _authService.Register(model.Username, model.Password, model.RoleId);
            if (result == null)
            {
                ModelState.AddModelError("", "Registration failed. Username might already exist.");
                ViewBag.Roles = _context.Roles.ToList();
                return View(model);
            }

            TempData["SuccessMessage"] = "User registered successfully";
            return RedirectToAction("Register");
        }

    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
    }
}