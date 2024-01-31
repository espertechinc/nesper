///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class PropertyDotNonLambdaMappedForge : ExprForge,
        ExprNodeRenderable
    {
        public PropertyDotNonLambdaMappedForge(
            int streamId,
            EventPropertyGetterMappedSPI mappedGetter,
            ExprForge paramForge,
            Type returnType)
        {
            StreamId = streamId;
            MappedGetter = mappedGetter;
            ParamForge = paramForge;
            EvaluationType = returnType;
        }

        public int StreamId { get; }

        public EventPropertyGetterMappedSPI MappedGetter { get; }

        public ExprForge ParamForge { get; }

        public ExprEvaluator ExprEvaluator => new PropertyDotNonLambdaMappedForgeEval(this, ParamForge.ExprEvaluator);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return PropertyDotNonLambdaMappedForgeEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public Type EvaluationType { get; }

        public ExprNodeRenderable ExprForgeRenderable => this;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace