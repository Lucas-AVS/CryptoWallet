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

            CreateMap<User, UserResponseDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? src.Email))
                .ReverseMap();

            CreateMap<User, UserUpdateDTO>().ReverseMap();

            CreateMap<Wallet, WalletDTO>()
                .ForMember(dest => dest.CryptoBalances,
                           opt => opt.MapFrom(src => src.CryptoBalances))
                .ReverseMap();

            CreateMap<CryptoBalance, CryptoBalanceDTO>().ReverseMap();

            CreateMap<Transaction, TransactionCreateDTO>().ReverseMap();

            CreateMap<Transaction, TransactionResponseDTO>().ReverseMap();
        }
    }
}