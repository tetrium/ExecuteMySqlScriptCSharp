using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoinDataBase
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("sps"))
            {

                    MysqlManager.GenerateStoredProcedures("");
               

            }
            else if (args.Length > 0 ){

                MysqlManager.GenerateStoredProcedures(args[0]);

            }
            if (args.Length == 0 )
            {
              //  MysqlManager.GenerateAllDataBase();
                MysqlManager.GenerateStoredProcedures("");

            }

            //   Console.ReadKey();

        }
    }
}
