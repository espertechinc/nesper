///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.codegen
{
    public class ResultSetProcessorCodegenNames
    {
        public const string NAME_AGENTINSTANCECONTEXT = "agentInstanceContext";
        public const string NAME_SELECTEXPRPROCESSOR = "selectExprProcessor";
        public const string NAME_AGGREGATIONSVC = "aggregationService";
        public const string NAME_ORDERBYPROCESSOR = "orderByProcessor";
        public const string NAME_STATEMENT_FIELDS = "statementFields";
        public const string NAME_NEWDATA = "newData";
        public const string NAME_OLDDATA = "oldData";
        public const string NAME_ISSYNTHESIZE = "isSynthesize";
        public const string NAME_ISNEWDATA = "isNewData";
        public const string NAME_JOINSET = "joinset";
        public const string NAME_VIEWABLE = "viewable";
        public const string NAME_VIEWEVENTSLIST = "viewEventsList";
        public const string NAME_JOINEVENTSSET = "joinEventsSet";
        public const string NAME_RESULTSETVISITOR = "visitor";
        public const string NAME_HAVINGEVALUATOR_ARRAYNONMEMBER = "havingEvaluatorArray";
        
        public const string NAME_SELECTEXPRPROCESSOR_MEMBER = "o.selectExprProcessor";
        public const string NAME_SELECTEXPRPROCESSOR_ARRAY = "o.selectExprProcessorArray";
        public const string NAME_HAVINGEVALUATOR_ARRAY_MEMBER = "o." + NAME_HAVINGEVALUATOR_ARRAYNONMEMBER;

        public static readonly CodegenExpressionMember MEMBER_AGENTINSTANCECONTEXT = new CodegenExpressionMember(NAME_AGENTINSTANCECONTEXT);
        public static readonly CodegenExpressionMember MEMBER_SELECTEXPRPROCESSOR = new CodegenExpressionMember(NAME_SELECTEXPRPROCESSOR_MEMBER);
        public static readonly CodegenExpressionMember MEMBER_SELECTEXPRPROCESSOR_ARRAY = new CodegenExpressionMember(NAME_SELECTEXPRPROCESSOR_ARRAY);
        public static readonly CodegenExpressionMember MEMBER_HAVINGEVALUATOR_ARRAY = new CodegenExpressionMember(NAME_HAVINGEVALUATOR_ARRAY_MEMBER);
        public static readonly CodegenExpressionMember MEMBER_SELECTEXPRNONMEMBER = new CodegenExpressionMember(NAME_SELECTEXPRPROCESSOR);
        public static readonly CodegenExpressionMember MEMBER_AGGREGATIONSVC = new CodegenExpressionMember(NAME_AGGREGATIONSVC);
        public static readonly CodegenExpressionMember MEMBER_ORDERBYPROCESSOR = new CodegenExpressionMember(NAME_ORDERBYPROCESSOR);
        
        public static readonly CodegenExpressionRef REF_ORDERBYPROCESSOR = new CodegenExpressionRef(NAME_ORDERBYPROCESSOR);
        public static readonly CodegenExpressionRef REF_STATEMENT_FIELDS = new CodegenExpressionRef(NAME_STATEMENT_FIELDS);

        public static readonly CodegenExpressionRef REF_NEWDATA = Ref(NAME_NEWDATA);
        public static readonly CodegenExpressionRef REF_OLDDATA = Ref(NAME_OLDDATA);
        public static readonly CodegenExpressionRef REF_ISSYNTHESIZE = Ref(NAME_ISSYNTHESIZE);
        public static readonly CodegenExpressionRef REF_ISNEWDATA = Ref(NAME_ISNEWDATA);
        public static readonly CodegenExpressionRef REF_JOINSET = Ref(NAME_JOINSET);
        public static readonly CodegenExpressionRef REF_VIEWABLE = Ref(NAME_VIEWABLE);
        public static readonly CodegenExpressionRef REF_VIEWEVENTSLIST = Ref(NAME_VIEWEVENTSLIST);
        public static readonly CodegenExpressionRef REF_JOINEVENTSSET = Ref(NAME_JOINEVENTSSET);
        public static readonly CodegenExpressionRef REF_RESULTSETVISITOR = Ref(NAME_RESULTSETVISITOR);
    }
} // end of namespace