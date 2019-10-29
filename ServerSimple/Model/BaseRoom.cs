using NetFrame.AbsClass;
using NetFrame.Base;
using NetFrame.Interfaces;
using ServerSimple.Manager;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerSimple.Model
{
    public abstract class BaseRoom: BroadcastSender, IMessageHandler{

        public long id;

        public abstract void OnClientClose(BaseToken token, string error);

        public abstract void OnClientConnected(BaseToken token);

        public abstract void Clear();
    }
}
