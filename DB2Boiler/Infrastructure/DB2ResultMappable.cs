using IBM.Data.Db2;
using System.Data.Common;

namespace DB2Boiler.Infrastructure
{

    /// <summary>
    /// This class enforces the requirement that the inheriting class has a method that
    /// can be called to map the data from a DbDataReader to that class
    /// </summary>
    public abstract class DB2ResultMappable
    {
        public abstract void GetDataFromReader(DbDataReader reader);
        public abstract void GetDataFromOutputParameters(DB2Command command);
    }
}
