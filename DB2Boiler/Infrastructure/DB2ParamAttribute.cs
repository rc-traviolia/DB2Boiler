using IBM.Data.Db2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB2Boiler.Infrastructure
{
    public class DB2ParamAttribute : Attribute
    {
        public ParameterDirection Direction { get; set; }
        public DB2Type DB2Type { get; set; }
        public int Size { get; set; }

        public DB2ParamAttribute(ParameterDirection direction, DB2Type db2Type, int size)
        {
            Direction = direction;
            DB2Type = db2Type;
            Size = size;
        }
    }
}
