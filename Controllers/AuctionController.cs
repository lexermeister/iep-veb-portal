using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Veb_portal_za_aukcijsku_prodaju.Models;
using PagedList;

namespace Veb_portal_za_aukcijsku_prodaju.Controllers
{
    public class AuctionController : Controller
    {

        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool checkAdmin()
        {
            bool cond = true;
            if (Session["admin"] != null)
            {
                if ((bool)Session["admin"])
                {
                    cond = false;
                }
            }

            return cond;
        }

        private bool checkLoginUser()
        {
            bool cond = false;
            if (Session["userID"] != null)
            {
                cond = true;
            }
            return cond;
        }

        // GET: Auction
        public ActionResult Index(int id = -1)
        {
            if (id == -1) return RedirectToAction("Index", "Home");
            Aukcija aukcija = null;
            using (var context = new IEPVebAukcijaEntities7())
            {
                aukcija = context.Aukcijas.Find(id);
                if (aukcija == null)
                {
                    return HttpNotFound();
                }

                var bids = context.Bids
                    .Where(n => n.AukcijaID == id)
                    .OrderByDescending(n => n.PonCena)
                    .Take(10);

                foreach (var bid in bids)
                {
                    Korisnik korisnik = context.Korisniks.Find(bid.KorisnikID);
                    bid.KorisnikImePrezime = korisnik.Ime + " " + korisnik.Prezime;
                }

                aukcija.Top10Bids = bids;

                aukcija.Top10Bids = aukcija.Top10Bids.ToList();

                if ((aukcija.VremeZatvaranja != null) && (!aukcija.VremeZatvaranja.Equals("")) && (aukcija.Status.Equals("OPEN")))
                    aukcija.PreostaloVreme = ((DateTime)aukcija.VremeZatvaranja - DateTime.Now).TotalSeconds;
                else
                    if ((aukcija.VremeOtvaranja != null) && (!aukcija.VremeOtvaranja.Equals("")) && (!aukcija.Status.Equals("OPEN")))
                        //auk.PreostaloVreme = ((DateTime)auk.VremeZatvaranja - (DateTime)auk.VremeOtvaranja).TotalSeconds;
                        aukcija.PreostaloVreme = -1;
                    else
                        aukcija.PreostaloVreme = (double)aukcija.Trajanje;
            }
            return View(aukcija);
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {

            if (checkAdmin())
                return RedirectToAction("Login", "Account");
            else
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
                        editAukcija.Status = "DRAFT";
                    }

                    using (var context = new IEPVebAukcijaEntities7())
                    {
                        context.Entry(editAukcija).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();

                        logger.Error("DELETE AUCTION: AuctionID: " + editAukcija.AukcijaID + ", AuctionStatus: " + editAukcija.Status);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Unable to delete auction");
                }

                return RedirectToAction("Index", "Admin", new { id = id });
            }

        }

        [HttpGet]
        public ActionResult AllAuctions(int? id, string sortOrder, string currentFilter, string searchString, string minP, string maxP, string AuctionStatus, int? page)
        {

            if (!checkLoginUser())
                return RedirectToAction("Login", "Account");
            else
            {
                bool onlyMin, onlyMax, minMax;
                onlyMin = onlyMax = minMax = false;

                if (!String.IsNullOrEmpty(minP)) onlyMin = true;
                if (!String.IsNullOrEmpty(maxP)) onlyMax = true;
                if (onlyMin && onlyMax)
                {
                    onlyMin = onlyMax = false;
                    minMax = true;
                }

                using (var context = new IEPVebAukcijaEntities7())
                {
                    ViewBag.CurrentSort = sortOrder;
                    ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                    ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

                    if (searchString != null)
                    {
                        page = 1;
                    }
                    else
                    {
                        searchString = currentFilter;
                    }

                    ViewBag.CurrentFilter = searchString;

                    IEnumerable<Veb_portal_za_aukcijsku_prodaju.Models.Aukcija> aukcijas = from user in context.Korisniks
                                                                                           from auc in user.Aukcijas
                                                                                           where user.KorisnikID == id
                                                                                           select auc;

                    if (!String.IsNullOrEmpty(searchString))
                    {
                        string[] words = searchString.Split(' ');
                        aukcijas = aukcijas.Where(s => s.Proizvod.Contains(searchString));
                    }
                    try
                    {
                        if (onlyMin || onlyMax || minMax)
                        {
                            if (minMax)
                            {
                                aukcijas = aukcijas.Where(s => s.TrenutnaCena >= Decimal.Parse(minP) && s.TrenutnaCena <= Decimal.Parse(maxP));
                            }
                            else
                            {
                                if (onlyMin)
                                {
                                    aukcijas = aukcijas.Where(s => s.TrenutnaCena >= Decimal.Parse(minP));
                                }
                                else
                                {
                                    aukcijas = aukcijas.Where(s => s.TrenutnaCena <= Decimal.Parse(maxP));
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error parsing double.");
                    }

                    if (!String.IsNullOrEmpty(AuctionStatus))
                    {

                        switch (AuctionStatus)
                        {

                            case "1":
                                aukcijas = aukcijas.Where(s => s.Status == "READY");
                                break;
                            case "2":
                                aukcijas = aukcijas.Where(s => s.Status == "OPEN");
                                break;
                            case "3":
                                aukcijas = aukcijas.Where(s => s.Status == "SOLD");
                                break;
                            case "4":
                                aukcijas = aukcijas.Where(s => s.Status == "EXPIRED");
                                break;

                        }
                    }

                    switch (sortOrder)
                    {
                        case "name_desc":
                            aukcijas = aukcijas.OrderByDescending(s => s.Proizvod);
                            break;
                        case "Date":
                            aukcijas = aukcijas.OrderBy(s => s.VremeKreiranja);
                            break;
                        case "date_desc":
                            aukcijas = aukcijas.OrderByDescending(s => s.VremeKreiranja);
                            break;
                        default:  // Name ascending 
                            aukcijas = aukcijas.OrderBy(s => s.Proizvod);
                            break;
                    }

                    foreach (Aukcija auk in aukcijas)
                    {
                        if (auk.BidID != null)
                        {
                            Bid bid = context.Bids.Find(auk.BidID);
                            Korisnik user = context.Korisniks.Find(bid.KorisnikID);

                            bid.Korisnik = user;
                            auk.Bid = bid;
                        }

                        if ((auk.VremeZatvaranja != null) && (!auk.VremeZatvaranja.Equals("")) && (auk.Status.Equals("OPEN")))
                            auk.PreostaloVreme = ((DateTime)auk.VremeZatvaranja - DateTime.Now).TotalSeconds;
                        else
                            if ((auk.VremeOtvaranja != null) && (!auk.VremeOtvaranja.Equals("")) && (!auk.Status.Equals("OPEN")))
                                //auk.PreostaloVreme = ((DateTime)auk.VremeZatvaranja - (DateTime)auk.VremeOtvaranja).TotalSeconds;
                                auk.PreostaloVreme = -1;
                            else
                                auk.PreostaloVreme = (double)auk.Trajanje;
                    }

                    int pageSize = 10;
                    int pageNumber = (page ?? 1);
                    return View(aukcijas.ToPagedList(pageNumber, pageSize));
                }
            }
        }

    }
}