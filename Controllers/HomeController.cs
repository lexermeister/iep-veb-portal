using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Veb_portal_za_aukcijsku_prodaju.Models;
using System.Data;
using System.Data.Entity;
using System.Net;
using PagedList;

namespace Veb_portal_za_aukcijsku_prodaju.Controllers
{
    public class HomeController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, string minP, string maxP, string AuctionStatus, int? page)
        {

            bool onlyMin, onlyMax, minMax, allOrNot = false;
            onlyMin = onlyMax = minMax = false;

            if ((sortOrder == null) && (currentFilter == null) && (searchString == null) && (minP == null) && (maxP == null) && (AuctionStatus == null) && (page == null))
                allOrNot = false;
            else
                allOrNot = true;

            if (!String.IsNullOrEmpty(minP)) onlyMin = true;
            if (!String.IsNullOrEmpty(maxP)) onlyMax = true;
            if (onlyMin && onlyMax)
            {
                onlyMin = onlyMax = false;
                minMax = true;
            }

            using (var context = new IEPVebAukcijaEntities7())
            {
                // NE DIRAJ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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

                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                IEnumerable<Aukcija> aukcijas;

                if (allOrNot == true)
                    aukcijas = context.Aukcijas.Include(a => a.Bid).Where(a => !a.Status.Equals("DRAFT"));
                else
                    aukcijas = context.Aukcijas.Include(a => a.Bid).Where(a => a.Status.Equals("OPEN") && !a.Status.Equals("DRAFT")).OrderByDescending(a => a.VremeOtvaranja).Take(5);

                if (!String.IsNullOrEmpty(searchString))
                {
                    string[] words = searchString.Split(' ');
                    //aukcijas = aukcijas.Where(s => s.Proizvod.Contains(searchString));

                    IEnumerable<Veb_portal_za_aukcijsku_prodaju.Models.Aukcija> tempAukc = null;
                    bool first = true;
                    foreach (string searchWord in words)
                    {
                        if ((!searchWord.Equals("")) && (!searchWord.Equals(" ")))
                        {
                            var tempAukcOneWord = aukcijas.Where(s => s.Proizvod.Contains(searchWord));

                            if (first)
                            {
                                tempAukc = tempAukcOneWord;
                                first = false;
                            }
                            else
                                tempAukc = tempAukc.Union(tempAukcOneWord.AsEnumerable());
                        }
                    }

                    aukcijas = tempAukc;
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
                // NE RADII!!!!!!!!!
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
                    {
                        double diff = ((DateTime)auk.VremeZatvaranja - DateTime.Now).TotalSeconds;
                        if (diff > 0)
                            auk.PreostaloVreme = diff;
                        else
                            auk.PreostaloVreme = -2;
                    }
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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}