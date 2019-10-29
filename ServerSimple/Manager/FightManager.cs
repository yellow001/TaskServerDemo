using NetFrame.AbsClass;
using NetFrame.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using NetFrame.Base;
using System.Collections.Concurrent;
using ServerSimple.Model;
using System.Threading;
using System.Linq;
using ServerSimple.Public;
using NetFrame.Tool;

namespace ServerSimple.Manager
{
    public class FightManager : BaseRoomManager<FightManager,FightRoom>{
        #region 旧的
        //static FightManager ins;

        //public ConcurrentDictionary<int, FightRoom> roomCache = new ConcurrentDictionary<int, FightRoom>();
        //public ConcurrentDictionary<BaseToken, int> tokenToRoomID = new ConcurrentDictionary<BaseToken, int>();

        //public ConcurrentStack<FightRoom> roomStack = new ConcurrentStack<FightRoom>();
        //private int index = 0;

        //public static FightManager Ins {
        //    get {
        //        if (ins == null) {
        //            ins = new FightManager();
        //        }
        //        return ins;
        //    }
        //}
        #endregion


        public FightManager() {
            Init();
        }

        protected override void Init() {

            StaticFunc.CreateFightRoom = CreateFightRoom;


            //1003002	FightRoomDTO	战斗房间房间初始化完毕反馈	此时客户端才开始更新战斗数据和切换场景
            MessageHandler.Ins.Register(1003003, PlayerInitCompleted);//进入战斗场景并初始化完毕,转播至对应房间  广播是 1003004(在对应房间广播)


            MessageHandler.Ins.Register(1003005, MoveCREQ);//移动相关请求,转播至对应房间  广播是 1003006(在对应房间广播)

            MessageHandler.Ins.Register(1003007, ShootCREQ);//射击相关请求  广播是 1003008(在对应房间广播)

            MessageHandler.Ins.Register(1003009, BulletDamageCREQ);//普通伤害请求(子弹伤害)  广播是 1003010(在对应房间广播)

            MessageHandler.Ins.Register(1003201, StatusCREQ);//客户端定期发送状态信息  广播是 1003202(在对应房间广播)

            //击杀广播  
        }

        private void StatusCREQ(BaseToken token, TransModel model) {
            //转发至对应的房间
            if (TokenHasRoom(token)) {
                roomCache[tokenToRoomID[token]].StatusCREQ(token, model);
            }
            else {
                Debugger.Trace("房间不存在");
            }
        }

        private void BulletDamageCREQ(BaseToken token, TransModel model) {
            //转发至对应的房间
            if (TokenHasRoom(token)) {
                roomCache[tokenToRoomID[token]].BulletDamageCREQ(token, model);
            }
            else {
                Debugger.Trace("房间不存在");
            }
        }

        private void ShootCREQ(BaseToken token, TransModel model) {
            //转发至对应的房间
            if (TokenHasRoom(token)) {
                roomCache[tokenToRoomID[token]].ShootCREQ(token, model);
            }
            else {
                Debugger.Trace("房间不存在");
            }
        }

        private void MoveCREQ(BaseToken token, TransModel model) {
            //转发至对应的房间
            if (TokenHasRoom(token)) {
                roomCache[tokenToRoomID[token]].MoveCREQ(token, model);
            }
            else {
                Debugger.Trace("房间不存在");
            }
        }

        void PlayerInitCompleted(BaseToken token,TransModel model) {
            //转发至对应的房间
            if (TokenHasRoom(token)) {
                roomCache[tokenToRoomID[token]].OnInitCompleted(token, model);
            }
            else {
                Debugger.Trace("房间不存在");
            }
        }



        public void CreateFightRoom(Dictionary<string, BaseToken> tokenDic) {
            //获取或创建战斗房间，添加对应的缓存
            FightRoom room = GetEmptyRoom();
            roomCache.TryAdd(room.id, room);
            BaseToken[] tokens = tokenDic.Values.ToArray();
            foreach (var item in tokens) {
                tokenToRoomID.TryAdd(item, room.id);
            }
            //调用战斗房间的初始化方法
            room.Init(tokens);
        }


        public override void OnClientClose(BaseToken token, string error) {
            if (TokenHasRoom(token)) {
                roomCache[tokenToRoomID[token]].OnClientClose(token,error);
            }
        }

        public override void OnClientConnected(BaseToken token) {
        }

        ///// <summary>
        ///// 从栈中获取或新建一个房间
        ///// </summary>
        ///// <returns></returns>
        //FightRoom GetEmptyRoom() {
        //    FightRoom room;
        //    if (roomStack.Count == 0) {
        //        Interlocked.Increment(ref index);
        //        room = new FightRoom(index);
        //        room.manager = this;
        //        return room;
        //    }

        //    if (roomStack.TryPop(out room)) {
        //        return room;
        //    }
        //    return null;
        //}

        protected override void InitRoom(BaseRoom r) {
            if (r is FightRoom) {
                (r as FightRoom).manager = this;
            }
        }

        //FightRoom[] GetRoomsDTO() {
        //    List<FightRoom> dtoList = new List<FightRoom>();
        //    foreach (var item in roomCache.Values) {
        //        dtoList.Add(item.GetDTO());
        //    }
        //    return dtoList.ToArray();
        //}
    }
}
