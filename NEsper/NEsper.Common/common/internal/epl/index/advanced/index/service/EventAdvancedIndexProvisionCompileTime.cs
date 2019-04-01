///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityQuery;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.service
{
    public class EventAdvancedIndexProvisionCompileTime
    {
        public EventAdvancedIndexProvisionCompileTime(
            AdvancedIndexDescWExpr indexDesc, ExprNode[] parameters, EventAdvancedIndexFactoryForge factory,
            EventAdvancedIndexConfigStatementForge configStatement)
        {
            IndexDesc = indexDesc;
            Parameters = parameters;
            Factory = factory;
            ConfigStatement = configStatement;
        }

        public AdvancedIndexDescWExpr IndexDesc { get; }

        public ExprNode[] Parameters { get; }

        public EventAdvancedIndexFactoryForge Factory { get; }

        public EventAdvancedIndexConfigStatementForge ConfigStatement { get; }

        public CodegenExpression CodegenMake(CodegenMethodScope parent, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(EventAdvancedIndexProvisionRuntime), typeof(EventAdvancedIndexProvisionCompileTime), classScope);
            var indexExpressions =
                ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceAsArray(IndexDesc.IndexedExpressions);
            var indexProperties = GetPropertiesPerExpressionExpectSingle(IndexDesc.IndexedExpressions);
            method.Block
                .DeclareVar(
                    typeof(EventAdvancedIndexProvisionRuntime), "desc",
                    NewInstance(typeof(EventAdvancedIndexProvisionRuntime)))
                .ExprDotMethod(Ref("desc"), "setIndexExpressionTexts", Constant(indexExpressions))
                .ExprDotMethod(Ref("desc"), "setIndexProperties", Constant(indexProperties))
                .ExprDotMethod(
                    Ref("desc"), "setIndexExpressionsAllProps",
                    Constant(IsExpressionsAllPropsOnly(IndexDesc.IndexedExpressions)))
                .ExprDotMethod(Ref("desc"), "setFactory", Factory.CodegenMake(parent, classScope))
                .ExprDotMethod(
                    Ref("desc"), "setParameterExpressionTexts",
                    Constant(ExprNodeUtilityPrint.ToExpressionStringsMinPrecedence(Parameters)))
                .ExprDotMethod(
                    Ref("desc"), "setParameterEvaluators",
                    ExprNodeUtilityCodegen.CodegenEvaluators(Parameters, parent, GetType(), classScope))
                .ExprDotMethod(Ref("desc"), "setConfigStatement", ConfigStatement.CodegenMake(parent, classScope))
                .ExprDotMethod(Ref("desc"), "setIndexTypeName", Constant(IndexDesc.IndexTypeName))
                .MethodReturn(Ref("desc"));
            return LocalMethod(method);
        }

        public EventAdvancedIndexProvisionRuntime ToRuntime()
        {
            var runtime = new EventAdvancedIndexProvisionRuntime();
            runtime.IndexExpressionTexts = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceAsArray(IndexDesc.IndexedExpressions);
            runtime.IndexProperties = GetPropertiesPerExpressionExpectSingle(IndexDesc.IndexedExpressions);
            runtime.IndexExpressionsOpt = IndexDesc.IndexedExpressions;
            runtime.IsIndexExpressionsAllProps = IsExpressionsAllPropsOnly(IndexDesc.IndexedExpressions);
            runtime.Factory = Factory.RuntimeFactory;
            runtime.ParameterExpressionTexts = ExprNodeUtilityPrint.ToExpressionStringsMinPrecedence(Parameters);
            runtime.ParameterEvaluators = GetEvaluatorsNoCompile(Parameters);
            runtime.ParameterExpressionsOpt = Parameters;
            runtime.ConfigStatement = ConfigStatement.ToRuntime();
            runtime.IndexTypeName = IndexDesc.IndexTypeName;
            return runtime;
        }
    }
} // end of namespace