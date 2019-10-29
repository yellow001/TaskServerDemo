using NetFrame.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using NetFrame.Base;
using NetFrame.AbsClass;
using ServerSimple.DTO.Login;
using ServerSimple.Cache;
using DAL;

namespace ServerSimple.Manager {
    public class LoginManager : SingleSender, IMessageHandler {

        static LoginManager ins;

        UserCache cache;

        public static LoginManager Ins {
            get {
                if (ins == null) {
                    ins = new LoginManager();
                }
                return ins;
            }
        }

        private LoginManager() {
            cache = UserCache.Ins;
            Init();
        }

        public void Init() {
            MessageHandler.Ins.Register(1001001, RegisterCREQ);//注册请求 1001001   反馈 1001002
            MessageHandler.Ins.Register(1001003, LoginCREQ);//登录请求 1001003   反馈 1001004 
        }

        #region 注册
        public void RegisterCREQ(BaseToken token, TransModel model) {
            int result = OnUserRegister(token, model);
            Send(token, 1001002, result);  //注册反馈 1001002
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>
        /// 1  注册成功
        /// -1 dto错误
        /// -2 用户名以及密码出错
        /// -3 用户已存在
        /// </returns>
        public int OnUserRegister(BaseToken token, TransModel model) {

            if (model.GetMsg<UserDTO>() == null) {
                return -1;
            }

            UserDTO dto = model.GetMsg<UserDTO>();
            if (string.IsNullOrEmpty(dto.name) || string.IsNullOrEmpty(dto.password)) {
                return -2;
            }

            if (cache.HasUser(dto.name)) {
                return -3;
            }

            cache.AddUser(dto.name, dto.password);

            return 1;
        }
        #endregion

        #region 登录
        /// <summary>
        /// 登录请求
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        public void LoginCREQ(BaseToken token, TransModel model) {
            int result = OnLogin(token, model);
            if (result == 1) {
                Send(token, 1001004, result, GetDTO(cache.GetDALByToken(token).name));  //登录反馈 1001004
            }
            else {
                Send(token, 1001004, result);
            }
            
        }

        /// <summary>
        /// 登录处理
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        /// <returns>
        /// 1 登录成功
        /// -1 dto错误
        /// -2 dto信息错误
        /// -3 用户不存在
        /// -4 用户或连接已登录
        /// -5 缓存出错
        /// -6 密码错误
        /// </returns>
        public int OnLogin(BaseToken token, TransModel model) {
            if (model.GetMsg<UserDTO>() == null) {
                return -1;
            }

            UserDTO dto = model.GetMsg<UserDTO>();

            if (string.IsNullOrEmpty(dto.name) || string.IsNullOrEmpty(dto.password)) {
                return -2;
            }

            if (!cache.HasUser(dto.name)) {
                return -3;
            }

            if (!cache.GetDALByName(dto.name).passwd.Equals(dto.password)) {
                return -6;
            }

            if (cache.IsOnline(dto.name) || cache.IsOnline(token)) {
                return -4;
            }

            if (!cache.Online(dto.name, token)) {
                return -5;
            }

            return 1;
        }


        #endregion
        
        public void OnClientClose(BaseToken token, string error) {
            if (cache.IsOnline(token)) {
                cache.OffLine(token);
            }
        }

        public void OnClientConnected(BaseToken token) {

        }

        public UserDTO GetDTO(string n) {
            if (cache.HasUser(n)) {
                UserDAL dal = cache.nameToUserDic[n];
                string name = dal.name;
                string headID=dal.headID;
                string hairData=dal.hairData;
                string clothData=dal.clothData;
                int win = dal.winCount;
                UserDTO dto = new UserDTO(name, "", headID,hairData,clothData,win);
                return dto;
            }
            return null;
        }
    }
}
