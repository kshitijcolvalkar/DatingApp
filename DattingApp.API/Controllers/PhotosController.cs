using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DattingApp.API.Data;
using DattingApp.API.Dtos;
using DattingApp.API.Helpers;
using DattingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DattingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userid}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _clodinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> clodinaryConfig)
        {
            _clodinaryConfig = clodinaryConfig;
            _mapper = mapper;
            _repo = repo;

            Account acc = new Account(
                _clodinaryConfig.Value.CloudName,
                _clodinaryConfig.Value.ApiKey,
                _clodinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name="GetPhoto")]
        public async Task<IActionResult> GetPhoto(int userid, int id)
        {
            if(userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var photo = await _repo.GetPhoto(id);
            var photoForReturn = _mapper.Map<PhotoForReturnDto>(photo);

            return Ok(photoForReturn);
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhotosForUser(int userid, [FromForm]PhotoForUserDto photoForUserDto)
        {
            if(userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var userFromRepo = await _repo.GetUser(userid);

            var file = photoForUserDto.File;

            var uploadResult = new ImageUploadResult();

            if(file != null && file.Length > 0)
            {
                using(var filestream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, filestream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }
            else
            {
                return BadRequest();
            }

            photoForUserDto.Url = uploadResult.Uri.ToString();
            photoForUserDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForUserDto);

            if(!userFromRepo.Photos.Any(p => p.IsMain))
                photo.IsMain = true;
            
            userFromRepo.Photos.Add(photo);

            if(await _repo.SaveAll())
            {
                var photoForReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { userid = userid, id = photo.Id }, photoForReturn);
            }
            
            return BadRequest($"Error uploading photo for user {userid}");
        }

        [HttpPost("{id}/setmain")]
        public async Task<IActionResult> SetMainPhoto(int userid, int id)
        {
            if(userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var userFromRepo = await _repo.GetUser(userid);

            if(!userFromRepo.Photos.Any(p => p.Id == id))
                return Unauthorized();
            
            var photo = await _repo.GetPhoto(id);

            if(photo.IsMain)
                return BadRequest("Photo you are trying to update is already a main photo");
            
            var currentMainPhoto = await _repo.GetMainPhotoForUser(userid);
            currentMainPhoto.IsMain = false;

            photo.IsMain = true;

            if(await _repo.SaveAll())
                return NoContent();
            
            return BadRequest($"Error while trying to set the photo {id} to main");
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userid, int id)
        {
            if(userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var userFromRepo = await _repo.GetUser(userid);

            if(!userFromRepo.Photos.Any(p => p.Id == id))
                return Unauthorized();
            
            var photo = await _repo.GetPhoto(id);

            if(photo.IsMain)
                return BadRequest("You cannot delete your main photo.");
            
            if(!string.IsNullOrWhiteSpace(photo.PublicId))
            {
                var deleteParams = new DeletionParams(photo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);

                if(result.Result == "ok")
                    _repo.Delete(photo);
            }
            else
            {
                _repo.Delete(photo);
            }

            if(await _repo.SaveAll())
                return Ok();
            
            return BadRequest("Error while deleting the photo");
        }
    }
}