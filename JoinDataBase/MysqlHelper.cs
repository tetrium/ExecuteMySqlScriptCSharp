using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JoinDataBase
{
    class MysqlHelper
    {
        private static MySqlConnection connection;
        private static String connectionString;
        private bool databaseSelected = false;
        public MySqlConnection GetConnection() {
            connectionString =@ConfigurationManager.AppSettings["dbConnectionString"];
            connection = new MySqlConnection(connectionString);

            return connection;
        }
        public static string StripComments(string code)
        {
            var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
            return Regex.Replace(code, re, "$1");
        }
        public void ExecuteQuery(String sqlQuery)
        {
            String dbName = ConfigurationManager.AppSettings["dbName"];
            if (sqlQuery.Replace(" ", "").Replace(";", "").Equals("use"+ dbName)) {
               // Console.WriteLine("Recontenctando base de datos");
                connection.Close();
                connectionString = @ConfigurationManager.AppSettings["dbConnectionString"] + ";DATABASE=" + dbName;
                connection = new MySqlConnection(connectionString);
                connection.Open();


            }
            if (sqlQuery.Equals("")) { return; }

          
          

            MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = sqlQuery;
                //Assign the connection using Connection
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.Text;



            //Execute query
            cmd.ExecuteNonQuery();


        }

        public void ExecuteStoredProcedure(String sqlQuery)
        {
        
            MySqlScript cmd = new MySqlScript(connection, sqlQuery+";");
                cmd.Query = sqlQuery;
                cmd.Delimiter = "$$";
                cmd.Execute();
               
       
        }
    }
}
