///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public class IntervalForgeCodegenNames
    {
        protected internal static readonly CodegenExpressionRef REF_LEFTSTART = @Ref("leftStart");
        protected internal static readonly CodegenExpressionRef REF_LEFTEND = @Ref("leftEnd");
        protected internal static readonly CodegenExpressionRef REF_RIGHTSTART = @Ref("rightStart");
        protected internal static readonly CodegenExpressionRef REF_RIGHTEND = @Ref("rightEnd");

        protected internal static readonly CodegenNamedParam FP_LEFTSTART = new CodegenNamedParam(typeof(long), REF_LEFTSTART);
        protected internal static readonly CodegenNamedParam FP_LEFTEND = new CodegenNamedParam(typeof(long), REF_LEFTEND);
        protected internal static readonly CodegenNamedParam FP_RIGHTSTART = new CodegenNamedParam(typeof(long), REF_RIGHTSTART);
        protected internal static readonly CodegenNamedParam FP_RIGHTEND = new CodegenNamedParam(typeof(long), REF_RIGHTEND);

        protected internal static readonly IList<CodegenNamedParam> PARAMS = new List<CodegenNamedParam>() {
            FP_LEFTSTART,
            FP_LEFTEND,
            FP_RIGHTSTART,
            FP_RIGHTEND
        };
    }
} // end of namespace