///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

        public const string LAMBDA_NAME_EPS = "_eventsPerStream";
        public const string LAMBDA_NAME_ISNEWDATA = "_isNewData";
        public const string LAMBDA_NAME_EXPREVALCONTEXT = "_exprEvalCtx";

        public static readonly CodegenExpressionRef REF_EPS = Ref(NAME_EPS);
        public static readonly CodegenExpressionRef REF_ISNEWDATA = Ref(NAME_ISNEWDATA);
        public static readonly CodegenExpressionRef REF_EXPREVALCONTEXT = Ref(NAME_EXPREVALCONTEXT);

        public static readonly CodegenExpressionRef LAMBDA_REF_EPS = Ref(LAMBDA_NAME_EPS);
        public static readonly CodegenExpressionRef LAMBDA_REF_ISNEWDATA = Ref(LAMBDA_NAME_ISNEWDATA);
        public static readonly CodegenExpressionRef LAMBDA_REF_EXPREVALCONTEXT = Ref(LAMBDA_NAME_EXPREVALCONTEXT);

        public static readonly CodegenNamedParam FP_EPS =
            new CodegenNamedParam(typeof(EventBean[]), NAME_EPS);

        public static readonly CodegenNamedParam FP_ISNEWDATA =
            new CodegenNamedParam(typeof(bool), NAME_ISNEWDATA);

        public static readonly CodegenNamedParam FP_EXPREVALCONTEXT =
            new CodegenNamedParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);

        public static readonly CodegenNamedParam LAMBDA_FP_EPS =
            new CodegenNamedParam(typeof(EventBean[]), LAMBDA_NAME_EPS);

        public static readonly CodegenNamedParam LAMBDA_FP_ISNEWDATA =
            new CodegenNamedParam(typeof(bool), LAMBDA_NAME_ISNEWDATA);

        public static readonly CodegenNamedParam LAMBDA_FP_EXPREVALCONTEXT =
            new CodegenNamedParam(typeof(ExprEvaluatorContext), LAMBDA_NAME_EXPREVALCONTEXT);

        public static readonly IList<CodegenNamedParam> PARAMS = Collections.List(
            FP_EPS,
            FP_ISNEWDATA,
            FP_EXPREVALCONTEXT);

        public static readonly IList<CodegenNamedParam> LAMBDA_PARAMS = Collections.List(
            LAMBDA_FP_EPS,
            LAMBDA_FP_ISNEWDATA,
            LAMBDA_FP_EXPREVALCONTEXT);
    }
} // end of namespace