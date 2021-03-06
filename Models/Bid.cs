//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Veb_portal_za_aukcijsku_prodaju.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Bid
    {
        public Bid()
        {
            this.Aukcijas = new HashSet<Aukcija>();
        }
    
        public int BidID { get; set; }
        public Nullable<decimal> PonCena { get; set; }
        public Nullable<int> AukcijaID { get; set; }
        public Nullable<int> KorisnikID { get; set; }
        public Nullable<System.DateTime> VremeSlanja { get; set; }
        public Nullable<int> BrojTokena { get; set; }
    
        public virtual Aukcija Aukcija { get; set; }
        public virtual ICollection<Aukcija> Aukcijas { get; set; }
        public virtual Korisnik Korisnik { get; set; }

        public string KorisnikImePrezime { get; set; }

    }
}
