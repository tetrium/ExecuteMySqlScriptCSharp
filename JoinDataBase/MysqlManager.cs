using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoinDataBase
{
    class MysqlManager
    {

        private static void ExecuteQuery(String sqlQuery) {
            sqlQuery = MysqlHelper.StripComments(sqlQuery);

            var mysqlHelper = new MysqlHelper();
            List<String> storedProcedureAndTablesQueries = sqlQuery.Split(new string[] { "DELIMITER $$" }, StringSplitOptions.None).ToList();
            List<String> tablesQueries = new List<String>();
            List<String> storedProcedureQueries = new List<String>();
            String queryToExecute = "";
            if (storedProcedureAndTablesQueries.Count == 1)
            {
                if (storedProcedureAndTablesQueries[0].Contains("$$"))
                {
                    storedProcedureQueries = storedProcedureAndTablesQueries[0].Split(new string[] { "$$" }, StringSplitOptions.None).ToList();
                }
                else {
                    tablesQueries = storedProcedureAndTablesQueries[0].Split(';').ToList();

                }

            }
            if (storedProcedureAndTablesQueries.Count > 1)
            {

                tablesQueries = storedProcedureAndTablesQueries[0].Split(';').ToList();
                storedProcedureQueries = storedProcedureAndTablesQueries[1].Split(new string[] { "$$" }, StringSplitOptions.None).ToList();

            }
          



            using (var connection =mysqlHelper.GetConnection())
            {
                try
                {
                    connection.Open();

                    Console.WriteLine($"MySQL version : {connection.ServerVersion}");



                }
                catch (Exception ex) {
                    Console.WriteLine("Fail Opening DbConnection: "+ex.Message+" connection string: "+ @ConfigurationManager.AppSettings["dbConnectionString"]);
                    return;
                }
                if (tablesQueries.Count > 0)
                {
                    Console.WriteLine("Executing queries");


                    try
                    {

                        foreach (String query in tablesQueries)
                        {
                            queryToExecute = query;
                            queryToExecute = queryToExecute.Replace("\r", "").Trim();
                            queryToExecute = queryToExecute.Replace("\n", " ");

                            //  Console.WriteLine("\nExecuting Query "+query);
                            mysqlHelper.ExecuteQuery(queryToExecute);

                        }


                    }
                    catch (Exception ex)
                    {
                        connection.Close();

                        Console.WriteLine("\nFail Executing Query: " + queryToExecute + "\n ErrorDescription:" + ex.Message + "\n\n");
                        return;
                    }

                }
                if (storedProcedureQueries.Count > 0)
                {
                    Console.WriteLine("\n Executing Stored Procedure(s)");
                    try
                    {
                        foreach (String query in storedProcedureQueries)
                        {
                           
                            queryToExecute = query;
                            queryToExecute = queryToExecute.Replace("\r", "").Trim();
                            queryToExecute = queryToExecute.Replace("\n", " ");
                            if (queryToExecute.Equals("")) continue;
                            //  Console.WriteLine("\nExecuting StoredProcedure: "+query);
                            mysqlHelper.ExecuteStoredProcedure(queryToExecute);

                        }
                        connection.Close();


                    }
                    catch (Exception ex)
                    {
                        connection.Close();

                        Console.WriteLine("\nFail Executing Query: " + queryToExecute + "\n ErrorDescription: " + ex.Message.Replace(";",";\n") + "\n\n");
                        return;
                    }
                }

                Console.WriteLine("\nSuccess ");





            }
        }

        public static void GenerateStoredProcedures(String storedProcedureName)
        {


            StringBuilder sb = new StringBuilder();
            String sqlQuery = "";
            String sqlQueryStoredProcedure = "";

            try
            {
                string[] storedProcedures = Directory.GetFiles(@ConfigurationManager.AppSettings["storedProceduresPath"], "*.*", SearchOption.AllDirectories);


                Console.WriteLine("Starting ");

                StringBuilder sbStoredProcedures = new StringBuilder();




                String dbName = ConfigurationManager.AppSettings["dbName"];
                sb.Append( "use " + dbName + ";\n\n ");
                foreach (var file in storedProcedures)
                {
                    FileInfo info = new FileInfo(file);
                    // Console.WriteLine("Join " + info.FullName);
                  
                    if (info.Extension == ".sql")
                    {

                        String filecontet = File.ReadAllText(file);
                        sbStoredProcedures.Append(filecontet + "$$\n\n");

                        //   Console.WriteLine("info.Name " + info.Name);
                        if (info.Name.Equals(storedProcedureName+".sql")&&!filecontet.Equals(""))
                        {
                            String delete = "use "+ dbName + ";\n\n  drop procedure if exists " + info.Name.Split('.')[0] + ";\n\n";
                            sqlQueryStoredProcedure = delete+" DELIMITER $$\n\n" +filecontet+"$$\n\n";
                         }
                        sb.Append("drop procedure if exists " + info.Name.Split('.')[0] + ";\n\n");

                        // sb.Append(filecontet+"$$\n\n");
                    }


                    // Do something with the Folder or just add them to a list via nameoflist.add();
                }

                sb.Append("DELIMITER $$\n\n" + sbStoredProcedures.ToString());

                String filePath = ConfigurationManager.AppSettings["dbPath"]  + ConfigurationManager.AppSettings["dbName"] + "_stored_procedures.sql";
                sqlQuery = "/*" + DateTime.Now + "*/\n" + sb.ToString();
                File.WriteAllText(filePath, sqlQuery);
                Console.WriteLine("\nStored Procedures Generated In " + filePath);
            }
            catch (Exception ex) {
                Console.WriteLine("\nError :" + ex.Message);
                return;
            }




            if (storedProcedureName.Equals("") && storedProcedureName.Length <= 0)
            {
                ExecuteQuery(sqlQuery);
                return;
            }


            if (sqlQueryStoredProcedure.Equals("")&& sqlQueryStoredProcedure.Length<=0)
            {
                Console.WriteLine("\nError : Stored procedure dont exists in file path "+storedProcedureName);


            }
            else {
                Console.WriteLine("\nExecuting Stored Procedure: " + storedProcedureName);

                ExecuteQuery(sqlQueryStoredProcedure);


            }


        }


        public static void GenerateAllDataBase() {
            String sqlQuery = "";

            try
            {

                StringBuilder sb = new StringBuilder();
            string[] storedProcedures = Directory.GetFiles(@ConfigurationManager.AppSettings["storedProceduresPath"], "*.*", SearchOption.AllDirectories);
            string[] schema = Directory.GetFiles(@ConfigurationManager.AppSettings["schemaPath"], "*.*", SearchOption.AllDirectories);

            List<String> tables = new List<string>();
            List<String> constraints = new List<string>();
            List<String> inserts = new List<string>();

            Console.WriteLine("\nStarting Generating All DataBase");
            // sb.Append(File.ReadAllText("linbuk_db/schema/tables.sql")+"\n\n/******************    STORED PROCEDURES      ******************/\n\n\n");
            foreach (var file in schema)
            {

                String s = File.ReadAllText(file);
                string[] ss = s.Split(new string[] { "----------------------------------------------------" }, StringSplitOptions.None);
                int i = 0;
                foreach (var s1 in ss)
                {
                    if (i == 0)
                    {
                        tables.Add(s1);

                    }
                    if (i == 1)
                    {
                        constraints.Add(s1);

                    }
                    if (i == 2)
                    {
                        inserts.Add(s1);

                    }
                    i++;
                }

            }
            sb.Append("/******************    TABLES     ******************/\n\n\n");

            foreach (var table in tables)
            {
                sb.Append("\n" + table + "\n");

            }

            sb.Append("/******************    INSERTS     ******************/\n\n\n");

            foreach (var insert in inserts)
            {
                sb.Append("\n" + insert + "\n");

            }
            sb.Append("/******************    CONSTRAINTS     ******************/\n\n\n");

            foreach (var constraint in constraints)
            {
                sb.Append("\n" + constraint + "\n");

            }


            sb.Append("/******************    StoredProcedures     ******************/\n\n\n");

                StringBuilder sbStoredProcedures = new StringBuilder();

                foreach (var file in storedProcedures)
                {
                    FileInfo info = new FileInfo(file);

                    if (info.Extension == ".sql")
                    {
                        String filecontet = File.ReadAllText(file);
                        sbStoredProcedures.Append(filecontet+"$$\n\n");
                        sb.Append( "drop procedure if exists "+info.Name.Split('.')[0]+";\n\n");
                    }
                }

                sb.Append("DELIMITER $$ \n\n"+ sbStoredProcedures.ToString());


                /*
                foreach (var file in storedProcedures)
            {
                FileInfo info = new FileInfo(file);

                if (info.Extension == ".sql")
                {
                    String filecontet = File.ReadAllText(file);
                    sb.Append(filecontet+"\n\n");
                }
            }
           */


          sqlQuery = "/* \nhttp://localhost:8080/phpmyadmin/\nDROP USER 'kn85jt6btdv8'@'localhost' ;\nCREATE USER 'kn85jt6btdv8'@'localhost' IDENTIFIED BY 'scorpio';\nGRANT ALL PRIVILEGES ON *.* TO 'kn85jt6btdv8'@'localhost';\nFLUSH PRIVILEGES; " + DateTime.Now + " */\n" + "DROP DATABASE IF EXISTS linbuk_db;  \n  commit;    \n  create database linbuk_db;   \n   use linbuk_db;\n\n\n" + sb.ToString();
            String filePath = @ConfigurationManager.AppSettings["dbPath"] +ConfigurationManager.AppSettings["dbName"] + ".sql";
            File.WriteAllText(filePath, sqlQuery);
            Console.WriteLine("\nDatabase Generated In " + filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError :" + ex.Message);
                return;
            }

            ExecuteQuery(sqlQuery);


        }


    }
}
