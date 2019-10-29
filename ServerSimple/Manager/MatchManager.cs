using NetFrame.AbsClass;
using NetFrame.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using NetFrame.Base;
using System.Collections.Concurrent;
using ServerSimple.Model;
using ServerSimple.Cache;
using System.Threading;
using ServerSimple.DTO.Match;
using ServerSimple.Public;
using ServerSimple.DTO.Login;
using DAL;

namespace ServerSimple.Manager
{
    public class MatchManager : BaseRoomManager<MatchManager,MatchRoom>{

        #region 旧的
        //static MatchManager ins;

        //public static MatchManager Ins {
        //    get {
        //        if (ins == null) {
        //            ins = new MatchManager();
        //        }
        //        return ins;
        //    }
        //}

        ///// <summary>
        ///// 匹配房间缓存
        ///// </summary>
        //public ConcurrentDictionary<int, MatchRoom> roomCache = new ConcurrentDictionary<int, MatchRoom>();

        ///// <summary>
        ///// 房间池（线程安全）
        ///// </summary>
        //public ConcurrentStack<MatchRoom> roomStack = new ConcurrentStack<MatchRoom>();

        ///// <summary>
        ///// 连接与匹配房间id的对应字典
        ///// </summary>
        //public ConcurrentDictionary<BaseToken, int> tokenToRoomID = new ConcurrentDictionary<BaseToken, int>();

        ///// <summary>
        ///// 自增房间id
        ///// </summary>
        //int index = 0;
        #endregion


        public MatchManager() {
            Init();
        }

        protected override void Init() {
            MessageHandler.Ins.Register(1002001, RefreshRoomCREQ);//刷新房间请求

            MessageHandler.Ins.Register(1002003, CreateRoomCREQ);//创建房间请求

            MessageHandler.Ins.Register(1002005, EnterRoomCREQ);//进入房间请求

            MessageHandler.Ins.Register(1002007, ExitRoomCREQ);//进入房间请求

            MessageHandler.Ins.Register(1002009, RemoveCREQ);//踢人请求

            MessageHandler.Ins.Register(1002010, StartCREQ);//开始游戏请求

            MessageHandler.Ins.Register(1002801, ChangeUserDataCREQ);//更换装扮请求
        }

        private void StartCREQ(BaseToken token, TransModel model) {
            int result = OnStart(token, model);
            if (result != 1) {
                Send(token, 1002011, result);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        /// <returns>
        /// 1 可以开始游戏
        /// -1 连接未登录
        /// -2 不是有效连接
        /// -3 连接对应房间不存在
        /// -4 不是房主，不能开始游戏
        /// -5 只有一个人，不可以开始游戏
        /// </returns>
        int OnStart(BaseToken token, TransModel model) {
            if (!UserCache.Ins.IsOnline(token)) {
                return -1;
            }

            //if (!tokenToRoomID.ContainsKey(token)) {
            //    return -2;
            //}

            //long id = tokenToRoomID[token];
            //if (!roomCache.ContainsKey(id)) {
            //    tokenToRoomID.Remove(token, out id);
            //    return -3;
            //}
            if (!IsVaildToken(token)) {
                return -2;
            }

            long id = tokenToRoomID[token];

            string tName = UserCache.Ins.GetDALByToken(token).name;
            if (!roomCache[id].masterName.Equals(tName)) {
                return -4;
            }

            //if (roomCache[id].allTokens.Count <= 1) {
            //    return -5;
            //}

            roomCache[id].StartGame();

            return 1;
        }

        private void RemoveCREQ(BaseToken token, TransModel model) {
            OnRemove(token, model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        /// <returns>
        /// 1 可以移除
        /// -1 连接未登录
        /// -2 不是有效连接   //连接不在房间内
        /// -3 连接对应房间不存在
        /// -4 不是房主，不能踢人
        /// -5 dto出错或踢的人是自己
        /// -6 踢的人不存在
        /// </returns>
        int OnRemove(BaseToken token, TransModel model) {
            if (!UserCache.Ins.IsOnline(token)) {
                return -1;
            }

            //if (!tokenToRoomID.ContainsKey(token)) {
            //    return -2;
            //}

            //long id= tokenToRoomID[token]; 
            //if (!roomCache.ContainsKey(id)) {
            //    tokenToRoomID.Remove(token, out id);
            //    return -3;
            //}

            if (!IsVaildToken(token)) {
                return -2;
            }

            long id = tokenToRoomID[token];

            string tName= UserCache.Ins.GetDALByToken(token).name;
            if (!roomCache[id].masterName.Equals(tName)){
                return -4;
            }

            if (string.IsNullOrEmpty(model.GetMsg<string>()) || model.GetMsg<string>().Equals(tName)) {
                return -5;
            }

            if (!roomCache[id].allTokens.ContainsKey(model.GetMsg<string>())) {
                return -6;
            }

            roomCache[id].Remove(model.GetMsg<string>());

            return 1;
        }

        /// <summary>
        /// 创建房间请求
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        private void CreateRoomCREQ(BaseToken token, TransModel model) {
            int result = OnCreateRoom(token, model);
            if (result != 1) {
                Send(token, 1002004, result);
            }
            else {
                Send(token, 1002004, result, roomCache[tokenToRoomID[token]].GetDTO());
            }
        }

        /// <summary>
        /// 创建房间处理
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        /// <returns>
        /// 1 创建成功
        /// -1 连接已在房间中
        /// -2 连接未登录
        /// -3 获取房间出错
        /// </returns>
        int OnCreateRoom(BaseToken token, TransModel model) {
            if (TokenHasRoom(token)) {
                return -1;
            }

            if (!UserCache.Ins.IsOnline(token)) {
                return -2;
            }

            MatchRoom room = GetEmptyRoom();
            if (room == null) {
                return -3;
            }
            
            room.masterName = UserCache.Ins.GetDALByToken(token).name;
            room.allTokens.Add(room.masterName, token);

            if (!string.IsNullOrEmpty(model.GetMsg<string>())) {
                room.passwd = model.GetMsg<string>();
            }

            roomCache.TryAdd(room.id, room);
            tokenToRoomID.TryAdd(token, room.id);

            return 1;
        }

        

        /// <summary>
        /// 刷新房间请求
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        void RefreshRoomCREQ(BaseToken token, TransModel model) {

            int result = OnRefreshRoom(token, model);

            if (result == 1) {
                Send(token, 1002002, 1, GetRoomsDTO());
            }
            else {
                Send(token, 1002002, result);
            }

            
        }

        /// <summary>
        /// 处理刷新房间请求
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        /// <returns>
        /// 1 可以刷新
        /// -1 连接在某房间中，不用刷新
        /// -2 连接未登录
        /// </returns>
        int OnRefreshRoom(BaseToken token, TransModel model) {
            if (TokenHasRoom(token)) {
                return -1;
            }

            if (!UserCache.Ins.IsOnline(token)) {
                return -2;
            }
            return 1;
        }

        /// <summary>
        /// 进入房间请求
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        void EnterRoomCREQ(BaseToken token, TransModel model) {
            int result = OnEnterRoom(token, model);
            if (result != 1) {
                Send(token, 1002006, result);
            }
        }

        /// <summary>
        /// 处理进入房间请求
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        /// <returns>
        /// 1 进入房间成功
        /// -1 房间不存在或已过期
        /// -2 连接还在某房间中
        /// -3 连接未登录
        /// -4 密码错误
        /// -5 人数已满
        /// -6 进入房间出错
        /// </returns>
        int OnEnterRoom(BaseToken token, TransModel model) {
            int roomID = model.area;
            if (!ContainsRoomByID(roomID)) {
                return -1;
            }
            if (tokenToRoomID.ContainsKey(token)) {
                return -2;
            }

            if (!UserCache.Ins.IsOnline(token)) {
                return -3;
            }

            return roomCache[roomID].EnterCREQ(token, model);
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        void ExitRoomCREQ(BaseToken token,TransModel model) {
            OnExitRoom(token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        /// <returns>
        /// 1  离开成功
        /// -1 不在房间中
        /// -2 连接未登录
        /// -3 连接不在该房间中
        /// </returns>
        int OnExitRoom(BaseToken token) {
            if (!TokenHasRoom(token)) {
                return -1;
            }
            if (!UserCache.Ins.IsOnline(token)) {
                return -2;
            }

            long roomID = tokenToRoomID[token];
            return roomCache[roomID].ExitCREQ(token);
        }

        public void ChangeUserDataCREQ(BaseToken token,TransModel model) {
            if (!string.IsNullOrEmpty(model.GetMsg<UserDTO>().name)) {
                UserDTO d = model.GetMsg<UserDTO>();
                UserDAL dal = new UserDAL();
                dal.clothData = d.clothData;
                dal.hairData = d.hairData;
                dal.headID = d.headID;
                if (UserCache.Ins.SaveUserDAL(token, dal)) {
                    Send(token, 1002802, 1);//更换成功
                }
                else {
                    Send(token, 1002802, 0);//更换失败
                }
            }
        }


        public override void OnClientClose(BaseToken token, string error) {
            if (tokenToRoomID.ContainsKey(token)) {
                ExitRoomCREQ(token, null);
            }
        }

        public override void OnClientConnected(BaseToken token) {
        }
        

        ///// <summary>
        ///// 从栈中获取或新建一个房间
        ///// </summary>
        ///// <returns></returns>
        //MatchRoom GetEmptyRoom() {
        //    MatchRoom room;
        //    if (roomStack.Count == 0) {
        //        Interlocked.Increment(ref index);
        //        room = new MatchRoom(index);
        //        room.manager = this;
        //        return room;
        //    }

        //    if (roomStack.TryPop(out room)) {
        //        return room;
        //    }
        //    return null;
        //}

        MatchRoomDTO[] GetRoomsDTO() {
            List<MatchRoomDTO> dtoList = new List<MatchRoomDTO>();
            foreach (var item in roomCache.Values) {
                dtoList.Add(item.GetDTO());
            }
            return dtoList.ToArray();
        }

        protected override void InitRoom(BaseRoom r) {
            if (r is MatchRoom) {
                (r as MatchRoom).manager = this;
            }
        }
    }
}
