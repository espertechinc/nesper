///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    public class AggSvcGroupByReclaimAgedEvalFuncFactoryVariableForge : AggSvcGroupByReclaimAgedEvalFuncFactoryForge
    {
        private readonly VariableMetaData _variableMetaData;

        public AggSvcGroupByReclaimAgedEvalFuncFactoryVariableForge(VariableMetaData variableMetaData)
        {
            this._variableMetaData = variableMetaData;
        }

        public CodegenExpressionInstanceField Make(CodegenClassScope classScope)
        {
            CodegenExpression resolve = VariableDeployTimeResolver.MakeResolveVariable(
                _variableMetaData,
                EPStatementInitServicesConstants.REF);
            return classScope.AddDefaultFieldUnshared(
                true,
                typeof(AggSvcGroupByReclaimAgedEvalFuncFactoryVariable),
                NewInstance<AggSvcGroupByReclaimAgedEvalFuncFactoryVariable>(resolve));
        }
    }
} // end of namespace