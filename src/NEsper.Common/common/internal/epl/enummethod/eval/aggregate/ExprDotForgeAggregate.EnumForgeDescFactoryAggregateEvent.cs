using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.aggregate
{
    public partial class ExprDotForgeAggregate
    {
        internal class EnumForgeDescFactoryAggregateEvent : EnumForgeDescFactory
        {
            private readonly ObjectArrayEventType evalEventType;
            private readonly EventType inputEventType;
            private readonly string streamName;
            private readonly int numParameters;

            public EnumForgeDescFactoryAggregateEvent(
                ObjectArrayEventType evalEventType,
                EventType inputEventType,
                string streamName,
                int numParameters)
            {
                this.evalEventType = evalEventType;
                this.inputEventType = inputEventType;
                this.streamName = streamName;
                this.numParameters = numParameters;
            }

            public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
            {
                return new EnumForgeLambdaDesc(
                    new EventType[] { evalEventType, inputEventType },
                    new string[] { evalEventType.Name, streamName });
            }

            public EnumForgeDesc MakeEnumForgeDesc(
                IList<ExprDotEvalParam> bodiesAndParameters,
                int streamCountIncoming,
                StatementCompileTimeServices services)
            {
                var init = bodiesAndParameters[0].BodyForge;
                var compute = (ExprDotEvalParamLambda)bodiesAndParameters[1];
                var forge = new EnumAggregateEvent(
                    streamCountIncoming,
                    init,
                    compute.BodyForge,
                    evalEventType,
                    numParameters);
                var type = EPChainableTypeHelper.SingleValue(init.EvaluationType.GetBoxedType());
                return new EnumForgeDesc(type, forge);
            }
        }
    }
}