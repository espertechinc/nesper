///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    public class OutputProcessViewCodegenNames
    {
        public const string NAME_RESULTSETPROCESSOR = "resultSetProcessor";
        public const string NAME_STATEMENTRESULTSVC = "statementResultService";
        public const string NAME_PARENTVIEW = "parentView";
        public const string NAME_JOINEXECSTRATEGY = "joinExecutionStrategy";
        public const string NAME_AGENTINSTANCECONTEXT = "agentInstanceContext";

        public static readonly CodegenExpressionMember MEMBER_CHILD = Member("child");
        public static readonly CodegenExpressionMember MEMBER_RESULTSETPROCESSOR = Member(NAME_RESULTSETPROCESSOR);
        public static readonly CodegenExpressionMember MEMBER_AGENTINSTANCECONTEXT = Member(NAME_AGENTINSTANCECONTEXT);
    }
} // end of namespace