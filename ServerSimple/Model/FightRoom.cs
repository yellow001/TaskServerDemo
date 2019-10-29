using System;
using System.Collections.Generic;
using System.Text;
using ServerSimple.Manager;
using NetFrame.Base;
using ServerSimple.Cache;
using ServerSimple.DTO.Fight;
using ServerSimple.Data;
using ServerSimple.Factory;
using NetFrame.Tool;
using ServerSimple.AI;
using System.Threading.Tasks;
using System.Threading;
using NetFrame.AbsClass;
using System.Linq;
using ServerSimple.DTO.Login;

namespace ServerSimple.Model
{
    public class FightRoom : BaseRoom {
        public FightManager manager;
        //public int id;

        public Dictionary<BaseToken, UserDTO> tokenToUserDTO = new Dictionary<BaseToken, UserDTO>();

        public Dictionary<int, BaseModel> baseModelDic = new Dictionary<int, BaseModel>();

        public Dictionary<BaseToken, int> tokenToModelID = new Dictionary<BaseToken, int>();

        public Dictionary<int, BaseAI> modelIdToAIDic = new Dictionary<int, BaseAI>();

        TimeEventModel updateModel;

        float deltaTime = 0.15f;

        public float allTime = 0;

        public Dictionary<BaseToken, UserDTO> completeTokens = new Dictionary<BaseToken, UserDTO>();

        float SyncStatusTime = 2f;
        float SyncStatusDeltaTime = 0;

        //发送伤害消息的ID列表
        Dictionary<int,float> DamageDic = new Dictionary<int, float>();

        /// <summary>
        /// 模型同步信号量
        /// </summary>
        Mutex ModelMutex = new Mutex();

        /// <summary>
        /// 伤害列表同步信号量
        /// </summary>
        Mutex DamageMutex = new Mutex();

        FightRoomState state = FightRoomState.Over;

        Random rand = new Random();
        public FightRoom() {
        }

        public void Init(BaseToken[] tokens) {
            state = FightRoomState.Init;
            //添加字典
            foreach (var item in tokens) {
                string name = UserCache.Ins.GetDALByToken(item).name;
                tokenToUserDTO.Add(item, LoginManager.Ins.GetDTO(name));
            }

            string[] pos_str = AppSetting.Ins.GetValue("pos").Split(';', StringSplitOptions.RemoveEmptyEntries);

            //初始化 模型数据（只有玩家）
            for (int i = 0; i < tokens.Length; i++) {
                BaseToken t = tokens[i];
                BaseModelData data = BaseModelDataFactory.GetDataByID(1);
                string[] p = pos_str[i].Split('_');
                Vector3Ex pos = new Vector3Ex(int.Parse(p[0]), int.Parse(p[1]), int.Parse(p[2]));
                BaseModel model = new BaseModel(i+1, 1, tokenToUserDTO[t].name, data, pos, Vector3Ex.Zero);//位置什么的都应该读表
                tokenToModelID.Add(t, model.id);
                baseModelDic.Add(model.id, model);
            }

            updateModel = new TimeEventModel(deltaTime, -1, Update);
            //TimeEventHandler.Ins.AddEvent(new TimeEventModel(300, 1, CreateDragon));
            TimeEventHandler.Ins.AddEvent(updateModel);

            state = FightRoomState.Ready;

            //发送房间数据
            Broadcast(tokenToUserDTO.Keys.ToList(), 1003002, 0, GetDTO());
        }

        /// <summary>
        /// deltaTime 执行一次的update函数
        /// </summary>
        public virtual void Update() {
            allTime += deltaTime;

            lock (modelIdToAIDic) {
                //执行AI逻辑
                if (modelIdToAIDic.Count > 0) {
                    Parallel.ForEach(modelIdToAIDic.Values, (item) => item.Update());
                }
            }

            //其他逻辑
            #region 状态同步
            //SyncStatusDeltaTime += deltaTime;
            //if (SyncStatusDeltaTime >= SyncStatusTime) {
            //    SyncStatusDeltaTime = 0;
            //    StatusSRES();
            //}
            #endregion

            #region 伤害列表冷却计算
            //if (DamageDic.Count > 0) {
            //    lock (DamageDic) {
            //        DamageMutex.WaitOne();

            //        Parallel.ForEach(DamageDic,(item)=> {
            //            DamageDic[item.Key] += deltaTime;

            //        })

            //        Parallel.For(0, DamageDic.Count, (index) => {
            //            DamageDic[index] += deltaTime;
            //            if (DamageDic[index] >= 0.25f) {
            //                DamageDic.Remove(index);
            //            }
            //        });
            //        DamageMutex.ReleaseMutex();
            //    }
            //}
            #endregion

        }

        /// <summary>
        /// 创建龙单位，并添加AI
        /// </summary>
        private void CreateDragon() {
            //判断房间是否还有效
            if (this == null || tokenToUserDTO == null || tokenToUserDTO.Count == 0) {
                return;
            }

            //加载龙的数据
        }

        public override void Clear() {
            //清除manager中的对应数据
            long id;
            FightRoom room;
            foreach (var item in tokenToModelID.Keys) {
                manager.tokenToRoomID.Remove(item, out id);
            }
            manager.roomCache.Remove(this.id, out room);

            //清除自身数据
            TimeEventHandler.Ins.RemoveEvent(updateModel);
            allTime = 0;

            tokenToUserDTO.Clear();
            baseModelDic.Clear();
            tokenToModelID.Clear();

            completeTokens.Clear();

            state = FightRoomState.Over;

            //加入到manager栈中
            manager.roomStack.Push(this);
        }

        #region 消息处理逻辑

        /// <summary>
        /// 客户端加载完毕 广播是 1003004(在对应房间广播)
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        internal void OnInitCompleted(BaseToken token, TransModel model) {
            //throw new NotImplementedException();
            if (completeTokens.ContainsKey(token)) {
                return;
            }
            else {
                completeTokens.Add(token,tokenToUserDTO[token]);
                int result = completeTokens.Count == tokenToUserDTO.Count ? 1 : 0;
                state = result == 1 ? FightRoomState.Fight : state;

                Dictionary<int, UserDTO> enterUserDTODic = new Dictionary<int, UserDTO>();

                foreach (var item in completeTokens) {
                    enterUserDTODic.Add(tokenToModelID[item.Key], item.Value);
                }

                Broadcast(completeTokens.Keys.ToList(), 1003004, result, enterUserDTODic);
            }
        }

        /// <summary>
        /// 移动请求
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        internal void MoveCREQ(BaseToken token, TransModel model) {

            if (state != FightRoomState.Fight) { return; }

            //throw new NotImplementedException();
            if (IsVaildUserModel(token)) {
                try {

                    if (model.area == 0) {
                        if (tokenToModelID[token] == model.GetMsg<MoveDataDTO>().modelID) {
                            Broadcast(tokenToModelID.Keys.ToList(), 1003006, 0, model.GetMsg<MoveDataDTO>());
                        }

                    }
                    else if (model.area == 3||model.area==4) {
                        if (tokenToModelID[token] == model.GetMsg<RotateYDTO>().modelID) {
                            Broadcast(tokenToModelID.Keys.ToList(), 1003006, model.area, model.GetMsg<RotateYDTO>());
                        }
                    }
                    else {
                        if (tokenToModelID[token] == model.GetMsg<MouseBtnDTO>().modelID) {
                            Broadcast(tokenToModelID.Keys.ToList(), 1003006, model.area, model.GetMsg<MouseBtnDTO>());
                        }
                    }

                    
                }
                catch (Exception ex) {
                    Debugger.Error(ex.ToString());
                    //throw;
                }
                
            }
        }

        /// <summary>
        /// 子弹伤害请求
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        internal void BulletDamageCREQ(BaseToken token, TransModel model) {

            if (state != FightRoomState.Fight) { return; }

            //throw new NotImplementedException();
            if (IsVaildUserModel(token)) {
                try {
                    DamageDTO damageDTO = model.GetMsg<DamageDTO>();
                    if (tokenToModelID[token] == damageDTO.SrcID) {
                        if (tokenToModelID.ContainsValue(damageDTO.DstID)) {
                            lock (DamageDic) {
                                DamageMutex.WaitOne();
                                if (!DamageDic.ContainsKey(damageDTO.SrcID)) {
                                    //DamageDic.Add(damageDTO.SrcID, 0);
                                    baseModelDic[damageDTO.DstID].hp -= 20;

                                    //伤害广播
                                    Broadcast(tokenToModelID.Keys.ToList(), 1003010, 0, damageDTO);

                                    if (baseModelDic[damageDTO.DstID].hp <= 0) {
                                        //击杀 广播  &&  失败反馈
                                        BaseToken dstToken = tokenToModelID.Where((t, id) => t.Value == damageDTO.DstID).ToList()[0].Key;

                                        Broadcast(tokenToModelID.Keys.ToList(), 1003011, 0, damageDTO);

                                        Send(dstToken, 10039998, damageDTO.DstID);

                                        // 清除失败 token 
                                        tokenToModelID.Remove(dstToken);
                                        tokenToUserDTO.Remove(dstToken);
                                        long rid;
                                        FightManager.Ins.tokenToRoomID.Remove(dstToken,out rid);

                                        if (tokenToModelID.Count == 1) {
                                            //成功广播 并销毁房间
                                            Broadcast(tokenToModelID.Keys.ToList(), 10039999, damageDTO.SrcID);
                                            Clear();
                                        }
                                    }
                                }
                                DamageMutex.ReleaseMutex();
                            }
                            
                        }
                    }
                }
                catch (Exception ex) {
                    Debugger.Error(ex.ToString());
                    //throw;
                }
            }
        }

        /// <summary>
        /// 射击相关请求（瞄准 开火） 广播是 1003008(在对应房间广播)
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        internal void ShootCREQ(BaseToken token, TransModel model) {

            if (state != FightRoomState.Fight) { return; }

            //throw new NotImplementedException();
            if (IsVaildUserModel(token)) {
                try {
                    if (tokenToModelID[token] == model.GetMsg<ShootDataDTO>().modelID) {
                        Broadcast(tokenToModelID.Keys.ToList(), 1003008, 0, model.GetMsg<ShootDataDTO>());
                    }
                }
                catch (Exception ex) {
                    Debugger.Error(ex.ToString());
                    //throw;
                }
            }
        }

        /// <summary>
        /// 客户端的状态信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        internal void StatusCREQ(BaseToken token, TransModel model) {
            //throw new NotImplementedException();
            if (IsVaildUserModel(token)) {
                //同步客户端模型状态（每个客户端5秒发一次过来）
                try {
                    BaseModel m = model.GetMsg<BaseModel>();
                    if (GetModelByToken(token) != null && GetModelByToken(token).id == m.id) {
                        Broadcast(tokenToModelID.Keys.ToList(), 1003202, 0, m);
                    }
                }
                catch (Exception ex) {
                    Debugger.Error(ex.ToString());
                    //throw;
                }
            }
        }

        ///// <summary>
        ///// 服务端每2秒发送一次模型的状态信息用于客户端修正
        ///// </summary>
        //void StatusSRES() {
        //    Broadcast(tokenToModelID.Keys.ToList(), 1003202, 0, GetDTO());
        //}
        #endregion



        public FightRoomDTO GetDTO() {
            return new FightRoomDTO(this);
        }

        public override void OnClientClose(BaseToken token, string error) {
            //throw new NotImplementedException();
            //不做掉线重连，直接清除对应数据 

            int modelID = tokenToModelID[token];

            // 清除掉线 token 
            tokenToModelID.Remove(token);
            tokenToUserDTO.Remove(token);
            long rid;
            FightManager.Ins.tokenToRoomID.Remove(token, out rid);

            //全部人都掉线了
            if (tokenToModelID.Count == 0) {
                Clear();
                return;
            }

            //广播掉线
            Broadcast(tokenToModelID.Keys.ToList(), 10039990, modelID);

        }


        public override void OnClientConnected(BaseToken token) {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// token对应的单位是否可操作（token是否存在&&对应model存在）
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        bool IsVaildUserModel(BaseToken token) {
            if (tokenToModelID.ContainsKey(token)) {
                int modelID = tokenToModelID[token];
                if (baseModelDic.ContainsKey(modelID)) {
                    if (!baseModelDic[modelID].IsDead()) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                else {
                    tokenToModelID.Remove(token);
                    return false;
                }
            }

            return false;
        }

        BaseModel GetModelByToken(BaseToken token) {
            if (IsVaildUserModel(token)) {
                return baseModelDic[tokenToModelID[token]];
            }
            else {
                return null;
            }
        }
    }

    public enum FightRoomState {
        Init,
        Ready,
        Fight,
        Over
    }
}
