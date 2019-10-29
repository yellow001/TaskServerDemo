using DAL;
using NetFrame.Base;
using ServerSimple.Base;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ServerSimple.Cache {
    public class UserCache:SingleClass<UserCache>
    {
        public ConcurrentDictionary<string, UserDAL> nameToUserDic;

        ConcurrentDictionary<string, BaseToken> nameToTokenDic;

        ConcurrentDictionary<BaseToken, string> tokenToNameDic;

        //Mutex mutex;

        public UserCache() {
            nameToUserDic = new ConcurrentDictionary<string, UserDAL>();
            nameToTokenDic = new ConcurrentDictionary<string, BaseToken>();
            tokenToNameDic = new ConcurrentDictionary<BaseToken, string>();
            //mutex = new Mutex();
        }

        /// <summary>
        /// 是否存在该用户
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool HasUser(string n) {
            if (nameToUserDic.ContainsKey(n)) {
                return true;
            }
            UserDAL dal = new UserDAL();
            dal.GetModel(n);
            if (!string.IsNullOrEmpty(dal.name)) {
                nameToUserDic.TryAdd(n, dal);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 增加用户
        /// </summary>
        /// <param name="n"></param>
        /// <param name="pwd"></param>
        public void AddUser(string n, string pwd) {
            if (HasUser(n)) { return; }

            UserDAL dal = new UserDAL();
            dal.name = n;
            dal.passwd = pwd;
            dal.Add();

            nameToUserDic.TryAdd(n, dal);
        }

        /// <summary>
        /// 移除用户
        /// </summary>
        /// <param name="n"></param>
        public void RemoveUser(string n) {
            if (!HasUser(n)) { return; }
            UserDAL.Delete(n);
            UserDAL dal = new UserDAL();
            nameToUserDic.Remove(n,out dal);
        }

        /// <summary>
        /// 用户是否已上线
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool IsOnline(string n) {
            //lock (onlineUserName) {
            //    if (onlineUserName.Contains(n)) {
            //        return true;
            //    }
            //    return false;
            //}
            if (nameToTokenDic.ContainsKey(n)) {
                return true;
            }
            return false;
        }

        public bool IsOnline(BaseToken t) {
            if (tokenToNameDic.ContainsKey(t)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 用户下线
        /// </summary>
        /// <param name="n"></param>
        public void OffLine(string n) {
            string temp = n;
            BaseToken token;
            if (nameToTokenDic.ContainsKey(temp)) {
                tokenToNameDic.TryRemove(nameToTokenDic[n], out temp);
                nameToTokenDic.TryRemove(temp, out token);
            }
        }

        public void OffLine(BaseToken t) {
            string temp;
            BaseToken token=t;
            if (tokenToNameDic.ContainsKey(t)) {
                nameToTokenDic.TryRemove(tokenToNameDic[t], out token);
                tokenToNameDic.TryRemove(token, out temp);
            }
        }

        /// <summary>
        /// 用户上线
        /// </summary>
        /// <param name="n"></param>
        public bool Online(string n,BaseToken t) {
            if (nameToTokenDic.ContainsKey(n) || tokenToNameDic.ContainsKey(t)) {
                return false;
            }

            nameToTokenDic.TryAdd(n, t);
            tokenToNameDic.TryAdd(t, n);
            return true;
        }

        public UserDAL GetDALByName(string n) {
            if (HasUser(n)) {
                return nameToUserDic[n];
            }
            return null;
        }

        public UserDAL GetDALByToken(BaseToken token) {
            if (tokenToNameDic.ContainsKey(token) && nameToTokenDic.ContainsKey(tokenToNameDic[token])) {
                return nameToUserDic[tokenToNameDic[token]];
            }
            return null;
        }

        public bool SaveUserDAL(BaseToken token,UserDAL d) {
            UserDAL odal = GetDALByToken(token);
            if (odal != null) {
                odal.headID = odal.headID.Equals(d.headID) ? odal.headID : d.headID;
                odal.hairData = odal.hairData.Equals(d.hairData) ? odal.hairData : d.hairData;
                odal.clothData = odal.clothData.Equals(d.clothData) ? odal.clothData : d.clothData;
                odal.Update();
                return true;
            }
            else {
                return false;
            }
        }
    }
}
