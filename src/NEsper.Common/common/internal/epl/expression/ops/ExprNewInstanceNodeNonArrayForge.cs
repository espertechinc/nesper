///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class ExprNewInstanceNodeNonArrayForge : ExprForge
    {
        private readonly InstanceManufacturerFactory _manufacturerFactory;

        private readonly ExprNewInstanceNode _parent;

        public ExprNewInstanceNodeNonArrayForge(
            ExprNewInstanceNode parent,
            Type targetClass,
            InstanceManufacturerFactory manufacturerFactory)
        {
            _parent = parent;
            EvaluationType = targetClass;
            _manufacturerFactory = manufacturerFactory;
        }

        public ExprNodeRenderable ExprForgeRenderable => _parent;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _manufacturerFactory.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public ExprEvaluator ExprEvaluator => new ExprNewInstanceNodeNonArrayForgeEval(_manufacturerFactory.MakeEvaluator());

        public Type EvaluationType { get; }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace