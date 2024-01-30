using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plain.reverse
{
    public partial class ExprDotForgeReverse
    {
        private class EnumForgeDescFactoryReverse : EnumForgeDescFactory
        {
            private readonly EPChainableType _type;
            private readonly bool _isScalar;

            public EnumForgeDescFactoryReverse(EPChainableType type, bool isScalar)
            {
                _type = type;
                _isScalar = isScalar;
            }

            public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
            {
                throw new IllegalStateException("No lambda expected");
            }

            public EnumForgeDesc MakeEnumForgeDesc(
                IList<ExprDotEvalParam> bodiesAndParameters,
                int streamCountIncoming,
                StatementCompileTimeServices services)
            {
                EnumForge forge = new EnumReverseForge(streamCountIncoming, _isScalar);
                return new EnumForgeDesc(_type, forge);
            }
        }
    }
}