using NetFrame.Base;
using NetFrame.EnDecode;
using NetFrame.EnDecode.Extend;
using NetFrame.Tool;
using NLog;
using ServerSimple.Base;
using System;

namespace ServerSimple
{
    class Program
    {
        static void Main(string[] args)
        {
            //BaseServer<TransModel, BaseToken> server = new BaseServer<TransModel, BaseToken>(12345);
            BaseServer<TransModel, H5Token> server = new BaseServer<TransModel, H5Token>(12345);
            AbsCoding.Ins = new PbCoding();
            server.Init(MessageHandler.Ins);
            server.Start();

            DbHelperMySQL.connectionString=AppSetting.Ins.Settings["connstring"];

            //CidModel m = new CidModel(1, 11, 111, -500);
            //byte[] ms = SeProtobuf.Serialization(m);

            //CidModel m2 = SeProtobuf.DeSerialization<CidModel>(ms);

            //Console.WriteLine(m2.cid);

            //Console.WriteLine(AppSetting.GetValue("test"));

            //log.Trace("fuck you");

            //DbHelperMySQL.connectionString = "server=120.25.84.142;database=TaskServer;uid=yellow;pwd=qW789456123=";

            //DataResult dr = DbHelperMySQL.Query("select * from user");

            //Console.WriteLine(dr.rows);

            while (true) {
                Console.ReadLine();
            }
        }
    }
}
