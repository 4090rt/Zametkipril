using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Forms;

namespace v22
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        /// 
        [STAThread]
        static void Main()
        {
            EnsureDatabaseCreated();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void EnsureDatabaseCreated()
        {
            try
            {
                string connectionString = "Data Source=UserBase.db";
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string createTableSql = @"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Login TEXT NOT NULL UNIQUE,
                            Password TEXT NOT NULL,
                            City TEXT,
                            Email TEXT,
                            CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";
                    using (var command = new SQLiteCommand(createTableSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    string createIndexSql = "CREATE INDEX IF NOT EXISTS idx_users_login ON Users(Login)";
                    using (var command = new SQLiteCommand(createIndexSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Тихо игнорируем, форма покажет ошибку при первой работе с БД
            }
        }
    }
}
