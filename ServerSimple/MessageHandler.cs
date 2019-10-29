using NetFrame.AbsClass;
using NetFrame.Base;
using NetFrame.Tool;
using ServerSimple.Manager;
using System;
using System.Collections.Generic;

namespace ServerSimple {
    public class MessageHandler : AbsHandlerCenter {
        static MessageHandler ins;

        Dictionary<int, List<MsgReceive_De>> msgPool;

        public static MessageHandler Ins {
            get {
                if (ins == null) {
                    ins = new MessageHandler();
                }
                return ins;
            }
        }

        private MessageHandler() {
            msgPool = new Dictionary<int, List<MsgReceive_De>>();
        }


        public override void OnClientClose(BaseToken token, string error) {
            //Console.WriteLine(error);
            Debugger.Warn("token close " + error+" "+ token.socket.RemoteEndPoint);

            FightManager.Ins.OnClientClose(token, error);

            MatchManager.Ins.OnClientClose(token, error);

            LoginManager.Ins.OnClientClose(token, error);

        }

        public override void OnClientConnent(BaseToken token) {
            Debugger.Trace("client connect  " + token.socket.RemoteEndPoint);

            LoginManager.Ins.OnClientConnected(token);
            MatchManager.Ins.OnClientConnected(token);
            FightManager.Ins.OnClientConnected(token);
        }

        public override void OnMsgReceive<T>(BaseToken token, T model) {
            Debugger.Trace("client message:  " +model.pID+" "+ token.socket.RemoteEndPoint);

            //Console.WriteLine(model.GetMsg<string>());
            //TransModel m = new TransModel(1001001, 1);
            //m.SetMsg("i am server");
            //token.Send(m);

            if (msgPool.ContainsKey(model.pID)) {
                lock (msgPool[model.pID]) {
                    foreach (var item in msgPool[model.pID]) {
                        item?.Invoke(token, model);
                    }
                }
            }
        }

        /// <summary>
        /// 注册协议对应函数
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="fun"></param>
        public void Register(int pid, MsgReceive_De fun) {
            if (!msgPool.ContainsKey(pid)) {
                List<MsgReceive_De> list = new List<MsgReceive_De>();
                list.Add(fun);
                msgPool.Add(pid, list);
            }
            else {
                if (msgPool[pid].Contains(fun)) {
                    Debugger.Warn(fun.ToString() + " is contain where pid is " + pid);
                }
                else {
                    msgPool[pid].Add(fun);
                }
            }
        }

        /// <summary>
        /// 移除某协议上的所有监听
        /// </summary>
        /// <param name="pid"></param>
        public void ClearPid(int pid) {
            if (!msgPool.ContainsKey(pid)) {
                Debugger.Warn("the pid " + pid + " you want to clear is not contain");
                return;
            }
            msgPool[pid].Clear();
        }

        public void RemoveFunByPid(int pid,MsgReceive_De fun) {
            if (!msgPool.ContainsKey(pid)) {
                Debugger.Warn("the pid " + pid + " you want to remove is not contain");
                return;
            }

            if (!msgPool[pid].Contains(fun)) {
                Debugger.Warn("the fun in pid " + pid + " you want to remove is not contain");
                return;
            }

            msgPool[pid].Remove(fun);
        }

        /// <summary>
        /// 移除所有协议监听
        /// </summary>
        public void RemoveAll() {
            msgPool.Clear();
            GC.Collect();
        }
    }

    public delegate void MsgReceive_De(BaseToken token, TransModel model);
}