using NetFrame.Base;
using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace ServerSimple.Base
{
    [Serializable]
    [ProtoContract]
    public class CidModel:TransModel
    {
        [ProtoMember(1)]
        public long cid;

        public CidModel() { }

        public CidModel(int pid, int a,long id=-1) :base(pid,a){
            cid = id;
        }
    }
}
