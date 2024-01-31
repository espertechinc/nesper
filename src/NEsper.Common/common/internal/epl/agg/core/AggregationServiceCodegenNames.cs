///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationServiceCodegenNames
    {
        public const string NAME_SUBQUERYNUMBER = "subqueryNumber";
        public const string NAME_GROUPKEY = "groupKey";
        public const string NAME_AGENTINSTANCEID = "agentInstanceId";
        public const string NAME_ROLLUPLEVEL = "rollupLevel";
        public const string NAME_CALLBACK = "callback";
        public const string NAME_AGGVISITOR = "visitor";
        public const string NAME_COLUMN = "column";
        public const string NAME_GROUPID = "groupId";
        public const string NAME_SCOL = "scol";
        public const string NAME_VCOL = "vcol";
        public const string NAME_VALUE = "value";
        public const string NAME_STREAMNUM = "streamNum";

        public static readonly CodegenExpressionRef REF_SUBQUERYNUMBER = Ref(NAME_SUBQUERYNUMBER);
        public static readonly CodegenExpressionRef REF_GROUPKEY = Ref(NAME_GROUPKEY);
        public static readonly CodegenExpressionRef REF_ROLLUPLEVEL = Ref(NAME_ROLLUPLEVEL);
        public static readonly CodegenExpressionRef REF_CALLBACK = Ref(NAME_CALLBACK);
        public static readonly CodegenExpressionRef REF_AGGVISITOR = Ref(NAME_AGGVISITOR);
        public static readonly CodegenExpressionRef REF_COLUMN = Ref(NAME_COLUMN);
        public static readonly CodegenExpressionRef REF_GROUPID = Ref(NAME_GROUPID);
        public static readonly CodegenExpressionRef REF_SCOL = Ref(NAME_SCOL);
        public static readonly CodegenExpressionRef REF_VCOL = Ref(NAME_VCOL);
        public static readonly CodegenExpressionRef REF_VALUE = Ref(NAME_VALUE);
        public static readonly CodegenExpressionRef REF_STREAMNUM = Ref(NAME_STREAMNUM);
    }
} // end of namespace