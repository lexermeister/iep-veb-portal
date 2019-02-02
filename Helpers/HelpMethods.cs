using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Veb_portal_za_aukcijsku_prodaju.Models;

namespace Veb_portal_za_aukcijsku_prodaju.Helpers
{
    public class HelpMethods
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void BidAuction(int auctionID, int userID, out string fullUserName, out string newPrice, out bool tokens, out double timeRemaining)
        {

            try
            {
                Aukcija aukcija = null;
                Korisnik korisnik = null;
                using (var context = new IEPVebAukcijaEntities7())
                {
                    aukcija = context.Aukcijas.Find(auctionID);
                    korisnik = context.Korisniks.Find(userID);
                }

                if ((aukcija != null) && (korisnik != null))
                {
                    if((korisnik.BrojTokena > 0) && (aukcija.Status == "OPEN"))
                    {
                        tokens = false;
                        double newPriceDouble = (double)aukcija.TrenutnaCena + 1;

                        aukcija.TrenutnaCena = (decimal)newPriceDouble;

                        double preostalo = ((DateTime)aukcija.VremeZatvaranja - DateTime.Now).TotalSeconds;

                        DateTime newDate = (DateTime)aukcija.VremeZatvaranja;
                        if(preostalo <= 10)
                        {
                            double increment = preostalo * (-1);
                            increment += 10; 
                            
                            newDate = newDate.AddSeconds(increment);                            
                        }

                        aukcija.VremeZatvaranja = newDate;
                        timeRemaining = aukcija.PreostaloVreme = ((DateTime)aukcija.VremeZatvaranja - DateTime.Now).TotalSeconds;                        

                        Bid newBid = new Bid()
                        {
                            PonCena = (decimal)newPriceDouble,
                            VremeSlanja = DateTime.Now,
                            KorisnikID = userID,
                            AukcijaID = auctionID
                        };

                        using (var context = new IEPVebAukcijaEntities7())
                        {                            

                            context.Entry(aukcija).State = System.Data.Entity.EntityState.Modified;
                            context.SaveChanges();

                            context.Bids.Add(newBid);
                            context.SaveChanges();

                            logger.Error("AUCTION BIDED: AuctionID: " + aukcija.AukcijaID + ", AuctionCurrentPrice: " + aukcija.TrenutnaCena + ", UserBidedID: " + userID);

                            fullUserName = korisnik.Ime + " " + korisnik.Prezime;
                            newPrice = "" + newPriceDouble;
                        }
                    }
                    else
                    {
                        fullUserName = newPrice = null;

                        if(aukcija.Status != "OPEN")
                        {
                            tokens = false;
                            timeRemaining = -1;
                        }
                        else
                        {
                            tokens = true;
                            timeRemaining = 1;
                        }                        
                    }
                }
                else
                {
                    fullUserName = newPrice = null;
                    tokens = true;
                    timeRemaining = -1;
                }
            }
            catch (FormatException)
            {
                fullUserName = newPrice = null;
                tokens = true;
                timeRemaining = -1;
                Console.WriteLine("Something went wrong.");
            }
        }

        public void ChangePrice(int? id, string newPrice)
        {            

            try
            {
                double doubleValue = Convert.ToDouble(newPrice);
                Aukcija editAukcija;

                using (var context = new IEPVebAukcijaEntities7())
                {
                    editAukcija = context.Aukcijas.Find(id);
                }

                if (editAukcija != null)
                {
                    editAukcija.PocetnaCena = (decimal)doubleValue;
                    editAukcija.TrenutnaCena = (decimal)doubleValue;
                }

                using (var context = new IEPVebAukcijaEntities7())
                {
                    context.Entry(editAukcija).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    logger.Error("AUCTION PRICE CHANGE: AuctionID: " + editAukcija.AukcijaID + ", AuctionStatus: " + editAukcija.Status + ", AuctionStartPrice: " + editAukcija.PocetnaCena);
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Unable to convert '{0}' to a Double.", newPrice);
            }
            catch (OverflowException)
            {
                Console.WriteLine("'{0}' is outside the range of a Double.", newPrice);
            }

            //return RedirectToAction("Index", "Admin", new { id = id });
        }

        public void OpenAuction(int? id)
        {
            if (id == null)
            {
                return;
            }

            try
            {
                Aukcija editAukcija;
                   
                using (var context = new IEPVebAukcijaEntities7())
                {
                    editAukcija = context.Aukcijas.Find(id);
                }

                if (editAukcija != null)
                {
                    DateTime startTime = DateTime.Now;
                    DateTime endTime = startTime.AddSeconds((int)editAukcija.Trajanje);

                    editAukcija.Status = "OPEN";
                    editAukcija.VremeOtvaranja = startTime;
                    editAukcija.VremeZatvaranja = endTime;
                }


                using (var context = new IEPVebAukcijaEntities7())
                {
                    context.Entry(editAukcija).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    logger.Error("AUCTION OPEN: AuctionID: " + editAukcija.AukcijaID + ", AuctionStatus: " + editAukcija.Status);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to delete auction");
            }

            // return RedirectToAction("Index", "Admin", new { id = id });
        }

        public string AuctionOver(int id)
        {
            string result = "";
            Aukcija aukcija;

            using (var context = new IEPVebAukcijaEntities7())
            {
                aukcija = context.Aukcijas.Find(id);
            }


            if (aukcija != null)
            {
                if (!aukcija.Status.Equals("OPEN"))
                {
                    result = aukcija.Status;
                }
                else
                {
                    Nullable<int> lastBid = aukcija.BidID;

                    if (lastBid != null)
                        aukcija.Status = "SOLD";                        
                    else
                        aukcija.Status = "EXPIRED";

                    result = aukcija.Status;

                    using (var context = new IEPVebAukcijaEntities7())
                    {
                        context.Entry(aukcija).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();

                        if(result == "SOLD")
                        {
                            Bid bid = context.Bids.Find(aukcija.BidID);
                            Korisnik korisnik = context.Korisniks.Find(bid.KorisnikID);

                            korisnik.Aukcijas.Add(aukcija);
                            context.SaveChanges();

                            logger.Error("AUCTION OVER: AuctionID: " + aukcija.AukcijaID + ", AuctionStatus: " + aukcija.Status);
                        }
                    }
                }

            }

            return result;
        }
    }
}