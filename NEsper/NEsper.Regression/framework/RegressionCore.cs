using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

            // Go delete all my previous run's data
            var srcDir = @"C:\src\Espertech\NEsper-master\NEsper\NEsper.Regression.Review";
            foreach (var srcFile in Directory.GetFiles(srcDir, "*.cs"))
            {
                File.Delete(srcFile);
            }

            var dllDir = @"C:\src\Espertech\NEsper-master\NEsper\NEsper.Regression.Runner\bin\Debug\net462";
            foreach (var dllFile in Directory.GetFiles(dllDir, "NEsper_*.dll"))
            {
                try
                {
                    File.Delete(dllFile);
                }
                catch
                {
                }
            }
        }

        public static void Initialize()
        {
        }
    }
}
