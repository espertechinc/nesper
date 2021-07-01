///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        public const string NAME_ENGINEIMPORTSVC = "classpathImportService";
        public const string NAME_ISSUBQUERY = "isSubquery";
        public const string NAME_SUBQUERYNUMBER = "subqueryNumber";
        public const string NAME_GROUPKEY = "groupKey";
        public const string NAME_AGENTINSTANCEID = "agentInstanceId";
        public const string NAME_ROLLUPLEVEL = "rollupLevel";
        public const string NAME_CALLBACK = "callback";
        public const string NAME_AGGVISITOR = "visitor";
        public const string NAME_COLUMN = "column";
        public const string NAME_GROUPID = "groupId";

        public static readonly CodegenExpressionRef REF_ENGINEIMPORTSVC = Ref(NAME_ENGINEIMPORTSVC);
        public static readonly CodegenExpressionRef REF_ISSUBQUERY = Ref(NAME_ISSUBQUERY);
        public static readonly CodegenExpressionRef REF_SUBQUERYNUMBER = Ref(NAME_SUBQUERYNUMBER);
        public static readonly CodegenExpressionRef REF_GROUPKEY = Ref(NAME_GROUPKEY);
        public static readonly CodegenExpressionRef REF_ROLLUPLEVEL = Ref(NAME_ROLLUPLEVEL);
        public static readonly CodegenExpressionRef REF_CALLBACK = Ref(NAME_CALLBACK);
        public static readonly CodegenExpressionRef REF_AGGVISITOR = Ref(NAME_AGGVISITOR);
        public static readonly CodegenExpressionRef REF_COLUMN = Ref(NAME_COLUMN);
        public static readonly CodegenExpressionRef REF_GROUPID = Ref(NAME_GROUPID);
    }
} // end of namespace