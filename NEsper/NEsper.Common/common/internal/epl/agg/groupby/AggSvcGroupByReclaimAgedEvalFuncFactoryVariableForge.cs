///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    public class AggSvcGroupByReclaimAgedEvalFuncFactoryVariableForge : AggSvcGroupByReclaimAgedEvalFuncFactoryForge
    {
        private readonly VariableMetaData variableMetaData;

        public AggSvcGroupByReclaimAgedEvalFuncFactoryVariableForge(VariableMetaData variableMetaData)
        {
            this.variableMetaData = variableMetaData;
        }

        public CodegenExpressionField Make(CodegenClassScope classScope)
        {
            CodegenExpression resolve = VariableDeployTimeResolver.MakeResolveVariable(variableMetaData, EPStatementInitServicesConstants.REF);
            return classScope.AddFieldUnshared(
                true, typeof(AggSvcGroupByReclaimAgedEvalFuncFactoryVariable),
                NewInstance(typeof(AggSvcGroupByReclaimAgedEvalFuncFactoryVariable), resolve));
        }
    }
} // end of namespace