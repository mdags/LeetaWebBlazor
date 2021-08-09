using DataAccessLibrary.Models;
using LeetaWebBlazor.Editors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LeetaWebBlazor.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class tokenController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        public tokenController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public async Task<IActionResult> Authenticate([FromBody] UserModel userParam)
        {
            if (userParam.user_name == "leeta_sa" && userParam.user_password == "Le2021..")
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userParam.user_name),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var authSigningKey = Encoding.ASCII.GetBytes(_appSettings.Secret);

                var token = new JwtSecurityToken(
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(authSigningKey), SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }
    }
}
