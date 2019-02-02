using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Veb_portal_za_aukcijsku_prodaju.Models;
using Veb_portal_za_aukcijsku_prodaju.Models.Authentication;
using System.Text;
using log4net;

namespace Veb_portal_za_aukcijsku_prodaju.Controllers
{
    

    [Authorize]
    public class AccountController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private ILog log = LogManager.GetLogger("AccountControllerLogger"); 

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // Encoding strings (passwords...)
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public void loginSession(Korisnik korisnik)
        {
            Session["userID"] = korisnik.KorisnikID;
            Session["userFullName"] = korisnik.Ime + " " + korisnik.Prezime;
            Session["userEmail"] = korisnik.Email;
            Session["admin"] = korisnik.Admin;
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

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (returnUrl != null)
                if (returnUrl.Equals("/Account/LogOff"))
                {
                    Session.Clear();
                    Session.Abandon();
                }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            log.Info("usao sam u login");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var context = new IEPVebAukcijaEntities7())
            {

                if (context.Korisniks.Count() > 0)
                {
                    string encodedPassword = Base64Encode(model.Lozinka);
                    var user = context.Korisniks.SingleOrDefault(u => u.Email == model.Email && u.Lozinka == encodedPassword);
                    if (user != null)
                    {
                        loginSession(user);
                        if ((bool)user.Admin)
                        {
                            Console.WriteLine("");
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "E-mail adresa ili lozinka nisu korektni. Molimo Vas, pokušajte ponovo.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Ne postoji nalog sa Vašim podacima u bazi.");
                }
            }
            return View(model);
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {

            if (ModelState.IsValid)
            {
                using (var context = new IEPVebAukcijaEntities7())
                {
                    Korisnik userEx = context.Korisniks.SingleOrDefault(u => u.Email.Equals(model.Email));
                    if (userEx == null)
                    {

                        if (model.Lozinka.Equals(model.PotvrdaLozinke))
                        {
                            var newUser = new Korisnik()
                            {
                                Ime = model.Ime,
                                Prezime = model.Prezime,
                                Email = model.Email,
                                Lozinka = Base64Encode(model.Lozinka),
                                BrojTokena = 100,
                                Admin = false
                            };

                            context.Korisniks.Add(newUser);
                            context.SaveChanges();
                            //loginSession(newUser);

                            return RedirectToAction("Login", "Account");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Ponovo uneta lozinka se ne poklapa sa prvobitnom lozinkom.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Postoji već nalog sa Vašom e-mail adresom.");
                    }
                }
            }

            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult ViewProfile()
        {
            bool check = checkLoginUser();

            if (check)
            {
                int id = (int)Session["userID"];

                Korisnik korisnik = null;
                using (var context = new IEPVebAukcijaEntities7())
                {
                    korisnik = context.Korisniks.Find(id);
                    if (korisnik == null)
                    {
                        return HttpNotFound();
                    }
                }
                return View(korisnik);
            }

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/ChangeFirstName
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeFirstName(string newFirstName)
        {
            bool check = checkLoginUser();

            if (check)
            {
                try
                {
                    Korisnik editKorisnik;
                    int id = (int)Session["userID"];
                    using (var context = new IEPVebAukcijaEntities7())
                    {
                        editKorisnik = context.Korisniks.Find(id);
                    }

                    if (editKorisnik != null)
                    {
                        editKorisnik.Ime = newFirstName;
                    }

                    using (var context = new IEPVebAukcijaEntities7())
                    {
                        context.Entry(editKorisnik).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Unable to change first name.");
                    ModelState.AddModelError("", "*Neispravno uneto ime.");
                }

                return RedirectToAction("ViewProfile", "Account");
            }

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/ChangeFirstName
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeLastName(string newLastName)
        {
            bool check = checkLoginUser();

            if (check)
            {
                try
                {
                    Korisnik editKorisnik;
                    int id = (int)Session["userID"];
                    using (var context = new IEPVebAukcijaEntities7())
                    {
                        editKorisnik = context.Korisniks.Find(id);
                    }

                    if (editKorisnik != null)
                    {
                        editKorisnik.Prezime = newLastName;
                    }

                    using (var context = new IEPVebAukcijaEntities7())
                    {
                        context.Entry(editKorisnik).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Unable to change last name.");
                    ModelState.AddModelError("", "*Neispravno uneto prezime.");
                }

                return RedirectToAction("ViewProfile", "Account");
            }

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/ChangeFirstName
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeEmail(string newEmail)
        {
            bool check = checkLoginUser();

            if (check)
            {
                try
                {
                    Korisnik editKorisnik;
                    int id = (int)Session["userID"];
                    using (var context = new IEPVebAukcijaEntities7())
                    {
                        editKorisnik = context.Korisniks.Find(id);
                    }

                    if (editKorisnik != null)
                    {
                        editKorisnik.Email = newEmail;
                    }

                    using (var context = new IEPVebAukcijaEntities7())
                    {
                        context.Entry(editKorisnik).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Unable to change email.");
                    ModelState.AddModelError("", "*Neispravna e-mail adresa.");
                }
                return RedirectToAction("ViewProfile", "Account");
            }

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/ChangePassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPassword, string newPassword)
        {
            bool check = checkLoginUser();

            if (check)
            {
                try
                {
                    if ((oldPassword.Equals("")) || (oldPassword == null) || (newPassword.Equals("")) || (newPassword == null))
                    {
                        ModelState.AddModelError("", "*Niste uneli vrednosti u polja.");
                        return RedirectToAction("ViewProfile", "Account");
                    }

                    Korisnik editKorisnik;
                    int id = (int)Session["userID"];
                    using (var context = new IEPVebAukcijaEntities7())
                    {
                        editKorisnik = context.Korisniks.Find(id);
                    }

                    if (editKorisnik != null)
                    {
                        if (!Base64Decode(editKorisnik.Lozinka).Equals(oldPassword))
                        {
                            ModelState.AddModelError("", "*Stara lozinka se ne poklapa.");
                            return RedirectToAction("ViewProfile", "Account");
                        }

                        editKorisnik.Lozinka = Base64Encode(newPassword);
                    }

                    using (var context = new IEPVebAukcijaEntities7())
                    {
                        context.Entry(editKorisnik).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Unable to change email.");
                    ModelState.AddModelError("", "*Neispravna nova lozinka.");
                }
                return RedirectToAction("ViewProfile", "Account");
            }

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/BuyTokens
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult BuyTokens()
        {

            bool check = checkLoginUser();

            if (check)
            {
                int orderID;
                DateTime datumKreiranja = DateTime.Now;
                string status = "čekanje na obradu";

                using (var context = new IEPVebAukcijaEntities7())
                {
                    var newNarudzbina = new Narudzbina()
                    {
                        CenaTokena = null,
                        BrojTokena = 5,
                        DatumPravljenja = datumKreiranja,
                        Status = status,
                        KorisnikID = (int)Session["userID"]
                    };

                    context.Narudzbinas.Add(newNarudzbina);
                    context.SaveChanges();

                    orderID = newNarudzbina.NarudzbinaID;
                }

                string encodedID = Base64Encode("kljuc_" + orderID);

                using (var context = new IEPVebAukcijaEntities7())
                {
                    
                    Narudzbina narudzbina = context.Narudzbinas.Find(orderID);
                    Korisnik korisnik = context.Korisniks.Find(narudzbina.KorisnikID);

                    narudzbina.CenaTokena = (decimal)5;
                    narudzbina.BrojTokena = 20;
                    narudzbina.Status = "realizovana";

                    korisnik.BrojTokena += 20;

                    context.Entry(narudzbina).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    context.Entry(korisnik).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                

                var CentiliLink = "https://stage.centili.com/payment/widget?apikey=c1b12df447e0a198c459bc2505960ca7";
                return Redirect(CentiliLink);
               
             //   return RedirectToAction("ViewProfile", "Account");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public HttpStatusCodeResult CentiliEndPoint(string userid)
        {
            
            return new HttpStatusCodeResult(200);
        } 

        
        public ActionResult EndPoint(string clientid, string status, int amount, double enduserprise)
        {

            string decodedID = Base64Decode(clientid);
            string[] splitedWords = decodedID.Split('_');

            int id = Int32.Parse(splitedWords[0]);

            using (var context = new IEPVebAukcijaEntities7())
            {
                if (status.Equals("success"))
                {
                    Narudzbina narudzbina = context.Narudzbinas.Find(id);
                    Korisnik korisnik = context.Korisniks.Find(narudzbina.KorisnikID);

                    narudzbina.CenaTokena = (decimal)enduserprise;
                    narudzbina.BrojTokena = amount;
                    narudzbina.Status = "realizovana";

                    korisnik.BrojTokena += amount;

                    context.Entry(narudzbina).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    context.Entry(korisnik).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    Narudzbina narudzbina = context.Narudzbinas.Find(id);

                    narudzbina.Status = "poništena";

                    context.Entry(narudzbina).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
            }

            return ViewProfile();
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            //Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

    }
}