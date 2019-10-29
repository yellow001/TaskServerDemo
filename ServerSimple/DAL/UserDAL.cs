using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using NetFrame.Tool;

namespace DAL {
    public class UserDAL {
        /// <summary>
        /// name
        /// </summary>		
        private string _name;
        public string name {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// passwd
        /// </summary>		
        private string _passwd;
        public string passwd {
            get { return _passwd; }
            set { _passwd = value; }
        }
        /// <summary>
        /// headID
        /// </summary>		
        private string _headid;
        public string headID {
            get { return _headid; }
            set { _headid = value; }
        }
        /// <summary>
        /// hairData
        /// </summary>		
        private string _hairdata;
        public string hairData {
            get { return _hairdata; }
            set { _hairdata = value; }
        }
        /// <summary>
        /// clothData
        /// </summary>		
        private string _clothdata;
        public string clothData {
            get { return _clothdata; }
            set { _clothdata = value; }
        }
        /// <summary>
        /// winCount
        /// </summary>		
        private int _wincount;
        public int winCount {
            get { return _wincount; }
            set { _wincount = value; }
        }


        public bool Exists(string name) {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select count(1) from User");
            strSql.Append(" where ");
            strSql.Append(" name = @name  ");
            MySqlParameter[] parameters = {
                    new MySqlParameter("@name", MySqlDbType.VarChar,255)            };
            parameters[0].Value = name;

            return DbHelperMySQL.Exists(strSql.ToString(), parameters);
        }



        /// <summary>
        /// 增加一条数据
        /// </summary>
        public void Add() {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into User(");
            strSql.Append("name,passwd,headID,hairData,clothData,winCount");
            strSql.Append(") values (");
            strSql.Append("@name,@passwd,@headID,@hairData,@clothData,@winCount");
            strSql.Append(") ");

            MySqlParameter[] parameters = {
                        new MySqlParameter("@name", MySqlDbType.VarChar,255) ,
                        new MySqlParameter("@passwd", MySqlDbType.VarChar,255) ,
                        new MySqlParameter("@headID", MySqlDbType.VarChar,128) ,
                        new MySqlParameter("@hairData", MySqlDbType.VarChar,128) ,
                        new MySqlParameter("@clothData", MySqlDbType.VarChar,128) ,
                        new MySqlParameter("@winCount", MySqlDbType.Int32,11)

            };

            parameters[0].Value = this.name;
            parameters[1].Value = this.passwd;
            parameters[2].Value = this.headID;
            parameters[3].Value = this.hairData;
            parameters[4].Value = this.clothData;
            parameters[5].Value = this.winCount;
            DbHelperMySQL.ExecuteSqlAsync(null, strSql.ToString(), parameters);

        }


        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update() {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update User set ");

            strSql.Append(" name = @name , ");
            strSql.Append(" passwd = @passwd , ");
            strSql.Append(" headID = @headID , ");
            strSql.Append(" hairData = @hairData , ");
            strSql.Append(" clothData = @clothData , ");
            strSql.Append(" winCount = @winCount  ");
            strSql.Append(" where name=@name  ");

            MySqlParameter[] parameters = {
                        new MySqlParameter("@name", MySqlDbType.VarChar,255) ,
                        new MySqlParameter("@passwd", MySqlDbType.VarChar,255) ,
                        new MySqlParameter("@headID", MySqlDbType.VarChar,128) ,
                        new MySqlParameter("@hairData", MySqlDbType.VarChar,128) ,
                        new MySqlParameter("@clothData", MySqlDbType.VarChar,128) ,
                        new MySqlParameter("@winCount", MySqlDbType.Int32,11)

            };

            parameters[0].Value = this.name;
            parameters[1].Value = this.passwd;
            parameters[2].Value = this.headID;
            parameters[3].Value = this.hairData;
            parameters[4].Value = this.clothData;
            parameters[5].Value = this.winCount;
            int rows = DbHelperMySQL.ExecuteSql(strSql.ToString(), parameters);
            if (rows > 0) {
                return true;
            }
            else {
                return false;
            }
        }


        /// <summary>
        /// 删除一条数据
        /// </summary>
        public static void Delete(string name, Action<bool> callback = null) {

            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from User ");
            strSql.Append(" where name=@name ");
            MySqlParameter[] parameters = {
                    new MySqlParameter("@name", MySqlDbType.VarChar,255)            };
            parameters[0].Value = name;


            DbHelperMySQL.ExecuteSqlAsync((rows) => {
                callback?.Invoke(rows > 0);
            }, strSql.ToString(), parameters);
        }



        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public void GetModel(string name) {

            StringBuilder strSql = new StringBuilder();
            strSql.Append("select name, passwd, headID, hairData, clothData, winCount  ");
            strSql.Append("  from User ");
            strSql.Append(" where name=@name ");
            MySqlParameter[] parameters = {
                    new MySqlParameter("@name", MySqlDbType.VarChar,255)            };
            parameters[0].Value = name;


            UserDAL model = new UserDAL();
            DataResult dr = DbHelperMySQL.Query(strSql.ToString(), parameters);

            if (dr.list == null || dr.list.Count <= 0) { return; }

            if (dr.list[0].Length > 0) {
                this.name = dr.list[0][0].ToString();

                this.passwd = dr.list[0][1].ToString();

                this.headID = dr.list[0][2].ToString();

                this.hairData = dr.list[0][3].ToString();

                this.clothData = dr.list[0][4].ToString();

                if (dr.list[0][5].ToString() != "") {
                    this.winCount = int.Parse(dr.list[0][5].ToString());
                }

            }
        }


        /// <summary>
        /// 获得数据列表
        /// </summary>
        public DataResult GetList(string strWhere) {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select * ");
            strSql.Append(" FROM User ");
            if (strWhere.Trim() != "") {
                strSql.Append(" where " + strWhere);
            }
            return DbHelperMySQL.Query(strSql.ToString());
        }

        /// <summary>
        /// 获得前几行数据
        /// </summary>
        public DataResult GetList(int Top, string strWhere, string filedOrder) {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select ");
            if (Top > 0) {
                strSql.Append(" top " + Top.ToString());
            }
            strSql.Append(" * ");
            strSql.Append(" FROM User ");
            if (strWhere.Trim() != "") {
                strSql.Append(" where " + strWhere);
            }
            strSql.Append(" order by " + filedOrder);
            return DbHelperMySQL.Query(strSql.ToString());
        }


    }
}

