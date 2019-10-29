using System;
using System.Collections.Generic;
using System.Text;

namespace ServerSimple.Base
{
    public class SingleClass<T>where T: SingleClass<T> {
        protected static T ins;
        public static T Ins {
            get {
                if (ins == null) {
                    ins = Activator.CreateInstance<T>();
                }
                return ins;
            }
        }
    }
}
