using MySql.Data.MySqlClient;

namespace KBR.Services
{
    public static class DataService
    {
        const string DatabaseName = "kbrteste";
        const string IP           = "localhost";
        const int    PORT         = 3306;
        const string User         = "root";
        const string Password     = "156156";

        public static MySqlConnection? Connection { get; private set; }

        public static bool isOpened = false;

        public static void Initialize()
        {
            string str = $"server={IP};port={PORT};user id={User};pwd={Password};";

            Connection = new MySqlConnection(str);
            Open();

            CheckDatabase();
            ExecuteNonQuery($"USE {DatabaseName};");

            CheckTables();


            if (!CheckExists(TABLE_ACCOUNTS, "email='admin@admin.com'"))
                ExecuteNonQuery($"INSERT INTO {TABLE_ACCOUNTS} (email, password, cpf, token, access) VALUES('admin@admin.com', 'admin', '12345678910', '', true)");            
        }

        public static void Open()
        {
            if (!isOpened)
            {
                Connection?.Open();
                isOpened = true;
            }
        }

        public static void Close()
        {
            if (isOpened)
            {
                Connection?.Close();
                isOpened = false;
            }
        }

        static void CheckDatabase()
        {
            ExecuteNonQuery($"CREATE DATABASE IF NOT EXISTS {DatabaseName};");
        }

        static void CheckTables()
        {
            // Contas
            var str = $@"CREATE TABLE IF NOT EXISTS {TABLE_ACCOUNTS}(id INT NOT NULL AUTO_INCREMENT,
email VARCHAR(255), password VARCHAR(255), cpf VARCHAR(11), token TEXT, access BOOLEAN, PRIMARY KEY (id));";
            ExecuteNonQuery(str);

            str = $@"CREATE TABLE IF NOT EXISTS {TABLE_LOANS}(id INT NOT NULL AUTO_INCREMENT,
account_id INT, date_loan VARCHAR(20), value VARCHAR(255), PRIMARY KEY (id));";
            ExecuteNonQuery(str);

        }
    }
}
