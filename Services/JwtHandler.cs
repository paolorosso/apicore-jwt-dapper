using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Api.Services
{
    interface IJwtHandler
    {
        Models.Token CreateToken(int userId, int role);

    }

    public class JwtHandler : IJwtHandler
    {

        public JwtHandler()
        {
        }


        public Models.Token CreateToken(int userId, int role)
        {

            // ------ Access token ------
            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, userId.ToString()),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToString(), ClaimValueTypes.Integer64),
            };


            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(LOAD.Chiave)), SecurityAlgorithms.HmacSha256);

            int expiryTimeSecond = 3600;      // 1 ora

            DateTime expiry = now.AddMinutes(expiryTimeSecond);    

            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: now,
                expires: expiry,
                signingCredentials: signingCredentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            // ---------------------------



            // ------ Refresh token ------
            string refToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            // ---------------------------

            

            // Token
            return new Models.Token()
            {
                Access_token = token,
                Expires_in = expiryTimeSecond,       
                Refresh_token = refToken
            };
        }

    }
}