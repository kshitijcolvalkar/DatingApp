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
    [Route("api/users/{userId}/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;

        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var message = await _repo.GetMessage(id);

            if(message == null)
                return NotFound();
            
            var messageForReturn = _mapper.Map<MessageForReturnDto>(message);

            return Ok(messageForReturn);
            
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery]MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            messageParams.UserId = userId;

            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);

            var messagesToReturn = _mapper.Map<IEnumerable<MessageForReturnDto>>(messagesFromRepo);
            
            Response.WritePaginationHeader(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);
            return Ok(messagesToReturn);
        }

        [HttpGet("thread/{recepientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recepientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var messagesFromRepo = await _repo.GetMessageThread(userId, recepientId);

            var messageThread = _mapper.Map<IEnumerable<MessageForReturnDto>>(messagesFromRepo);
            return Ok(messageThread);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreation)
        {
            var sender = await _repo.GetUser(userId);
            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            messageForCreation.SenderId = userId;

            if(await _repo.GetUser(messageForCreation.RecepientId) == null)
                return NotFound();
            
            var message = _mapper.Map<Message>(messageForCreation);

            _repo.Add(message);

            if(await _repo.SaveAll())
            {
                var messageForReturn = _mapper.Map<MessageForReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { userId, id = message.Id}, messageForReturn);
            }

            return BadRequest("Failed to create message");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var messagesFromRepo = await _repo.GetMessage(id);

            if(messagesFromRepo == null)
                return NotFound();
            
            if(messagesFromRepo.SenderId == userId)
                messagesFromRepo.SenderDeleted = true;
            
            if(messagesFromRepo.RecepientId == userId)
                messagesFromRepo.RecepientDeleted = true;
            
            if(messagesFromRepo.SenderDeleted && messagesFromRepo.RecepientDeleted)
                _repo.Delete(messagesFromRepo);
            
            if(await _repo.SaveAll())
                return NoContent();

            return BadRequest("Error deleting the meaage");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var messagesFromRepo = await _repo.GetMessage(id);

            if(userId != messagesFromRepo.RecepientId)
                return Unauthorized();
            
            messagesFromRepo.IsRead = true;
            messagesFromRepo.DateRead = DateTime.Now;

            if(await _repo.SaveAll())
                return NoContent();
            
            return BadRequest();
        }
    }
}