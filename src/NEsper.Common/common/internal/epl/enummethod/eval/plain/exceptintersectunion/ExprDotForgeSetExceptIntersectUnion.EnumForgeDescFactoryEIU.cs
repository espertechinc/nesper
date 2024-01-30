using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plain.exceptintersectunion
{
    public partial class ExprDotForgeSetExceptIntersectUnion
    {
        private class EnumForgeDescFactoryEIU : EnumForgeDescFactory
        {
            private readonly EnumMethodEnum _enumMethod;
            private readonly EPChainableType _type;
            private readonly ExprDotEnumerationSourceForge _enumSrc;

            public EnumForgeDescFactoryEIU(
                EnumMethodEnum enumMethod,
                EPChainableType type,
                ExprDotEnumerationSourceForge enumSrc)
            {
                _enumMethod = enumMethod;
                _type = type;
                _enumSrc = enumSrc;
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
                var scalar = _type is EPChainableTypeClass;
                EnumForge forge;
                if (_enumMethod == EnumMethodEnum.UNION) {
                    forge = new EnumUnionForge(streamCountIncoming, _enumSrc.Enumeration, scalar);
                }
                else if (_enumMethod == EnumMethodEnum.INTERSECT) {
                    forge = new EnumIntersectForge(streamCountIncoming, _enumSrc.Enumeration, scalar);
                }
                else if (_enumMethod == EnumMethodEnum.EXCEPT) {
                    forge = new EnumExceptForge(streamCountIncoming, _enumSrc.Enumeration, scalar);
                }
                else {
                    throw new ArgumentException("Invalid enumeration method for this factory: " + _enumMethod);
                }

                return new EnumForgeDesc(_type, forge);
            }
        }
    }
}