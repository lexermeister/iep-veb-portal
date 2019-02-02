using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Veb_portal_za_aukcijsku_prodaju.Models.Authentication
{
    public class LoginModel
    {

        [Required(ErrorMessage = "*Vaša email adresa je neophodna.")]
        [EmailAddress(ErrorMessage = "Uneli ste neispravno email adresu.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "*Vaše lozinka je neophodna.")]
        public string Lozinka { get; set; }

    }
}