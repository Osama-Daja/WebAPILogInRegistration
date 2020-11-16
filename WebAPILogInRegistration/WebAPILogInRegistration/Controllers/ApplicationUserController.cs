using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebAPILogInRegistration.Models;


namespace WebAPILogInRegistration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private UserManager<ApplicationUser> _UserManeger;
        private SignInManager<ApplicationUser> _SingnManeger;
        private ApplicationSettings _appSetting;
        private AuthenticationContext _context;

        public ApplicationUserController(AuthenticationContext context,UserManager<ApplicationUser> UserManeger, SignInManager<ApplicationUser> SingnManeger, IOptions<ApplicationSettings> appSettings)
        {
            _SingnManeger = SingnManeger;
            _UserManeger = UserManeger;
            _appSetting = appSettings.Value;
            _context = context;
        }


        [HttpPost]
        [Route("Register")]
        //Post : /api/ApplicationUser/Register
        public async Task<Object> PostApplicationUser(ApplicationUserModel Model)
        {
            if (Model == null) return new StatusCodeResult(500);

            //var result = await _UserManeger.CreateAsync(role);

            var applicationuser = new ApplicationUser()
            {
                Email = Model.Email,
                FullName = Model.FullName,
                UserName = Model.UserName,
            };

            var result = await _UserManeger.CreateAsync(applicationuser, Model.Password);

            try
            {
                    var UserRole = new IdentityRole()
                    {
                        Name = Model.Role
                    };
                    _context.Roles.Add(UserRole);
                    _context.SaveChanges();

                    IdentityUserRole<string> R = new IdentityUserRole<string>()
                    {
                        UserId = applicationuser.Id,
                        RoleId = UserRole.Id
                    };
                    _context.UserRoles.Add(R);
                    _context.SaveChanges();

                    if (await _UserManeger.IsInRoleAsync(applicationuser, Model.Role))
                    {
                        var resultRol = await _UserManeger.AddToRoleAsync(applicationuser, Model.Role);
                    }

                //var resultRol = await _UserManeger.AddToRoleAsync(applicationuser, Model.Role);

                return Ok(result);
            }
            catch (Exception ex)
            {
                //return new StatusCodeResult(500);
                     throw ex;
            }
        }


        [HttpPost]
        [Route("Login")]
        //POST : /api/ApplicationUser/Login
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _UserManeger.FindByNameAsync(model.UserName);

            if (user != null && await _UserManeger.CheckPasswordAsync(user, model.Password))
            {
                var role = await _UserManeger.GetRolesAsync(user);
                IdentityOptions _options = new IdentityOptions();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim("UserID",user.Id.ToString()),
                    new Claim(_options.ClaimsIdentity.RoleClaimType,role.FirstOrDefault())
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSetting.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new { token });
            }
            else
            {
                return BadRequest(new { message = "User Name Or Password is incorrect." });
            }
        }
    }

    /*
     * 
     *             var user = await _UserManeger.FindByNameAsync(model.UserName);
            if (user != null && await _UserManeger.CheckPasswordAsync(user, model.Password))
            {
                var role = await _UserManeger.GetRolesAsync(user);
                IdentityOptions _options = new IdentityOptions();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserID",user.Id),
                        new Claim(_options.ClaimsIdentity.RoleClaimType,role.FirstOrDefault())
                    }),
                    Expires = DateTime.Now.Add(TimeSpan.FromDays(1)),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSetting.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new { token });
            }
            else
                return BadRequest(new { message = "Username or password is incorrect." });
    */
}
