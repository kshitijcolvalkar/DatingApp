using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DattingApp.API.Data;
using DattingApp.API.Dtos;
using DattingApp.API.Helpers;
using DattingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DattingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repo.GetUser(userId);
            userParams.UserId = userFromRepo.Id;

            if(string.IsNullOrWhiteSpace(userParams.Gender))
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";

            var users = await _repo.GetUsers(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.WritePaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name="GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var userFromRepo = await _repo.GetUser(id);
            _mapper.Map(userForUpdateDto, userFromRepo);

            if(await _repo.SaveAll())
                return NoContent();
            
            throw new Exception($"Failed to update user {id}");

        }

        [HttpPost("{id}/like/{recepientid}")]
        public async Task<IActionResult> AddLike(int id, int recepientid)
        {
            if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var liked = await _repo.GetLike(id, recepientid);

            if(liked != null)
                return BadRequest($"You already liked this user {liked.Likee.KnownAs}");
            
            if(await _repo.GetUser(recepientid) == null)
                return NotFound();
            
            var like = new Like{
                LikerId = id,
                LikeeId = recepientid
            };

            _repo.Add<Like>(like);

            if(await _repo.SaveAll())
                return Ok();
            
            return BadRequest("Error while liking the user");
            
        }
    }
}