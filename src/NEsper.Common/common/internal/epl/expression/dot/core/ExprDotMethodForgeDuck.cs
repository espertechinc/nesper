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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotMethodForgeDuck : ExprDotForge
    {
        public ExprDotMethodForgeDuck(
            string statementName,
            ImportService importService,
            string methodName,
            Type[] parameterTypes,
            ExprForge[] parameters)
        {
            StatementName = statementName;
            ImportService = importService;
            MethodName = methodName;
            ParameterTypes = parameterTypes;
            Parameters = parameters;
        }

        public string StatementName { get; }

        public ImportService ImportService { get; }

        public string MethodName { get; }

        public Type[] ParameterTypes { get; }

        public ExprForge[] Parameters { get; }

        public EPType TypeInfo => EPTypeHelper.SingleValue(typeof(object));

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitMethod(MethodName);
        }

        public ExprDotEval DotEvaluator => new ExprDotMethodForgeDuckEval(
            this,
            ExprNodeUtilityQuery.GetEvaluatorsNoCompile(Parameters));

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return ExprDotMethodForgeDuckEval.Codegen(
                this,
                inner,
                innerType,
                parent,
                symbols,
                classScope);
        }
    }
} // end of namespace