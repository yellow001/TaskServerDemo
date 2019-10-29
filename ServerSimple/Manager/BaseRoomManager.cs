
using NetFrame.AbsClass;
using NetFrame.Base;
using NetFrame.Interfaces;
using ServerSimple.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerSimple.Manager
{
    public abstract class BaseRoomManager<M,R> : SingleSender, IMessageHandler where R :BaseRoom where M:BaseRoomManager<M,R>
    {
        static M ins;

        public static M Ins {
            get {
                if (ins == null) {
                    ins = Activator.CreateInstance<M>();
                }
                return ins;
            }
        }


        public ConcurrentDictionary<long, R> roomCache = new ConcurrentDictionary<long, R>();
        public ConcurrentDictionary<BaseToken, long> tokenToRoomID = new ConcurrentDictionary<BaseToken, long>();

        public ConcurrentStack<R> roomStack = new ConcurrentStack<R>();
        protected long index = 0;

        protected abstract void Init();

        /// <summary>
        /// 从栈中获取或新建一个房间
        /// </summary>
        /// <returns></returns>
        protected R GetEmptyRoom() {
            R room;
            if (roomStack.Count == 0) {
                Interlocked.Increment(ref index);
                room = Activator.CreateInstance<R>();
                room.id = index;
                InitRoom(room);
                return room;
            }

            if (roomStack.TryPop(out room)) {
                return room;
            }
            return null;
        }

        protected abstract void InitRoom(BaseRoom r);
        public abstract void OnClientConnected(BaseToken token);
        public abstract void OnClientClose(BaseToken token, string error);

        /// <summary>
        /// 房间是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsRoomByID(long id) {
            return roomCache.ContainsKey(id);
        }

        /// <summary>
        /// 连接是否有对应的房间
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool TokenHasRoom(BaseToken token) {
            return tokenToRoomID.ContainsKey(token);
        }

        /// <summary>
        /// 连接有对应房间并且该房间存在
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsVaildToken(BaseToken token) {
            if (TokenHasRoom(token)) {
                long id = tokenToRoomID[token];
                if (ContainsRoomByID(id)) {
                    return true;
                }
                else {
                    tokenToRoomID.Remove(token, out id);
                    return false;
                }
            }
            else {
                return false;
            }
        }
    }
}
