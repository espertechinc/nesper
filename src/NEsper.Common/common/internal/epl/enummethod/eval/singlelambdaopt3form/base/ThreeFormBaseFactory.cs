///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base {
    public abstract class ThreeFormBaseFactory : EnumForgeDescFactory {
        private readonly ThreeFormInitFunction _returnType;

        protected abstract EnumForge MakeForgeWithParam(
            ExprDotEvalParamLambda lambda,
            EPChainableType typeInfo,
            StatementCompileTimeServices services);

        public abstract EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum);

        public ThreeFormBaseFactory(ThreeFormInitFunction returnType)
        {
            _returnType = returnType;
        }

        public EnumForgeDesc MakeEnumForgeDesc(
            IList<ExprDotEvalParam> bodiesAndParameters,
            int streamCountIncoming,
            StatementCompileTimeServices services)
        {
            if (bodiesAndParameters.IsEmpty()) {
                throw new UnsupportedOperationException();
            }

            var first = (ExprDotEvalParamLambda)bodiesAndParameters[0];
            var typeInfo = _returnType.Invoke(first);
            var forge = MakeForgeWithParam(first, typeInfo, services);
            return new EnumForgeDesc(typeInfo, forge);
        }
    }
} // end of namespace