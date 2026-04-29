using ToDoList.Models;
using ToDoList.Models.Dtos;
using AutoMapper;

namespace ToDoList.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserRegisterDto, User>();

            CreateMap<Todo, TodoCreateDto>().ReverseMap();
        }   
    }
}
