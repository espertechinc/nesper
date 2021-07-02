///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public abstract class EnumAggregateForge : EnumForge
    {
        private readonly ExprForge _initialization;
        private readonly ExprForge _innerExpression;
        private readonly ObjectArrayEventType _resultEventType;
        private readonly int _streamNumLambda;

        protected EnumAggregateForge(
            ExprForge initialization,
            ExprForge innerExpression,
            int streamNumLambda,
            ObjectArrayEventType resultEventType)
        {
            _initialization = initialization;
            _innerExpression = innerExpression;
            _streamNumLambda = streamNumLambda;
            _resultEventType = resultEventType;
        }

        public int StreamNumLambda => _streamNumLambda;

        public ObjectArrayEventType ResultEventType => _resultEventType;

        public int StreamNumSize => _streamNumLambda + 2;

        public ExprForge Initialization => _initialization;

        public ExprForge InnerExpression => _innerExpression;

        public abstract EnumEval EnumEvaluator { get; }

        public abstract CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace