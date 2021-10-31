
using System.ComponentModel.DataAnnotations;

namespace WebUI.Model
{
    public class URLLink
    {
        [Required]
        [StringLength(180,MinimumLength =4, ErrorMessage = "URL incorrect length (4 to 180 character limit).")]
        public string URL { get; set; }
    }
}
