using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ringer.Core.Models;

namespace Ringer.HubServer.Extensions
{
    public static class UserExtension
    {
        public static string JwtToken(this User user)
        {
            var securityKey = "this_is_super_long_security_key_for_ringer_service";
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()), // Context.UserIdentifier
                new Claim(ClaimTypes.Name, user.Name)
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

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
