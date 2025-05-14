using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CryptoWalletApi.Data;
using CryptoWalletApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CryptoWalletApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApiContext _context;

        public UserController(ApiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public JsonResult Get()
        {
            var users = _context.Users.ToList();
            return new JsonResult(Ok(users));
        }


        // [HttpGet("{id}")]
        // public IActionResult GetById(int id)
        // {
        //     return Ok($"Hello from User controller with id {id}");
        // }

        [HttpPost]
        public JsonResult Create([FromBody] string name)

        {
            var user = new User
            {
                Id = 1,
                Name = name,
                Email = "aa@email.com",
                Wallets = null,
                PasswordHash = "aaa"

            };
            _context.Users.Add(user);
            _context.SaveChanges();

            return new JsonResult(CreatedAtAction(nameof(User), new { name = user.Name }, user));
        }
    }
}