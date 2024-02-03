///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base {
    public class ThreeFormNoParamFactory : EnumForgeDescFactory {
        private readonly EPChainableType returnType;
        private readonly ForgeFunction function;

        public ThreeFormNoParamFactory(
            EPChainableType returnType,
            ForgeFunction function)
        {
            this.returnType = returnType;
            this.function = function;
        }

        public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
        {
            return new EnumForgeLambdaDesc(Array.Empty<EventType>(), Array.Empty<string>());
        }

        public EnumForgeDesc MakeEnumForgeDesc(
            IList<ExprDotEvalParam> bodiesAndParameters,
            int streamCountIncoming,
            StatementCompileTimeServices statementCompileTimeService)
        {
            return new EnumForgeDesc(returnType, function.Invoke(streamCountIncoming));
        }

        public delegate EnumForge ForgeFunction(int streamCountIncoming);
    }
} // end of namespace