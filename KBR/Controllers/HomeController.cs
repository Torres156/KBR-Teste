using KBR.Models;
using KBR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Collections.Specialized;
using System.Diagnostics;

namespace KBR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

            if (!Variables.ContainsKey("login-msg"))
                Variables.Add("login-msg", "");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Forgot()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegisterNew(string email, string pwd, string cpf)
        {
            if (cpf.Length < 11 || !cpf.IsNumeric() || !cpf.IsCpf())
                return View("register", "CPF inválido!");

            if (CheckExists(TABLE_ACCOUNTS, $"email='{email}'"))
                return View("register", "Esse email já está em uso!");

            if (CheckExists(TABLE_ACCOUNTS, $"cpf='{cpf}'"))
                return View("register", "Esse CPF já está em uso!");

            ExecuteNonQuery($"INSERT INTO {TABLE_ACCOUNTS} (email, password, cpf, token, access) VALUES('{email}', '{pwd}', '{cpf}', '', false)");

            return View("register","Criado com sucesso!");
        }

        [HttpPost]
        public IActionResult ForgotPassword(string cpf)
        {
            if (!CheckExists(TABLE_ACCOUNTS, $"cpf='{cpf}'"))
                return View("forgot", "Não há email associado a esse CPF!");

            var r = ExecuteReader($"SELECT email FROM {TABLE_ACCOUNTS} WHERE cpf='{cpf}';");
            var email = "";
            if (r.Read())
                email = r.GetString(0);
            r.Close();

            return View("forgot",$"[/]{email}");
        }

        [HttpPost]
        public IActionResult CheckCode(string code)
        {
            ViewData["error"] = "";
            if (Request.Cookies.ContainsKey("authentical-id") && Request.Cookies.ContainsKey("authentical-code"))
            {
                var cookieCode = Request.Cookies["authentical-code"];

                if (cookieCode == code)
                {
                    var token = Guid.NewGuid().ToString();
                    // If exists a token, delete it
                    if (!String.IsNullOrEmpty(Request.Cookies["session-token"]))
                        Response.Cookies.Delete("session-token");

                    ExecuteNonQuery($"UPDATE {TABLE_ACCOUNTS} SET token='{token}' WHERE id={Request.Cookies["authentical-id"]};");

                    Response.Cookies.Append("session-token", token);                    
                    return Redirect("/Loan/");
                }
                else
                    ViewData["error"] = "Código incorreto!";
            }
            return View();
        }

        [HttpPost]
        public IActionResult Check(string email, string[] cpf)
        {
            Variables["login-msg"] = "";

            if (!CheckExists(TABLE_ACCOUNTS, $"email='{email}'"))
            {
                Variables["login-msg"] = "E-mail não cadastrado!";
                goto Err;
            }

            // Load data values
            var dataCpf = "";
            var dataId  = 0;
            var r = ExecuteReader($"SELECT id,cpf FROM {TABLE_ACCOUNTS} WHERE email='{email}';");
            if (r.Read())
            {
                dataId  = r.GetInt32("id");
                dataCpf = r.GetString("cpf");
            }
            r.Close();

            // Check if value cpf is equal as database cpf
            var currentCpf = cpf[0] + cpf[1] + cpf[2] + cpf[3];
            if (!currentCpf.Equals(dataCpf.Substring(0, 4)))
            {
                Variables["login-msg"] = "CPF incorreto!";
                goto Err;
            }

            // Create cookie
            var cookieOptions = new CookieOptions()
            {
                Secure  = true,
                Expires = DateTime.Now.AddMinutes(5),
            };

            // Delete if authentical-id exists
            if (Request.Cookies.ContainsKey("authentical-id"))
                Response.Cookies.Delete("authentical-id");

            Response.Cookies.Append("authentical-id", dataId.ToString(), cookieOptions);

            int code = Rand.Next(100000, 999999);
            while (CodeUsed.Contains(code))
            {
                code = Rand.Next(100000, 999999);
            };
            CodeUsed.Add(code);

            // Delete if authentical-code exists
            if (Request.Cookies.ContainsKey("authentical-code"))
                Response.Cookies.Delete("authentical-code");

            Response.Cookies.Append("authentical-code", code.ToString(), cookieOptions);

            return View("CheckCode");

        Err:;
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}