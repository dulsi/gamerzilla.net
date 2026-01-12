using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class GameImageUpload
    {
        [FromForm(Name = "game")]
        [Required]
        public string Game { get; set; }

        [FromForm(Name = "imagefile")]
        [Required]
        public IFormFile ImageFile { get; set; }
    }
}
