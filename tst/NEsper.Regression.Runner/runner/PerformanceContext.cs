using System;
using System.Reflection;

using NUnit.Framework.Internal;

namespace com.espertech.esper.regressionrun.runner
{
    public class PerformanceContext : IDisposable
    {
        public static readonly PropertyInfo ContextProperty = typeof(TestExecutionContext).GetProperty("CurrentContext");

        private readonly TestExecutionContext _priorContext;
            
        public PerformanceContext()
        {
            _priorContext = TestExecutionContext.CurrentContext;
            ContextProperty.SetValue(null, null);
        }

        public void Dispose()
        {
            ContextProperty.SetValue(null, _priorContext);
        }
    }
}