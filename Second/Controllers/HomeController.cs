using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Second.Helper;

namespace Second.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,"some id"),
                new Claim("department","Technology"),
                new Claim("level","1")
            };


            var secretBytes = Encoding.UTF8.GetBytes(JwtTokenConstants.Secret);
            var key = new SymmetricSecurityKey(secretBytes);
            var algorithm = SecurityAlgorithms.HmacSha256;
            var signinCredentials = new SigningCredentials(key,algorithm);

            var token = new JwtSecurityToken(
                JwtTokenConstants.Issuer,
                JwtTokenConstants.Audience,
                claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddDays(30),
                signinCredentials
                );

            var tokenJson = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { access_token = tokenJson });
        }

        [Authorize]
        public IActionResult Secret()
        {
            return View();
        }
    }
}
