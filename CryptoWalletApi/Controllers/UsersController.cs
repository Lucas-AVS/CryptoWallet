using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CryptoWalletApi.Data;
using CryptoWalletApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MinimalAPIDemo.Models;

namespace CryptoWalletApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApiContext _context;

        public UsersController(ApiContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var response = new ApiResponse()
            {
                IsSuccess = true,
                Result = _context.Users.ToList(),
                StatusCode = HttpStatusCode.OK
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public IActionResult Get(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            var response = new ApiResponse();
            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new List<string> { $"User {id} not found" };
                return NotFound(response);
            }

            response.IsSuccess = true;
            response.Result = user;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }


        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public IActionResult Create([FromBody] string name, string email, string password)
        {
            var response = new ApiResponse()
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.BadRequest
            };

            if (name is null || email is null || password is null)
            {
                return BadRequest(response);
            }

            var HighId = _context.Users.Max(u => (int?)u.Id);

            var user = new User
            {
                Id = HighId == null ? 1 : HighId.Value + 1,
                Name = name,
                Email = email,
                Wallets = null,
                PasswordHash = password // will be changed

            };
            _context.Users.Add(user);

            response.IsSuccess = true;
            response.Result = user;
            response.StatusCode = HttpStatusCode.Created;

            _context.SaveChanges();

            return Created($"/api/friends/{user.Name}", response);
        }
    }
}
