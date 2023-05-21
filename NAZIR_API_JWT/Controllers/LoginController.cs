// LoginController.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JWT.Controllers
{
	public class UserModel
	{
		public string username { get; set; }
		public string password { get; set; }
		public string email { get; set; }
	}

	[ApiController]
	[Route("api/[controller]")]
	public class LoginController : ControllerBase
	{
		[Route("CreateJWT")]
		private string CreateJWT(UserModel user)
		{
			var secretkey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("yJhbGciOiJIUzI1NiJ9.eyJSb2xlIjoiQWRtaW4iLCJJc3N1ZXIiOiJJc3N1ZXIiLCJVc2VybmFtZSI6IkphdmFJblVzZSIsImV4cCI6MTY4NDY5MDk3MywiaWF0IjoxNjg0NjkwOTczfQ.Vq6MzQTC7jhGmyr7QeXjfaGyghZvRYu624AtS3o8C20")); // NOTE: SAME KEY AS USED IN Startup.cs FILE
			var credentials = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256);

			var claims = new[] // NOTE: could also use List<Claim> here
			{
				new Claim(ClaimTypes.Name, user.username), // this will be "User.Identity.Name" value
				new Claim(JwtRegisteredClaimNames.Sub, user.username),
				new Claim(JwtRegisteredClaimNames.Email, user.email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")) // this could the unique ID assigned to the user by a database
			};

			var token = new JwtSecurityToken(issuer: "https://localhost:44345/", audience: "https://localhost:44345/", claims: claims, expires: DateTime.Now.AddMinutes(60), signingCredentials: credentials);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		private UserModel Authenticate(UserModel login)
		{
			if (login.username == "test" && login.password == "abc123") // NOTE: in production, query a database for user information
				return new UserModel { username = login.username, email = "test@gmail.com" };
			return null;
		}

		[HttpPost,Route("Post")]
		public async Task<IActionResult> Post([FromBody] UserModel login)
		{
			return await Task.Run(() =>
			{
				IActionResult response = Unauthorized();

				UserModel user = Authenticate(login);

				if (user != null)
					response = Ok(new { token = CreateJWT(user) });

				return response;
			});
		}
	}
}