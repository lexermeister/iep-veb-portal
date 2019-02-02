using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Veb_portal_za_aukcijsku_prodaju.Models.Authentication
{
    public class RegisterModel
    {

        [Required(ErrorMessage = "*Vaše ime je neophodno.")]
        public string Ime { get; set; }

        [Required(ErrorMessage = "*Vaše prezime je neophodno.")]
        public string Prezime { get; set; }

        [Required(ErrorMessage = "*Vaša email adresa je neophodna.")]
        [EmailAddress(ErrorMessage = "*Uneli ste neispravnu email adresu.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "*Vaša lozinka je neophodna.")]
        public string Lozinka { get; set; }

        [Required(ErrorMessage = "*Potvrda lozinke je neophodna.")]
        public string PotvrdaLozinke { get; set; }
    }
}