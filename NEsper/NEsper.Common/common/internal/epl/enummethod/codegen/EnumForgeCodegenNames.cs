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
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.enummethod.codegen
{
    public class EnumForgeCodegenNames
    {
        public static readonly CodegenExpressionRef REF_ENUMCOLL = Ref("enumcoll");
        public static readonly CodegenExpressionRef REF_EPS = Ref(NAME_EPS);

        public static readonly CodegenNamedParam FP_ENUMCOLL = new CodegenNamedParam(
            typeof(ICollection<object>), REF_ENUMCOLL);

        public static readonly IList<CodegenNamedParam> PARAMS = Collections.List(
            FP_EPS, FP_ENUMCOLL, FP_ISNEWDATA, FP_EXPREVALCONTEXT);
    }
} // end of namespace