﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Ringer.Core.Data;
using Ringer.Core.Models;

namespace Ringer.HubServer.Extensions
{
    public static class UserExtension
    {
        public static string JwtToken(this User user, LoginInfo loginInfo, string secretKey)
        {
            var securityKey = secretKey;
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Context.UserIdentifier
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("DeviceId", loginInfo.DeviceId),
                new Claim("DeviceType", loginInfo.DeviceType.ToString())
            };

            // create token
            var token = new JwtSecurityToken
            (
                issuer: "Ringer",
                audience: "ringer.co.kr",
                expires: DateTime.UtcNow.AddSeconds(5),
                signingCredentials: signingCredentials,
                claims: claims
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
