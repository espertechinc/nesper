///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    public class PropertyDotNonLambdaIndexedForge : ExprForge,
        ExprNodeRenderable
    {
        public PropertyDotNonLambdaIndexedForge(
            int streamId,
            EventPropertyGetterIndexedSPI indexedGetter,
            ExprForge paramForge,
            Type returnType)
        {
            StreamId = streamId;
            IndexedGetter = indexedGetter;
            ParamForge = paramForge;
            EvaluationType = returnType;
        }

        public int StreamId { get; }

        public EventPropertyGetterIndexedSPI IndexedGetter { get; }

        public ExprForge ParamForge { get; }

        public ExprEvaluator ExprEvaluator => new PropertyDotNonLambdaIndexedForgeEval(this, ParamForge.ExprEvaluator);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return PropertyDotNonLambdaIndexedForgeEval.Codegen(
                this,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public Type EvaluationType { get; }

        public ExprNodeRenderable ExprForgeRenderable => this;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace