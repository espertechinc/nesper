///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.codegen
{
	public class ResultSetProcessorCodegenNames {
	    public const string NAME_AGENTINSTANCECONTEXT = "agentInstanceContext";
	    public const string NAME_SELECTEXPRPROCESSOR_MEMBER = "o.selectExprProcessor";
	    public const string NAME_SELECTEXPRPROCESSOR_ARRAY = "o.selectExprProcessorArray";
	    public const string NAME_SELECTEXPRPROCESSOR = "selectExprProcessor";
	    public const string NAME_AGGREGATIONSVC = "aggregationService";
	    public const string NAME_ORDERBYPROCESSOR = "orderByProcessor";
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
	    public const string NAME_HAVINGEVALUATOR_ARRAY_MEMBER = "o." + NAME_HAVINGEVALUATOR_ARRAYNONMEMBER;

	    public readonly static CodegenExpressionRef REF_AGENTINSTANCECONTEXT = new CodegenExpressionRef(NAME_AGENTINSTANCECONTEXT);
	    public readonly static CodegenExpressionRef REF_SELECTEXPRPROCESSOR = new CodegenExpressionRef(NAME_SELECTEXPRPROCESSOR_MEMBER);
	    public readonly static CodegenExpressionRef REF_SELECTEXPRPROCESSOR_ARRAY = new CodegenExpressionRef(NAME_SELECTEXPRPROCESSOR_ARRAY);
	    public readonly static CodegenExpressionRef REF_HAVINGEVALUATOR_ARRAY = new CodegenExpressionRef(NAME_HAVINGEVALUATOR_ARRAY_MEMBER);
	    public readonly static CodegenExpressionRef REF_SELECTEXPRNONMEMBER = new CodegenExpressionRef(NAME_SELECTEXPRPROCESSOR);
	    public readonly static CodegenExpressionRef REF_AGGREGATIONSVC = new CodegenExpressionRef(NAME_AGGREGATIONSVC);
	    public readonly static CodegenExpressionRef REF_ORDERBYPROCESSOR = new CodegenExpressionRef(NAME_ORDERBYPROCESSOR);
	    public readonly static CodegenExpressionRef REF_NEWDATA = @Ref(NAME_NEWDATA);
	    public readonly static CodegenExpressionRef REF_OLDDATA = @Ref(NAME_OLDDATA);
	    public readonly static CodegenExpressionRef REF_ISSYNTHESIZE = @Ref(NAME_ISSYNTHESIZE);
	    public readonly static CodegenExpressionRef REF_ISNEWDATA = @Ref(NAME_ISNEWDATA);
	    public readonly static CodegenExpressionRef REF_JOINSET = @Ref(NAME_JOINSET);
	    public readonly static CodegenExpressionRef REF_VIEWABLE = @Ref(NAME_VIEWABLE);
	    public readonly static CodegenExpressionRef REF_VIEWEVENTSLIST = @Ref(NAME_VIEWEVENTSLIST);
	    public readonly static CodegenExpressionRef REF_JOINEVENTSSET = @Ref(NAME_JOINEVENTSSET);
	    public readonly static CodegenExpressionRef REF_RESULTSETVISITOR = @Ref(NAME_RESULTSETVISITOR);
	}
} // end of namespace