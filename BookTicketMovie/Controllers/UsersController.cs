using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookTicketMovie.Data;
using BookTicketMovie.Models;
using BookTicketMovie.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace BookTicketMovie.Controllers
{
    public class UsersController : Controller
    {
        private readonly BookTicketMovieContext _context;
        private readonly ICommonUserService<User> _userService;
        private readonly ICommonDataService<User> _userCrudService;
        public UsersController(BookTicketMovieContext context, ICommonUserService<User> userService, ICommonDataService<User> userCrudService)
        {
            _context = context;
            _userService = userService;
            _userCrudService = userCrudService;
        }

		public IActionResult Login()
        {
            if (User.Identity != null)
            {
                if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Login")]
        public async Task<IActionResult> LoginUser(User user)
        {
           
            if (ModelState.IsValid)
            {
                var userResult = await _userService.Login(user);
                if (userResult != null)
                {
                    HttpContext.Session.SetString("Username", userResult.Name!);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier,  userResult.Id!.ToString()),
                        new Claim(ClaimTypes.Name,  userResult.Name!),
                        new Claim(ClaimTypes.Email,  userResult.Email!),
                        new Claim(ClaimTypes.Role,  userResult.Role!)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return RedirectToAction("Index", "Home");
                }    
                ModelState.AddModelError("userWrong", "Tài khoản hoặc mật khẩu không chính xác");
                return View(user);
            }
            return View(user);
        }

        public async Task<IActionResult> Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user, string rePassword)
        {
            if (user.Name == "" || user.Name == null)
            {
                ModelState.AddModelError("Name", "Tên không được bỏ trống");
            }
            if (user.Email == null)
            {
                ModelState.AddModelError("Email", "Email không được bỏ trống");
            }
            if (user.Password == null)
            {
                ModelState.AddModelError("Password", "Password không được bỏ trống");
            }

            if (user.Email != null)
            {
                var checkEmail = await _userService.MailIsExists(user.Email);
                if (checkEmail)
                {
                    ModelState.AddModelError("CheckEmail", "Email đã tồn tại");
                }
            }

            if (user.Password != rePassword)
            {
                ModelState.AddModelError("rePassWrong", "Mật khẩu nhập lại không chính xác");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            



            if (ModelState.IsValid)
            {
                user.Role = "";
                await _userCrudService.CreateAsync(user);
                return View(nameof(Login));
            }

            return View(user);
        }
        [Authorize(Roles = "admin")]

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.User.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Password,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [Authorize(Roles = "admin")]


        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Password,Role")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _userCrudService.EditAsync(user) != null)
                    return RedirectToAction(nameof(Index));
                return NotFound();
            }
            return View(user);
        }
        [Authorize(Roles = "admin")]

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userCrudService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            if (id == Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
                return RedirectToAction(nameof(Login));
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

	}
}
