///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.epl.dataflow.core.EPDataFlowServiceImpl;

namespace com.espertech.esper.common.@internal.epl.dataflow.ops
{
    public class EPStatementSourceForge : DataFlowOperatorForge
    {
#pragma warning disable 649
        [DataFlowOpParameter] private ExprNode statementDeploymentId;

        [DataFlowOpParameter] private ExprNode statementName;

        [DataFlowOpParameter] private IDictionary<string, object> statementFilter; // interface EPDataFlowEPStatementFilter

        [DataFlowOpParameter] private IDictionary<string, object> collector; // interface EPDataFlowIRStreamCollector
#pragma warning restore 649

        private bool submitEventBean;

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            if (context.OutputPorts.Count != 1) {
                throw new ArgumentException(
                    "EPStatementSource operator requires one output stream but produces " + context.OutputPorts.Count + " streams");
            }

            if (statementName != null && statementFilter != null) {
                throw new ExprValidationException("Both 'statementName' or 'statementFilter' parameters were provided, only either one is expected");
            }

            if ((statementDeploymentId == null && statementName != null) |
                (statementDeploymentId != null && statementName == null)) {
                throw new ExprValidationException("Both 'statementDeploymentId' and 'statementName' are required when either of these are specified");
            }

            DataFlowOpOutputPort portZero = context.OutputPorts[0];
            if (portZero != null && portZero.OptionalDeclaredType != null && portZero.OptionalDeclaredType.IsWildcard) {
                submitEventBean = true;
            }

            return null;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(
                    OP_PACKAGE_NAME + ".epstatementsource.EPStatementSourceFactory", this.GetType(), "stmtSrc", parent, symbols, classScope)
                .Exprnode("statementDeploymentId", statementDeploymentId)
                .Exprnode("statementName", statementName)
                .Map("statementFilter", statementFilter)
                .Map("collector", collector)
                .Constant("submitEventBean", submitEventBean)
                .Build();
        }
    }
} // end of namespace