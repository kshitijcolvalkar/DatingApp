using System;
using Microsoft.AspNetCore.Http;

namespace DattingApp.API.Dtos
{
    public class PhotoForUserDto
    {
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public String PublicId { get; set; }
        public IFormFile File { get; set; }
        
    }
}