﻿using AutoMapper;
using MatchDayApp.Application.Models;
using MatchDayApp.Application.Models.Auth;
using MatchDayApp.Infra.CrossCutting.Contract.V1.Request.Auth;
using MatchDayApp.Infra.CrossCutting.Contract.V1.Request.User;

namespace MatchDayApp.Infra.CrossCutting.Mapper
{
    public class RequestToViewModelMappingProfile : Profile
    {
        public RequestToViewModelMappingProfile()
        {
            CreateMap<LoginRequest, LoginModel>();
            CreateMap<RegisterRequest, RegisterModel>();
            CreateMap<ResetPasswordRequest, ResetPasswordModel>();
            CreateMap<UpdateUserRequest, UserModel>();
        }
    }
}
