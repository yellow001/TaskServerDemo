using NetFrame.Base;
using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using ServerSimple.Model;
using ServerSimple.DTO.Login;
using System.Linq;

namespace ServerSimple.DTO.Fight
{
    [Serializable]
    [ProtoContract]
    public class FightRoomDTO
    {
        [ProtoMember(1)]
        public Dictionary<string, int> nameToModelID = new Dictionary<string, int>();

        [ProtoMember(2)]
        public Dictionary<int, BaseModel> baseModelDic = new Dictionary<int, BaseModel>();

        [ProtoMember(3)]
        public float allTime = 0;

        public FightRoomDTO() { }

        public FightRoomDTO(FightRoom room) {

            foreach (var item in room.tokenToModelID.Keys) {
                nameToModelID.Add(room.tokenToUserDTO[item].name, room.tokenToModelID[item]);
            }
            baseModelDic = room.baseModelDic;

            allTime = room.allTime;
        }
    }
}
