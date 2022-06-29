using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace KBR.Services
{
    public static class GlobalService
    {
        public static Dictionary<string, object> Variables = new Dictionary<string, object>();
        public static List<int> CodeUsed = new List<int>();
        public static Random Rand = new Random();

        public const string TABLE_ACCOUNTS = "accounts";
        public const string TABLE_LOANS = "loans";

        static GlobalService()
        {
            Rand.Next();        
        }

        public static string CPFFormat(string cpf)
        {
            return Convert.ToUInt64(cpf).ToString(@"000\.000\.000\-00");
        }

        public static bool IsCpf(this string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
                return false;
            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cpf.EndsWith(digito);
        }


        public static bool IsNumeric(this string value)
        {
            return value.All(char.IsNumber);
        }

        public static int ExecuteNonQuery(string str)
        {
            var cmd = DataService.Connection?.CreateCommand();
            cmd.CommandText = str;
            return cmd.ExecuteNonQuery();
        }

        public static int ExecuteScalar(string str)
        {
            var cmd = DataService.Connection?.CreateCommand();
            cmd.CommandText = str;
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static MySqlDataReader ExecuteReader(string str)
        {
            var cmd = DataService.Connection?.CreateCommand();
            cmd.CommandText = str;
            return cmd.ExecuteReader();
        }

        public static bool CheckExists(string table, string where)
        {
            var r = ExecuteReader($"SELECT COUNT(*) FROM {table} WHERE {where};");
            var result = false;
            if (r.Read())
            {
                result = r.GetInt32(0) > 0;
            }
            r.Close();
            return result;
        }

        public static int CheckCount(string table, string where)
        {
            var r = ExecuteReader($"SELECT COUNT(*) FROM {table} WHERE {where};");
            var result = 0;
            if (r.Read())
            {
                result = r.GetInt32(0);
            }
            r.Close();
            return result;
        }
    }
}
