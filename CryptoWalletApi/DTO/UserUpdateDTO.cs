using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoWalletApi.DTO
{
    public class UserUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [MinLength(2)]
        public string Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MinLength(6)]
        public string? NewPassword { get; set; }
    }
}