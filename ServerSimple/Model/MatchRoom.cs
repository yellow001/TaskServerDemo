using NetFrame.AbsClass;
using NetFrame.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using NetFrame.Base;
using ServerSimple.Cache;
using ServerSimple.Manager;
using ServerSimple.DTO.Match;
using System.Linq;
using ServerSimple.Public;
using ServerSimple.DTO.Login;

namespace ServerSimple.Model
{
    public class MatchRoom : BaseRoom {

        /// <summary>
        /// 房间id
        /// </summary>
        //public int id;

        /// <summary>
        /// 最多人数(默认为10)
        /// </summary>
        public int maxNum=10;

        public string masterName;

        /// <summary>
        /// 房间密码
        /// </summary>
        public string passwd;

        public Dictionary<string, BaseToken> allTokens = new Dictionary<string, BaseToken>();

        public MatchManager manager;

        MatchRoomDTO dto;
        public MatchRoom() {
        }


        /// <summary>
        /// 进入房间请求
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// -4 密码错误
        /// -5 人数已满
        /// -6 进入房间出错
        /// </returns>
        public int EnterCREQ(BaseToken token,TransModel model) {

            if (allTokens.Count >= maxNum) {
                return -5;
            }


            if (string.IsNullOrEmpty(passwd)) {
                if (!manager.tokenToRoomID.TryAdd(token, id)) {
                    return -6;
                }

                string name = UserCache.Ins.GetDALByToken(token).name;
                allTokens.Add(name, token);
                //发送进入广播,（房间信息）
                Broadcast(allTokens.Values.ToList(), 1002006, 1, GetDTO());
                return 1;
            }
            else {
                string pwd = model.GetMsg<string>();
                if (string.IsNullOrEmpty(pwd) || !pwd.Equals(passwd)) {
                    return -4;
                }

                if (!manager.tokenToRoomID.TryAdd(token, id)) {
                    return -6;
                }

                string name = UserCache.Ins.GetDALByToken(token).name;
                allTokens.Add(name, token);
                //发送进入广播,（房间信息）
                Broadcast(allTokens.Values.ToList(), 1002006, 1, GetDTO());
                return 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// 1  离开成功
        /// -3 连接不在该房间中
        /// </returns>
        public int ExitCREQ(BaseToken token) {
            long id;
            if (!allTokens.Values.Contains(token)) {
                manager.tokenToRoomID.Remove(token, out id);
                return -3;
            }

            //发送的area 0表示有人退出 1表示自己退出（房主退出时所有人都要退出）
            string name = UserCache.Ins.GetDALByToken(token).name;
            if (name.Equals(masterName)) {
                //离开的是房主
                Broadcast(allTokens.Values.ToList(), 1002008, 1);
                Clear();
            }
            else {
                //离开的不是房主
                Send(token, 1002008, 1);
                manager.tokenToRoomID.Remove(token, out id);
                allTokens.Remove(name);
                Broadcast(allTokens.Values.ToList(), 1002008, 0, GetDTO());
            }
            return 1;
        }

        public void StartGame() {
            Broadcast(allTokens.Values.ToList(), 1002011, 1);
            
            //初始化战斗房间  清空本房间
            StaticFunc.CreateFightRoom?.Invoke(allTokens);

            Clear();
        }

        public void Remove(string n) {
            long id;
            Send(allTokens[n], 1002008, 1);
            manager.tokenToRoomID.Remove(allTokens[n], out id);
            allTokens.Remove(n);
            Broadcast(allTokens.Values.ToList(), 1002008, 0, GetDTO());
        }

        public override void Clear() {
            //清除manager中的对应数据
            long id;
            MatchRoom room;
            foreach (var item in allTokens.Values) {
                manager.tokenToRoomID.Remove(item, out id);
            }
            manager.roomCache.Remove(this.id, out room);

            //重置自身数据
            allTokens.Clear();
            passwd = "";

            //加入到manager的栈中
            manager.roomStack.Push(this);
        }

        public override void OnClientClose(BaseToken token, string error) {
        }

        public override void OnClientConnected(BaseToken token) {
        }

        public MatchRoomDTO GetDTO() {
            if (dto == null) {
                dto = new MatchRoomDTO(id, maxNum, masterName, GetUserDTOList(), string.IsNullOrEmpty(passwd) ? null : "passwd");
            }
            else {
                dto.maxNum = maxNum;
                dto.masterName = masterName;
                dto.playerList.Clear();
                dto.playerList.AddRange(GetUserDTOList());
                dto.passwd = string.IsNullOrEmpty(passwd) ? null : "passwd";
            }

            return dto;
        }

        UserDTO[] GetUserDTOList() {
            List<UserDTO> list=new List<UserDTO>();
            foreach (var item in allTokens) {
                list.Add(LoginManager.Ins.GetDTO(item.Key));
            }
            return list.ToArray();
        }
    }
}
