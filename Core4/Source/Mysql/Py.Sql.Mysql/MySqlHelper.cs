/* *****************************************************************************
 *
 * Copyright (c) 2009-2010 Xuld. All rights reserved.
 * 
 * Project Url: http://play.xuld.net
 * 
 * This source code is part of the Project Play.Core for .Net .
 * 
 * This code is licensed under Play.Core Licence.
 * See the file License.html for the license details.
 * 
 * 
 * You must not remove this notice, or any other, from this software.
 *
 * 
 * SqlHelper.cs  by xuld
 * 
 * ******************************************************************************/




using System;
using System.Data;
using System.Data.MySqlClient;
using System.Data.Common;
using Py.RunTime;

namespace Py.Sql {

	/// <summary>
	/// 处理 SqlServer 的数据库处理类。
	/// </summary>
	[CLSCompliant(true)]
	public class MySqlHelper : DbHelper, IDisposable {

		#region 私有变量

		/// <summary> 
		/// 操作的SqlDataReader对象。
		/// </summary> 
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		MySqlDataReader _dataReader;

		/// <summary> 
		/// 操作的SqlConnection对象。
		/// </summary> 
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		MySqlConnection _connection;

		/// <summary> 
		/// 操作的SqlCommand对象。
		/// </summary> 
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		MySqlCommand _command;

		#endregion

		#region 公共属性

        /// <summary>
        /// 获得正操作的DataReader对象。
        /// </summary>
		public override IDataReader DataReader {
			get {
				return _dataReader;
            }
            protected set {
                _dataReader = (MySqlDataReader)value;
            }
		}

		/// <summary> 
		/// 获得正操作的DbConnection对象。
		/// </summary> 
		public override IDbConnection Connection {
			get {
				return _connection;
            }
		}

		/// <summary> 
		/// 获得正操作的 DbCommand对象。
		/// </summary> 
		public override IDbCommand Command {
			get {
				return _command;
			}
		}

		#endregion

		#region 连接

		/// <summary> 
		/// 初始化 Play.Sql.MySqlDbHelper 类的新实例。
		/// </summary>
		public MySqlHelper() : base() { }

        /// <summary>
        /// 初始化 Play.Sql.DbHelper 类的新实例。
        /// </summary>
        /// <param name="connection">已创建的连接的实例。</param>
        /// <exception cref="InvalidCastException">给的连接不符合当前类的标准。</exception>
        public MySqlHelper(MySqlConnection connection) {
			Thrower.ThrowArgumentNullExceptionIf(connection, "connection");
            _connection = connection;
            _command = _connection.CreateCommand();
        }

        /// <summary>
        /// 初始化 Play.Sql.DbHelper 类的新实例。
        /// </summary>
        /// <param name="command">已创建的命令的实例。</param>
		/// <exception cref="ArgumentNullException"><paramref name="command"/> 为空。</exception>
        public MySqlHelper(MySqlCommand command) {
			Thrower.ThrowArgumentNullExceptionIf(command, "command");
            _connection = command.Connection;
            _command = command;
        }

		/// <summary> 
		/// 使用指定的数据字符初始化 Play.Sql.MySqlDbHelper 类的新实例。
		/// </summary>
        /// <param name="connString">数据源或连接字符串。服务器的Ip地址、域名、文件名或机器名。如 "localhost"、 ".\SqlEXPRESS" 。</param> 
		/// <exception cref="ArgumentNullException"><paramref name="connString"/> 为空。</exception>
		public MySqlHelper(string connString) : base(connString) { }

        /// <summary>
        /// 使用指定的数据字符,数据库密码初始化 Play.Sql.MySqlDbHelper 类的新实例。
        /// </summary>
        /// <param name="dataSource">数据源。服务器的Ip地址、域名、文件名或机器名。如 "localhost"、 ".\SqlEXPRESS" 。</param> 
        /// <param name="userName">登录数据库的用户名字。可空。当不使用用户名时，将使用默认身份验证。（如SqlSever的 windows 身份验证。）</param>
        /// <param name="password">登录数据库的用户密码。可空。</param>
		/// <exception cref="ArgumentNullException"><paramref name="dataSource"/> 为空。</exception>
		public MySqlHelper(string dataSource, string userName, string password)
			: base(dataSource, userName, password) { }

		/// <summary> 
		/// 使用指定的数据字符,用户名,密码初始化 Play.Sql.MySqlDbHelper 类的新实例。
		/// </summary>
        /// <param name="dataSource">数据源。服务器的Ip地址、域名、文件名或机器名。如 "localhost"、 ".\SqlEXPRESS" 。</param> 
        /// <param name="userName">登录数据库的用户名字。可空。当不使用用户名时，将使用默认身份验证。（如SqlSever的 windows 身份验证。）</param>
        /// <param name="password">登录数据库的用户密码。可空。</param>
        /// <param name="database">附加的数据库文件或默认使用的数据库。</param>
		/// <exception cref="ArgumentNullException"><paramref name="dataSource"/> 为空。</exception>
		public MySqlHelper(string dataSource, string userName, string password, string database)
			: base(dataSource, userName, password, database) { }


		/// <summary>
		/// 使用已有的辅助类初始化 <see cref="Py.Sql.SqlHelper"/> 的新实例，新实例和参数使用同一个连接。
		/// </summary>
		/// <param name="helper">The helper。</param>
		/// <exception cref="ArgumentNullException"><paramref name="helper" /> 为空。</exception>
		/// <exception cref="ArgumentException">传递的辅助类和当前实例的类型不相同。</exception>
        public MySqlHelper(DbHelper helper) {
			Thrower.ThrowArgumentNullExceptionIf(helper, "helper");
            _connection = helper.Connection as MySqlConnection;
            Thrower.ThrowArgumentExceptionIf(_connection == null, "连接空");
		}

        /// <summary>
        /// 当被子类重写时，初始化连接。
        /// </summary>
        protected override void CreateConnection() {
            _connection = new MySqlConnection();
            _command = _connection.CreateCommand();
        }

		/// <summary>
		/// 创建并返回一个与当前使用的连接关联的 <see cref="T:System.Data.SqlClient.SqlCommand" /> 对象。
		/// </summary>
		/// <returns>创建的 <see cref="T:System.Data.SqlClient.SqlCommand" /> 。</returns>
		public override IDbCommand CreateCommand() {
			return _connection.CreateCommand();
		}

        /// <summary> 
        /// 返回指定数据库连接的字符串。
        /// </summary>  
        /// <param name="dataSource">数据源。服务器的Ip地址、域名、文件名或机器名。如 "localhost"、 ".\SqlEXPRESS" 。</param> 
        /// <param name="userName">登录数据库的用户名字。可空。当不使用用户名时，将使用默认身份验证。（如SqlSever的 windows 身份验证。）</param>
        /// <param name="password">登录数据库的用户密码。可空。</param>
        /// <param name="database">附加的数据库文件或默认使用的数据库。</param>
        /// <param name="connTimeOut">连接服务器超时的时间。</param>
        /// <param name="openTimeOut">保持打开连接超时的时间。</param>
        /// <returns>连接字符串。</returns>
		protected override string GetConnectionString(string @dataSource, string userName, string password, string database, int openTimeOut, int connTimeOut) {

            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(ConnectionString);

            builder.Server = dataSource;

            builder.UserID = userName;

            builder.Password = password;

            builder.Database = database;

            if (openTimeOut >= 0)
                builder.ConnectTimeout = (uint)openTimeOut;

            if (connTimeOut >= 0)
                builder.ConnectionLifeTime = (uint)connTimeOut;

            return builder.ConnectionString;

		}


		#endregion

		#region SQL

        /// <summary>
        /// 添加命名参数。
        /// </summary>
        /// <param name="name">要查找的列的名字。</param>
        /// <param name="value">执行的命令的参数的值。</param>
        /// <returns>对所添加的参数的引用。</returns>
        /// <example><code>
        /// using System;
        /// using Play.Sql;
        /// class Sample{
        /// static void Main(){
        /// DbHelper Sql = new OledbDbHelper("~/App_Code/DbHelper.mdb");
        /// Sql.SetSql("Select * from [TABLE] where id&gt;@id");
        /// Sql.AddParameter("id", 200);
        /// }
        /// }
        /// </code></example>
		public override IDbDataParameter AddParameter(string name, object value) {
            if (value == null)
                return _command.Parameters.AddWithValue(name, DBNull.Value);
            else if (value is Enum)
                return _command.Parameters.AddWithValue(name, (int)value);
            else 
				return _command.Parameters.AddWithValue(name, value);
		}

        /// <summary>
        /// 添加命名参数。
        /// </summary>
        /// <param name="name">要查找的列的名字。</param>
        /// <param name="value">执行的命令的参数的值。</param>
        /// <returns>对所添加的参数的引用。</returns>
		public override IDbDataParameter AddParameter(string name, bool value) {
			return _command.Parameters.AddWithValue(name, value);
		}

        /// <summary>
        /// 添加命名参数。
        /// </summary>
        /// <param name="name">要查找的列的名字。</param>
        /// <param name="value">执行的命令的参数的值。</param>
        /// <returns>对所添加的参数的引用。</returns>
        /// <see cref="DbHelper.AddParameter(string, object)"/>
		public override IDbDataParameter AddParameter(string name, string value) {
            if(value == null)
                return _command.Parameters.AddWithValue(name, DBNull.Value);
            return _command.Parameters.AddWithValue(name, value);

		}


        /// <summary>
        /// 添加命名参数。
        /// </summary>
        /// <param name="name">要查找的列的名字。</param>
        /// <param name="value">执行的命令的参数的值。</param>
        /// <returns>对所添加的参数的引用。</returns>
        /// <see cref="DbHelper.AddParameter(string, object)"/>
		public override IDbDataParameter AddParameter(string name, int value) {
			return _command.Parameters.AddWithValue(name, value);
		}

        /// <summary>
        /// 增加命名参数。
        /// </summary>
        /// <param name="name">要查找的列的名字。</param>
        /// <param name="value">执行的命令的参数的值。</param>
        /// <param name="type">执行的命令的参数类型。</param>
        /// <returns>对所添加的参数的引用。</returns>
        /// <see cref="DbHelper.AddParameter(string, object)"/>
		public IDbDataParameter AddParameter(string name, object value, SqlDbType type) {
            IDbDataParameter p = _command.Parameters.AddWithValue(name, type);
            p.Value = value;
            return p;
		}

		/// <summary>
        /// 增加命名参数。
		/// </summary>
		/// <param name="name">执行的命令的参数名。</param>
		/// <param name="value">执行的命令的参数的值。</param>
        /// <param name="type">执行的命令的参数类型。</param>
        /// <param name="size">执行的命令的参数大小。</param>
        /// <returns>对所添加的参数的引用。</returns>
		public IDbDataParameter AddParameter(string name, object value, SqlDbType type, int size) {
            IDbDataParameter p = _command.Parameters.AddWithValue(name, type);
            p.Value = value;
            return p;
		}

        /// <summary>
        /// 添加命名参数。
        /// </summary>
        /// <param name="name">执行的命令的参数名。</param>
        /// <param name="value">执行的命令的参数的值。</param>
        /// <returns>对所添加的参数的引用。</returns>
		public override IDbDataParameter AddParameter(string name, DateTime value) {
			return _command.Parameters.AddWithValue(name, value);
		}

        /// <summary>
        /// 添加命名参数，不添加值。
        /// </summary>
        /// <param name="name">要查找的列的名字。</param>
        /// <returns>对所添加的参数的引用。</returns>
        public override IDbDataParameter AddParameter(string name) {
            return _command.Parameters.Add(new MySqlParameter(name, null));
        }

        /// <summary>
        /// 添加命名参数。
        /// </summary>
        /// <param name="d">参数键/值的集合。</param>
		/// <exception cref="ArgumentNullException"><paramref name="d" /> 为空。</exception>
		public override void AddParameters(System.Collections.IDictionary d) {
			Thrower.ThrowArgumentNullExceptionIf(d, "d");
			foreach(string v in d) {
				_command.Parameters.AddWithValue(v, d[v]);
			}
		}

		#endregion



        /// <summary> 
        /// 执行一个命令，统计条数。
        /// </summary> 
        /// <param name="tableName">要统计的表名。</param>
        /// <returns>个数。</returns>
        /// <exception cref="InvalidOperationException">未设置执行的命令，无法继续。</exception>
        /// <exception cref="ArgumentNullException">必须指明 Table, 以确认正在操作的表。</exception>
        public override int ExecuteCount(string tableName) {
            Thrower.ThrowArgumentNullExceptionIf(tableName, "tableName", "表为空");
            Sql = "SELECT COUNT(*) FROM " + tableName;
            if (SqlCondition != null) CommandText += " WHERE " + SqlCondition;
            object s = ExecuteScalar();
            return s == null ? -1 : (int)(long)s;

        }

		#region 静态辅助

		/// <summary>
		/// 执行数据库语句返回受影响的行数，失败或异常返回-1。
		/// </summary>
		/// <param name="sql">SQL语句。</param>
		/// <param name="parameter">数据库参数。</param>
		/// <returns>受影响的行数。</returns>
		/// <exception cref="ArgumentException">连接为空。</exception>
        public static int ExecuteNonQuery(string sql, params MySqlParameter[] parameter) {
			return ExecuteNonQuery(sql, CommandType.Text, parameter);
		}

		/// <summary>
		/// 执行数据库语句返回受影响的行数，失败或异常返回-1。
		/// </summary>
		/// <param name="commandText">SQL语句。</param>
		/// <param name="commandType">解释命令的字符串。</param>
		/// <param name="parameter">数据库参数。</param>
		/// <returns>受影响的行数。</returns>
		/// <exception cref="ArgumentException">连接为空。</exception>
        public static int ExecuteNonQuery(string commandText, CommandType commandType, params MySqlParameter[] parameter) {
			int result = 0;
            MySqlConnection connection = new MySqlConnection(DefaultConnectionString);
            using (MySqlCommand command = connection.CreateCommand()) {
				bool mustCloseConnection = PrepareCommand(connection, command, commandType, commandText, parameter);
				try {
					result = command.ExecuteNonQuery();
				} finally {
					command.Parameters.Clear();
					if(mustCloseConnection)
						connection.Close();
				}
			}

			return result;
			
		}

		/// <summary>
		/// 执行数据库语句返回第一行第一列，失败或异常返回null 。
		/// </summary>
		/// <param name="sql">SQL语句。</param>
		/// <param name="parameter">数据库参数。</param>
		/// <returns>object。</returns>
        public static object ExecuteScalar(string sql, params MySqlParameter[] parameter) {
			return ExecuteScalar(sql, CommandType.Text, parameter);
		}

		/// <summary>
		/// 执行数据库语句返回第一行第一列，失败或异常返回null 。
		/// </summary>
		/// <param name="commandText">SQL语句。</param>
		/// <param name="commandType">解释命令的字符串。</param>
		/// <param name="parameter">数据库参数。</param>
		/// <returns>object。</returns>
        public static object ExecuteScalar(string commandText, CommandType commandType, params MySqlParameter[] parameter) {
			object result = null;
            MySqlConnection connection = new MySqlConnection(DefaultConnectionString);
            using (MySqlCommand command = connection.CreateCommand()) {
				bool mustCloseConnection = PrepareCommand(connection, command, commandType, commandText, parameter);
				try {
					result = command.ExecuteScalar();
				} finally {
					command.Parameters.Clear();
					if(mustCloseConnection)
						connection.Close();
				}
			}
			return result;
			
		}

		/// <summary>
		/// 执行数据库语句返回第一个内存表。
		/// </summary>
		/// <param name="sql">SQL语句。</param>
		/// <param name="parameter">数据库参数。</param>
		/// <returns>表。</returns>
        public static DataTable ExecuteTable(string sql, params MySqlParameter[] parameter) {
			return ExecuteTable(sql, CommandType.Text, parameter);
		}

		/// <summary>
		/// 执行数据库语句返回第一个内存表。
		/// </summary>
		/// <param name="commandText">SQL语句。</param>
		/// <param name="commandType">解释命令的字符串。</param>
		/// <param name="parameter">数据库参数。</param>
		/// <returns>表。</returns>
        public static DataTable ExecuteTable(string commandText, CommandType commandType, params MySqlParameter[] parameter) {
			DataTable dataTable = new DataTable();
            using (MySqlDataReader dr = ExecuteReader(commandText, commandType, parameter)) {
				dataTable.Load(dr);
			}
			return dataTable;
		}

		/// <summary>
		/// 执行数据库语句返回一个自进结果集流。
		/// </summary>
		/// <param name="sql">SQL语句。</param>
		/// <param name="parameter">数据库参数。</param>
		/// <returns>读取。</returns>
        public static MySqlDataReader ExecuteReader(string sql, params MySqlParameter[] parameter) {
			return ExecuteReader(sql, CommandType.Text, parameter);
		}

		/// <summary>
		/// 执行数据库语句返回一个自进结果集流。
		/// </summary>
		/// <param name="commandText">SQL语句。</param>
		/// <param name="commandType">解释命令的字符串。</param>
		/// <param name="parameter">数据库参数。</param>
		/// <returns>读取。</returns>
        public static MySqlDataReader ExecuteReader(string commandText, CommandType commandType, params MySqlParameter[] parameter) {
            using (MySqlConnection connection = new MySqlConnection(DefaultConnectionString)) {
                using (MySqlCommand command = connection.CreateCommand()) {
					PrepareCommand(connection, command, commandType, commandText, parameter);

					return command.ExecuteReader(CommandBehavior.CloseConnection);
				}
			}
		}

		#endregion

        #region CommandBuilder

        /// <summary>
        /// 生成一个命令生成工具。
        /// </summary>
        /// <returns></returns>
        protected override DbHelper.CommandTextBuilder GetCommandTextBuilder() {
            return new CommandTextBuilder();
        }

        /// <summary>
        /// Sql 生成类。
        /// </summary>
        protected new class CommandTextBuilder :DbHelper.CommandTextBuilder {

            /// <summary>
            /// 获取生成的 Sql。
            /// </summary>
            /// <param name="operation">生成的操作。</param>
            /// <returns>Sql</returns>
            public override string ToString(SqlOperation operation) {

                 //  MYSQL 允许直接分页
                if (operation == SqlOperation.Select && PageSize >= 0) {

                    return String.Format("SELECT {0} FROM {1}{4}{5} LIMIT {2},{3}", Column, Table, CurrentPage <= 0 ? 0 : CurrentPage * PageSize, PageSize, String.IsNullOrEmpty(Condition) ? String.Empty : " WHERE " + Condition, Orderby == null ? String.Empty : " ORDER BY " + Orderby);

                }

                return base.ToString(operation);
            }

            /// <summary>
            /// 格式化字符串格式。
            /// </summary>
            /// <param name="column">列。</param>
            /// <param name="format">参数源。</param>
            /// <returns>处理后的字符串。</returns>
            protected override string Format(string column, string format) {
                System.Text.StringBuilder c = new System.Text.StringBuilder();
                foreach (string s in column.Split(',')) {
                    c.AppendFormat(format, s, Py.Core.Str.Unsurround(s, '`', '`'));
                }
                return c.ToString(1, c.Length - 1);
            }
        }

        #endregion

		#region AdapterHelper

        /// <summary>
        /// 生成适合当前的数据库适配器辅助类。
        /// </summary>
        /// <returns>数据库适配器的辅助实例。</returns>
		public override DbHelper.AdapterHelper CreateAdapter() {
			return new AdapterHelper(this);
		}

		/// <summary> 
		/// 数据库适配器辅助类。
		/// </summary>
		public new class AdapterHelper : DbHelper.AdapterHelper, IDisposable {

			#region 私有变量


			/// <summary>
			/// 数据适配。
			/// </summary>
			MySqlDataAdapter _dataAdapter;

			/// <summary>
			/// 辅助类。
			/// </summary>
			MySqlHelper _dbHelper;

			/// <summary>
			/// 命令生成
			/// </summary>
			MySqlCommandBuilder _commandBuilder;

			#endregion

			#region 公共属性

			/// <summary>
			/// 获取当前正在使用的 数据适配器。
			/// </summary>
			public override DbDataAdapter DataAdapter {
				get { return _dataAdapter; }
			}

            /// <summary>
            /// 获取当前正在使用的 数据库辅助类。
            /// </summary>
			public override DbHelper DbHelper {
				get { return _dbHelper; }
			}

            /// <summary>
            /// 被子类重写时，实现生成一个命令。
            /// </summary>
            /// <param name="cmdText">命令文本。</param>
            /// <returns>一个命令实例。</returns>
			protected override DbCommand CreateCommand(string cmdText) {
                return new MySqlCommand(cmdText, _dbHelper._connection);
			}

            /// <summary>
            /// 获取命令生成类。
            /// </summary>
            public override DbCommandBuilder CommandBuilder {
				get { return _commandBuilder; }
			}

			#endregion

			#region 方法

			/// <summary> 
			/// 使用Play.RunTime.Sql.SqlData 初始化 Play.RunTime.Sql.SqlData.RecordSet 类的新实例。
			/// </summary> 
			/// <param name="dbHelper">dbHelper对象</param>
			public AdapterHelper(MySqlHelper dbHelper)
				: base() {
				_dbHelper = dbHelper;
				Initialize();
			}

			/// <summary>
			/// 初始化当前集合。
			/// </summary>
			public override void Initialize() {
				_dataAdapter = new MySqlDataAdapter(_dbHelper._command);
				_dataAdapter.SelectCommand.CommandText = DbHelper.CommandText;
				_commandBuilder = new MySqlCommandBuilder(_dataAdapter);
                _commandBuilder.QuotePrefix = "`";
                _commandBuilder.QuoteSuffix = "`";
				base.Initialize();
			}


			#endregion
		}

		#endregion

    }

}