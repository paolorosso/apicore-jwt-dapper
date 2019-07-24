using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private Repository.IAuthRepo repo;

        public AuthController(Repository.IAuthRepo _authRepo)
        {
            repo = _authRepo;
        }



        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] Models.AccessCred cred)
        {

            Models.Token token = null;

            if (cred.Grant_type == "password")
            {
                token = await repo.GetToken(cred);
            }

            if (token == null)
                return BadRequest(new { message = "Utente o password non corretti." });
            else
                return Ok(token);
        }



        [HttpPost]
        [Route("refreshtoken")]
        public async Task<IActionResult> Refresh([FromBody] Models.AccessCred cred)
        {

            Models.Token token = null;

            if (cred.Grant_type == "refresh_token")
            {
                token = await repo.GetRefreshToken(cred);
            }

            if (token == null)
                return BadRequest(new { message = "Refresh token non valido." });
            else
                return Ok(token);
        }



        [HttpPost]
        [Authorize]
        [Route("logout")]
        public async Task<IActionResult> Logout(Models.AccessCred cred)
        {
            int idUser =Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await repo.SignOut(cred.Refresh_token, idUser);

            return Ok();

        }
    }
}