using System;
using System.Collections.Generic;
using System.Text;
using Py.Logging;
using System.Data;

namespace Py.Sql.Mysql.Test {
    class Program {
        static void Main(string[] args) {

            using (DbHelper sql = new MySqlHelper()) {

                sql.SetConnectionString("localhost", "root", "root", "test");
                string conn = sql.ConnectionString;

                Logger.Info("最简单的SQL执行");

                sql.Execute("SELECT * FROM TableName");
                while (sql.Read())
                    Logger.Write(sql.GetString("Value"));
                //也是一个类库的简化操作

                sql.Sql = ("update tablename set `Time`=@Time where Sort=@Sort");

                sql.AddParameter("@Sort", (int?)null); // 这样防注入
                sql.AddParameter("@Time", DateTime.Now);

                sql.ExecuteCommits();

                sql.Execute("SELECT Time FROM TableName");
                while (sql.Read())
                    Logger.Write(sql.GetDateTime(0));

                Logger.Info("带参数的SQL执行");

                sql.Sql = "SELECT * FROM TableName WHERE Sort <= @Sort";

                sql.AddParameter("@Sort", 3);
                sql.Execute();
                while (sql.Read())
                    Logger.Write(sql.GetInt16("Sort", -1));

                Logger.Info("自动生成SQL语句");

                sql.SqlTable = "TableName";
                sql.SqlColumn = "Name, Value, Time";
                sql.SqlPrimaryKey = "Id";
                sql.CreateCommandText(SqlOperation.Select);
                sql.Execute();
                Logger.Write(sql.CanRead);
                while (sql.Read())
                    Logger.Write(sql.GetDateTime("Time"));

                sql.Sql = "SELECT   `Name` FROM      TableName WHERE   (`Name` = @Name)";

                sql.AddParameter("@Name", (object)"3");

                sql.Execute();

                Logger.Info("分页功能");

                Py.Core.PagerInfo p = new Py.Core.PagerInfo(3, -1, 1);
                sql.ExecutePageInfo(p);
                while (sql.Read())
                    Logger.Write(sql.GetInt("Value"));

                sql.SqlOrderby = "Sort";

                Logger.Write("第一页");
                int i;
                sql.ExecutePageInfo(1, out i);
                while (sql.Read())
                    Logger.Write(sql.GetInt("Value"));

                Logger.Write();
                Logger.Write("第二页");
                sql.ExecutePageInfo(2, out i);
                while (sql.Read())
                    Logger.Write(sql.GetInt("Value"));

                Logger.Write();
                Logger.Write("第三页");
                sql.ExecutePageInfo(3, out i);
                while (sql.Read())
                    Logger.Write(sql.GetInt("Value"));

                Logger.Info("数据集");
                sql.SqlCurrentPage = 0;
                sql.SqlColumn = "Id," + sql.SqlColumn;
                sql.CreateCommandText(SqlOperation.Select);
                object d = sql.DataSource;


                sql.DataSource = d;
                Logger.Write(d);




                Logger.Info("静态");
                Logger.Write(DbHelper.ExecuteNonQuery(sql.Connection, "UPDATE TableName SET `Value` = `Value` + 2 WHERE `Value` = 1", CommandType.Text));




                DbHelper.DefaultConnectionString = conn;

                MySqlHelper.ExecuteScalar("UPDATE TableName SET `Value` = `Value` + 2 WHERE `Value` = 1");

                Logger.Info("特殊用处");
                sql.ForEach(delegate(IDataReader r, int index) {
                    Logger.Write("第" + index, r.GetValue(2));
                    return true;
                });


            }



            Logger.Success();

            Console.Read();
        }
    }
}
