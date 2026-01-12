using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class TrophyImageUpload
    {
        [FromForm(Name = "game")]
        [Required]
        public string Game { get; set; }

        [FromForm(Name = "trophy")]
        [Required]
        public string Trophy { get; set; }

        [FromForm(Name = "trueimagefile")]
        [Required]
        public IFormFile TrueImageFile { get; set; }

        [FromForm(Name = "falseimagefile")]
        [Required]
        public IFormFile FalseImageFile { get; set; }
    }
}
