using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veb_portal_za_aukcijsku_prodaju.Models.Authentication
{
    public class NewAuctionModel
    {
        [Required(ErrorMessage = "*Ime proizvoda je neophodno.")]
        public string Proizvod { get; set; }

        [Required(ErrorMessage = "*Trajanje u sekundama je neophodno.")]
        public int Trajanje { get; set; }

        [Required(ErrorMessage = "*Početna cena proizvoda je neophodna.")]
        public double PocetnaCena { get; set; }

        [NotMapped]
        public HttpPostedFileBase Slika { get; set; }
    }
}