using System;
using System.IO;
using System.Reflection;

using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.db.drivers;

namespace com.espertech.esper.regressionlib.framework
{
    public class RegressionCore
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Type DatabaseDriver = typeof(DbDriverPgSQL);

        static RegressionCore()
        {
            Log.Info("Database Driver Loaded: {0}", DatabaseDriver);
        }

        public static void Initialize()
        {
        }
    }
}