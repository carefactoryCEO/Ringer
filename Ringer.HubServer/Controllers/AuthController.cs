using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Ringer.Core.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ringer.HubServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public async Task<ActionResult<string>> LoginAsync(LoginInfo loginInfo)
        {
            // TODO: compare to server
            var user = new User
            {
                Name = loginInfo.Name,
                Email = loginInfo.Email,
                Password = loginInfo.Password
            };

            var securityKey = "this_is_super_long_security_key_for_ringer_service";
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()), // Context.UserIdentifier
                new Claim(ClaimTypes.Email, user.Email)
            };

            // create token
            var token = new JwtSecurityToken
                (
                    issuer: "Ringer",
                    audience: "ringer.co.kr",
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: signingCredentials,
                    claims: claims
                );

            await Task.Delay(10);

            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }


    /*
     * var loginInfo = JsonSerializer.Serialize(new
            {
                Email = "test@carefactory.co.kr",
                Password = "some-password",
                DeviceType = "console",
                LoginType = "Password",
                Name = name
            });
     */
}
