using ServerSimple.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerSimple.Factory
{
    public class BaseModelDataFactory
    {
        static Dictionary<int, BaseModelData> modelDataDic = new Dictionary<int, BaseModelData>();

        static BaseModelDataFactory() {
            modelDataDic.Add(1, new BaseModelData(1, "machine_001", new GrowValue(500, 50),
                                                                 new GrowValue(1000, 50),
                                                                 new GrowValue(10, 2),
                                                                 new GrowValue(100, 10),
                                                                 new GrowValue(20, 5),
                                                                 new GrowValue(50, 10)));
        }

        public static BaseModelData GetDataByID(int id) {
            if (modelDataDic.ContainsKey(id)) {
                return modelDataDic[id];
            }
            return null;
        }
    }
}
