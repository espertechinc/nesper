using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.index.service
{
    public class EventAdvancedIndexProvisionDesc
    {
        public EventAdvancedIndexProvisionDesc(AdvancedIndexDesc indexDesc, ExprEvaluator[] parameters,
            EventAdvancedIndexFactory factory, EventAdvancedIndexConfigStatement configStatement)
        {
            IndexDesc = indexDesc;
            Parameters = parameters;
            Factory = factory;
            ConfigStatement = configStatement;
        }

        public AdvancedIndexDesc IndexDesc { get; }

        public ExprEvaluator[] Parameters { get; }

        public EventAdvancedIndexFactory Factory { get; }

        public EventAdvancedIndexConfigStatement ConfigStatement { get; }
    }
} // end of namespace