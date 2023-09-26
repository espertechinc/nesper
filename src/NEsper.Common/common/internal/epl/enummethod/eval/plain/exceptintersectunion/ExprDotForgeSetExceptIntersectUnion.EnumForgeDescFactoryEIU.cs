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
            private readonly EnumMethodEnum enumMethod;
            private readonly EPChainableType type;
            private readonly ExprDotEnumerationSourceForge enumSrc;

            public EnumForgeDescFactoryEIU(
                EnumMethodEnum enumMethod,
                EPChainableType type,
                ExprDotEnumerationSourceForge enumSrc)
            {
                this.enumMethod = enumMethod;
                this.type = type;
                this.enumSrc = enumSrc;
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
                var scalar = type is EPChainableTypeClass;
                EnumForge forge;
                if (enumMethod == EnumMethodEnum.UNION) {
                    forge = new EnumUnionForge(streamCountIncoming, enumSrc.Enumeration, scalar);
                }
                else if (enumMethod == EnumMethodEnum.INTERSECT) {
                    forge = new EnumIntersectForge(streamCountIncoming, enumSrc.Enumeration, scalar);
                }
                else if (enumMethod == EnumMethodEnum.EXCEPT) {
                    forge = new EnumExceptForge(streamCountIncoming, enumSrc.Enumeration, scalar);
                }
                else {
                    throw new ArgumentException("Invalid enumeration method for this factory: " + enumMethod);
                }

                return new EnumForgeDesc(type, forge);
            }
        }
    }
}