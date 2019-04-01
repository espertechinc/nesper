///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.createvariable
{
    public class StatementAgentInstanceFactoryCreateVariableForge
    {
        private readonly ExprForge optionalInitialValue;
        private readonly string resultSetProcessorProviderClassName;
        private readonly string variableName;

        public StatementAgentInstanceFactoryCreateVariableForge(
            string variableName, ExprForge optionalInitialValue, string resultSetProcessorProviderClassName)
        {
            this.variableName = variableName;
            this.optionalInitialValue = optionalInitialValue;
            this.resultSetProcessorProviderClassName = resultSetProcessorProviderClassName;
        }

        public CodegenMethod InitializeCodegen(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(StatementAgentInstanceFactoryCreateVariable), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(StatementAgentInstanceFactoryCreateVariable), "saiff",
                    NewInstance(typeof(StatementAgentInstanceFactoryCreateVariable)))
                .ExprDotMethod(Ref("saiff"), "setVariableName", Constant(variableName))
                .ExprDotMethod(
                    Ref("saiff"), "setResultSetProcessorFactoryProvider",
                    NewInstance(resultSetProcessorProviderClassName, symbols.GetAddInitSvc(method)));
            if (optionalInitialValue != null)
            {
                method.Block
                    .ExprDotMethod(
                        Ref("saiff"), "setVariableInitialValueExpr",
                        ExprNodeUtilityCodegen.CodegenEvaluator(optionalInitialValue, method, GetType(), classScope))
                    .Expression(
                        ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("addReadyCallback", Ref("saiff")));
            }

            method.Block.MethodReturn(Ref("saiff"));
            return method;
        }
    }
} // end of namespace