﻿using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NetFrame.Tool
{
    public abstract class DbHelperMySQL
    {

        public static string connectionString = string.Empty;

        public DbHelperMySQL() { }

        #region 公用方法
        /// <summary>
        /// 得到最大值
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static int GetMaxID(string FieldName, string TableName) {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetSingle(strsql);
            if (obj == null) {
                return 1;
            }
            else {
                return int.Parse(obj.ToString());
            }
        }
        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public static bool Exists(string strSql) {
            object obj = GetSingle(strSql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value))) {
                cmdresult = 0;
            }
            else {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0) {
                return false;
            }
            else {
                return true;
            }
        }
        /// <summary>
        /// 是否存在（基于MySqlParameter）
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public static bool Exists(string strSql, params MySqlParameter[] cmdParms) {
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value))) {
                cmdresult = 0;
            }
            else {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0) {
                return false;
            }
            else {
                return true;
            }
        }
        #endregion

        #region  执行简单SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection)) {
                    try {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e) {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        public static int ExecuteSqlByTime(string SQLString, int Times) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection)) {
                    try {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e) {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public static int ExecuteSqlTran(List<String> SQLStringList) {
            using (MySqlConnection conn = new MySqlConnection(connectionString)) {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                MySqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try {
                    int count = 0;
                    for (int n = 0; n < SQLStringList.Count; n++) {
                        string strsql = SQLStringList[n];
                        if (strsql.Trim().Length > 1) {
                            cmd.CommandText = strsql;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return count;
                }
                catch {
                    tx.Rollback();
                    return 0;
                }
            }
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, string content) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                MySqlCommand cmd = new MySqlCommand(SQLString, connection);
                MySql.Data.MySqlClient.MySqlParameter myParameter = new MySql.Data.MySqlClient.MySqlParameter("@content", MySqlDbType.LongText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (MySql.Data.MySqlClient.MySqlException e) {
                    throw e;
                }
                finally {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static object ExecuteSqlGet(string SQLString, string content) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                MySqlCommand cmd = new MySqlCommand(SQLString, connection);
                MySql.Data.MySqlClient.MySqlParameter myParameter = new MySql.Data.MySqlClient.MySqlParameter("@content", MySqlDbType.LongText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try {
                    connection.Open();
                    object obj = cmd.ExecuteScalar();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value))) {
                        return null;
                    }
                    else {
                        return obj;
                    }
                }
                catch (MySql.Data.MySqlClient.MySqlException e) {
                    throw e;
                }
                finally {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                MySqlCommand cmd = new MySqlCommand(strSQL, connection);
                MySql.Data.MySqlClient.MySqlParameter myParameter = new MySqlParameter("@fs", MySqlDbType.LongBlob);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (MySql.Data.MySqlClient.MySqlException e) {
                    throw e;
                }
                finally {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection)) {
                    try {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value))) {
                            return null;
                        }
                        else {
                            return obj;
                        }
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e) {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }
        public static object GetSingle(string SQLString, int Times) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection)) {
                    try {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value))) {
                            return null;
                        }
                        else {
                            return obj;
                        }
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e) {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }
        /// <summary>
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public static MySqlDataReader ExecuteReader(string strSQL) {
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand(strSQL, connection);
            try {
                connection.Open();
                MySqlDataReader myReader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (MySql.Data.MySqlClient.MySqlException e) {
                throw e;
            }

        }
        /// <summary>
        /// 执行查询语句，返回DataResult
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataResult</returns>
        public static DataResult Query(string SQLString, int Times = 30) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                DataResult dr = new DataResult();
                try {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.CommandText = SQLString;
                    cmd.CommandTimeout = Times;
                    MySqlDataReader rd = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                    dr.Read(rd);
                }
                catch (MySqlException ex) {
                    throw new Exception(ex.Message);
                }
                return dr;
            }
        }

        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, params MySqlParameter[] cmdParms) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                using (MySqlCommand cmd = new MySqlCommand()) {
                    try {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (MySqlException e) {
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static async void ExecuteSqlAsync(Action<int> callback, string SQLString, params MySqlParameter[] cmdParms) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                using (MySqlCommand cmd = new MySqlCommand()) {
                    try {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = await cmd.ExecuteNonQueryAsync();
                        cmd.Parameters.Clear();
                        callback?.Invoke(rows);
                    }
                    catch (MySqlException e) {
                        throw e;
                    }
                }
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public static void ExecuteSqlTran(Hashtable SQLStringList) {
            using (MySqlConnection conn = new MySqlConnection(connectionString)) {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction()) {
                    MySqlCommand cmd = new MySqlCommand();
                    try {
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList) {
                            string cmdText = myDE.Key.ToString();
                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public static void ExecuteSqlTranWithIndentity(Hashtable SQLStringList) {
            using (MySqlConnection conn = new MySqlConnection(connectionString)) {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction()) {
                    MySqlCommand cmd = new MySqlCommand();
                    try {
                        int indentity = 0;
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList) {
                            string cmdText = myDE.Key.ToString();
                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
                            foreach (MySqlParameter q in cmdParms) {
                                if (q.Direction == System.Data.ParameterDirection.InputOutput) {
                                    q.Value = indentity;
                                }
                            }
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            foreach (MySqlParameter q in cmdParms) {
                                if (q.Direction == System.Data.ParameterDirection.Output) {
                                    indentity = Convert.ToInt32(q.Value);
                                }
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, params MySqlParameter[] cmdParms) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                using (MySqlCommand cmd = new MySqlCommand()) {
                    try {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value))) {
                            return null;
                        }
                        else {
                            return obj;
                        }
                    }
                    catch (MySqlException e) {
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public static MySqlDataReader ExecuteReader(string SQLString, params MySqlParameter[] cmdParms) {
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand();
            try {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                MySqlDataReader myReader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (MySqlException e) {
                throw e;
            }
            //			finally
            //			{
            //				cmd.Dispose();
            //				connection.Close();
            //			}	

        }

        /// <summary>
        /// 执行查询语句，返回DataResult
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataResult</returns>
        public static DataResult Query(string SQLString, params MySqlParameter[] cmdParms) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                MySqlCommand cmd = new MySqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                MySqlDataReader rd = cmd.ExecuteReader();

                DataResult dr = new DataResult(rd);


                return dr;
                //using (MySqlDataAdapter da = new MySqlDataAdapter(cmd)) {
                //    DataResult ds = new DataResult();
                //    try {
                //        da.Fill(ds, "ds");
                //        cmd.Parameters.Clear();
                //    }
                //    catch (MySql.Data.MySqlClient.MySqlException ex) {
                //        throw new Exception(ex.Message);
                //    }
                //    return ds;
                //}
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataResult
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataResult</returns>
        public static async void QueryAsync(Action<DataResult> callBack,string SQLString, params MySqlParameter[] cmdParms) {
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                MySqlCommand cmd = new MySqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                MySqlDataReader rd =(MySqlDataReader)(await cmd.ExecuteReaderAsync());

                DataResult dr = new DataResult(rd);


                callBack?.Invoke(dr);
                //using (MySqlDataAdapter da = new MySqlDataAdapter(cmd)) {
                //    DataResult ds = new DataResult();
                //    try {
                //        da.Fill(ds, "ds");
                //        cmd.Parameters.Clear();
                //    }
                //    catch (MySql.Data.MySqlClient.MySqlException ex) {
                //        throw new Exception(ex.Message);
                //    }
                //    return ds;
                //}
            }
        }


        public static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms) {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = System.Data.CommandType.Text;//cmdType;
            if (cmdParms != null) {


                foreach (MySqlParameter parameter in cmdParms) {
                    if ((parameter.Direction == System.Data.ParameterDirection.InputOutput || parameter.Direction == System.Data.ParameterDirection.Input) &&
                        (parameter.Value == null)) {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 数据返回结果列表
    /// </summary>
    public class DataResult {
        /// <summary>
        /// 数据行数
        /// </summary>
        public int rows = 0;
        /// <summary>
        /// 数据列数
        /// </summary>
        public int cols = 0;
        /// <summary>
        /// 数据内容
        /// </summary>
        public List<object[]> list = new List<object[]>();

        public DataResult() { }

        public DataResult(MySqlDataReader rd) {
            list = new List<object[]>();

            while (rd.Read()) {
                rows++;
                int count = rd.FieldCount;
                object[] re = new object[count];
                for (int i = 0; i < count; i++) {
                    re[i] = rd[i];
                }
                cols = re.Length;
                list.Add(re);
            }
            rd.Close();
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="rd"></param>
        public void Read(MySqlDataReader rd) {

            rows = 0;
            cols = 0;
            list = new List<object[]>();

            while (rd.Read()) {
                rows++;
                int count = rd.FieldCount;
                object[] re = new object[count];
                for (int i = 0; i < count; i++) {
                    re[i] = rd[i];
                }
                cols = re.Length;
                list.Add(re);
            }
            rd.Close();
        }


        public T getValue<T>(int row, int col) {
            if (list.Count <= row || list[row].Length <= col) {
                return default(T);
            }

            try {
                return (T)list[row][col];
            }
            catch (Exception ex) {

                return default(T);
            }

        }
    }
}
