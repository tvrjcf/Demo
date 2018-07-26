using System;
using System.Collections.Generic;
using System.Text;
using Py.Sql;
using System.Data.OleDb;

namespace Py.Demo {
    public class SqlTest : DemoBase {
        public override void Start() {

            B();

//Console.WriteLine(
//            Py.Core.Until.RunTime(A, 10)  
//            );



//            Console.WriteLine(
//            Py.Core.Until.RunTime(B, 10)
//            );
        }


        void A() {

            using (OleDbConnection con = new OleDbConnection()) {
                con.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=F:\\wz\\Py\\Py.Core\\预览\\Demo\\Demo\\Data\\DbHelper.mdb";
                con.Open();
                var command = con.CreateCommand();
                command.CommandText = "SELECT * FROM TableName";

                var reader = command.ExecuteReader();

                while (reader.Read()) ;
            }
        }


        void B() {

            using (DbHelper sql = new OleDbHelper()) {

                sql.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=F:\\wz\\Py\\Py.Core\\预览\\Demo\\Demo\\Data\\DbHelper.mdb";

                sql.Execute("SELECT * FROM TableName");
                while (sql.Read()) ;

            }
        }
    }
}
