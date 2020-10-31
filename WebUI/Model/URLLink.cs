using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Model
{
    public class URLLink
    {
        [Required]
        [StringLength(120,MinimumLength =4, ErrorMessage = "URL too incorrect length (4 to 120 character limit).")]

        public string URL { get; set; }
    }
}
