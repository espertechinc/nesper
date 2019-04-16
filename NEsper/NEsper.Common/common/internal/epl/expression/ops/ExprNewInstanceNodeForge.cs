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
using com.espertech.esper.common.@internal.@event.bean.manufacturer;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprNewInstanceNodeForge : ExprForge
    {
        private readonly InstanceManufacturerFactory manufacturerFactory;

        private readonly ExprNewInstanceNode parent;

        public ExprNewInstanceNodeForge(
            ExprNewInstanceNode parent,
            Type targetClass,
            InstanceManufacturerFactory manufacturerFactory)
        {
            this.parent = parent;
            EvaluationType = targetClass;
            this.manufacturerFactory = manufacturerFactory;
        }

        public ExprEvaluator ExprEvaluator =>
            new ExprNewInstanceNodeForgeEval(this, manufacturerFactory.MakeEvaluator());

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return manufacturerFactory.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public Type EvaluationType { get; }

        public ExprNodeRenderable ForgeRenderable => parent;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace