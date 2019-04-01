///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class ExprForgeCodegenNames
    {
        public const string NAME_EPS = "eventsPerStream";
        public const string NAME_ISNEWDATA = "isNewData";
        public const string NAME_EXPREVALCONTEXT = "exprEvalCtx";

        public static readonly CodegenExpressionRef REF_EPS = Ref(NAME_EPS);
        public static readonly CodegenExpressionRef REF_ISNEWDATA = Ref(NAME_ISNEWDATA);
        public static readonly CodegenExpressionRef REF_EXPREVALCONTEXT = Ref(NAME_EXPREVALCONTEXT);

        public static readonly CodegenNamedParam FP_EPS = new CodegenNamedParam(typeof(EventBean[]), NAME_EPS);
        public static readonly CodegenNamedParam FP_ISNEWDATA = new CodegenNamedParam(typeof(bool), NAME_ISNEWDATA);

        public static readonly CodegenNamedParam FP_EXPREVALCONTEXT =
            new CodegenNamedParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);

        public static readonly IList<CodegenNamedParam> PARAMS = Collections.List(
            FP_EPS, FP_ISNEWDATA, FP_EXPREVALCONTEXT);
    }
} // end of namespace