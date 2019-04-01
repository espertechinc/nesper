using System;
using System.Linq;

using XLR8.CGLib;

using com.espertech.esper.util;

namespace com.espertech.esper.client.scopetest
{
    /// <summary>
    /// Internal abstraction around the assertion process.  This method can proxy its calls to NUnit, System.Diagnostics
    /// or any other assertion framework.
    /// </summary>
    public class AssertProxy
    {
        /// <summary>
        /// An action that is triggered when a condition fails.
        /// </summary>
        public static Action<string> AssertFail { get; set; }

        /// <summary>
        /// Static constructor
        /// </summary>
        static AssertProxy()
        {
            AssertFail = message => System.Diagnostics.Debug.Assert(true, message);

            // See if NUnit is loaded into the domain.  If it is, then by default, we will switch to using it.
            // Remember, you can change whatever you'd like about how the AssertProxy class works by simply
            // changing the AssertFail property.

            var appDomain = AppDomain.CurrentDomain;
            var assemblies = appDomain.GetAssemblies();
            var appTypes = assemblies.SelectMany(assembly => assembly.GetTypes()).ToArray();

            var nunitAssertionType = TypeHelper.ResolveType("NUnit.Framework.Assert", false);
            if (nunitAssertionType != null)
            {
                var asFastClass = FastClass.Create(nunitAssertionType);
                var asFastMethod = asFastClass?.GetMethod("Fail", new Type[] { typeof(string) });
                if (asFastMethod != null)
                {
                    AssertFail = message => asFastMethod.InvokeStatic(message);
                }
            }
        }

        private static string SanitizeMessage(string message, string defaultMessage)
        {
            if (message == null)
            {
                return defaultMessage;
            }

            return message;
        }


        public static void True(bool condition, string message = null)
        {
            if (!condition)
            {
                AssertFail(SanitizeMessage(message, "Expected true, but received false"));
            }
        }

        public static void False(bool condition, string message = null)
        {
            if (condition)
            {
                AssertFail(SanitizeMessage(message, "Expected false, but received true"));
            }
        }

        public static void Fail(string message = null)
        {
            AssertFail(SanitizeMessage(message, string.Empty));
        }
    }
}
