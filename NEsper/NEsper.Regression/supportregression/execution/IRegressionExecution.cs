using com.espertech.esper.client;

namespace com.espertech.esper.supportregression.execution
{
    public interface IRegressionExecution
    {
        void Configure(Configuration configuration);
        void Run(EPServiceProvider epService);
        bool ExcludeWhenInstrumented();
    }
}