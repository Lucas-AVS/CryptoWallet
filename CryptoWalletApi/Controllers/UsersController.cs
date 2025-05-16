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


        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public IActionResult Edit([FromBody] int id, string? name, string? email, string? password)
        {
            var response = new ApiResponse()
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.NotFound
            };

            var user = _context.Users.Find(id);

            if (user is null) // ✅ Verifica se o usuário existe
            {
                return NotFound();
            }


            if (name != null)
            {
                user.Name = name;
            }
            if (email != null)
            {
                user.Email = email;
            }
            if (password != null)
            {
                user.PasswordHash = password;
            }

            response.IsSuccess = true;
            response.Result = user;
            response.StatusCode = HttpStatusCode.OK;

            _context.SaveChanges();

            return Ok(response);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)] // invalid ID
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            var response = new ApiResponse();

            if (id <= 0)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ErrorMessages = new List<string> { "Invalid id" };
                return BadRequest(response);
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new List<string> { $"User id:{id} not found." };
                return NotFound(response);
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = new { DeletedUserId = id, Message = "User removed." };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}

