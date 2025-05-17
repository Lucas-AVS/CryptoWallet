using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CryptoWalletApi.DTO;
using CryptoWalletApi.Models;

namespace CryptoWalletApi.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserCreateDTO>().ReverseMap();

            CreateMap<User, UserResponseDTO>().ReverseMap();

            CreateMap<User, UserUpdateDTO>().ReverseMap();
        }
    }
}