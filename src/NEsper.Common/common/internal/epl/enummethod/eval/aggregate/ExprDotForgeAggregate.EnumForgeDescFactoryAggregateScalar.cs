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
        internal class EnumForgeDescFactoryAggregateScalar : EnumForgeDescFactory
        {
            private readonly ObjectArrayEventType evalEventType;

            public EnumForgeDescFactoryAggregateScalar(ObjectArrayEventType evalEventType)
            {
                this.evalEventType = evalEventType;
            }

            public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
            {
                return new EnumForgeLambdaDesc(new EventType[] { evalEventType }, new string[] { evalEventType.Name });
            }

            public EnumForgeDesc MakeEnumForgeDesc(
                IList<ExprDotEvalParam> bodiesAndParameters,
                int streamCountIncoming,
                StatementCompileTimeServices services)
            {
                var init = bodiesAndParameters[0].BodyForge;
                var compute = (ExprDotEvalParamLambda)bodiesAndParameters[1];
                var forge = new EnumAggregateScalar(
                    streamCountIncoming,
                    init,
                    compute.BodyForge,
                    evalEventType,
                    compute.GoesToNames.Count);
                var boxed = init.EvaluationType.GetBoxedType();
                var type = EPChainableTypeHelper.SingleValue(boxed);
                return new EnumForgeDesc(type, forge);
            }
        }
    }
}