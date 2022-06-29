using KBR.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace KBR.Controllers
{
    public class LoanController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["session-token"];
            var accountId = 0;
            var haveAccess = false;

            // Get account id by token
            var r = ExecuteReader($"SELECT id, access FROM {TABLE_ACCOUNTS} WHERE token='{token}';");
            if (r.Read())
            {
                accountId = r.GetInt32("id");
                haveAccess = r.GetBoolean("access");
            }
            r.Close();

            r = ExecuteReader($"SELECT value FROM {TABLE_LOANS} WHERE account_id={accountId};");
            var countSimulates = 0;
            var countTotalValue = 0d;
            while (r.Read())
            {
                countSimulates++;
                var value = r.GetString("value");
                countTotalValue += Convert.ToDouble(value.Split(";")[0], CultureInfo.InvariantCulture);
            }
            r.Close();

            var loan = new LoanModel
            {
                HaveAccess = haveAccess,
                CountSimulates = countSimulates,
                TotalValueSimulate = countTotalValue,
            };
            return View(loan);
        }


        public IActionResult Simulate()
        {
            return View();
        }

        public async Task<IActionResult> Simulation(string value, int parc)
        {
            var val = Convert.ToDouble(value, CultureInfo.InvariantCulture) ;
            if (val < 5000d)
                return RedirectToAction("Index");

            var perc = new[]{17.9d, 9.4d,5.2d,3.7d};
            var parcId = 0;
            if (parc == 12) parcId = 1;
            if (parc == 24) parcId = 2;
            if (parc == 36) parcId = 3;

            var sim = new SimulateModel();
            sim.QuotaCount = parc;
            sim.Value = val;
            sim.Percentage = perc[parcId];

            ViewData["teste"] = val;

            for (int i = 0; i < parc; i++)
                sim.QuotaValue.Add((val * (1 + (sim.Percentage / 100d)) * (Math.Pow(1.012d, i + 1))) / (double)parc);

            var token = Request.Cookies["session-token"];
            var accountId = 0;

            // Get account id by token
            var r = ExecuteReader($"SELECT id FROM {TABLE_ACCOUNTS} WHERE token='{token}';");
            if (r.Read())
                accountId = r.GetInt32("id");
            r.Close();

            var valueStr = $"{val};{sim.QuotaCount};{sim.Percentage};";
            foreach (var i in sim.QuotaValue)
                valueStr += $"{i};";

            // Remove the last ;
            valueStr = valueStr.Remove(valueStr.Length - 1, 1);

            ExecuteNonQuery(@$"INSERT INTO {TABLE_LOANS}(account_id, date_loan, value) VALUES(
{accountId},'{DateTime.Now.ToShortDateString()}', '{valueStr}');");

            return View(sim);
        }

        public IActionResult ExportCSV()
        {
            var builder = new StringBuilder();
            builder.AppendLine(@"Id,Email,CPF,Data,Valor,Parcelas");

            var r = ExecuteReader(@$"SELECT l.id, l.date_loan, l.value, a.email, a.cpf
FROM {TABLE_LOANS} l JOIN {TABLE_ACCOUNTS} a ON a.id = l.account_id");
            while (r.Read())
            {
                var value = Convert.ToDouble(r.GetString(2).Split(";")[0], CultureInfo.InvariantCulture).ToString("N2");
                var parc  = r.GetString(2).Split(";")[1];

                builder.AppendLine($@"{r.GetInt32(0)},{r.GetString(3)},{CPFFormat(r.GetString(4))},{r.GetString(1)},{value},{parc}");
            }
            r.Close();

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"loans-{Rand.Next(1, 9999)}{DateTime.Now.ToShortTimeString()}.csv");
        }
    }
}
