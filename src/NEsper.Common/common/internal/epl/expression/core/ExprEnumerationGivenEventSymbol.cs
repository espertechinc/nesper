///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprEnumerationGivenEventSymbol : CodegenSymbolProvider
    {
        private CodegenExpressionRef _optionalEventRef;
        private CodegenExpressionRef _optionalExprEvalCtxRef;

        public void Provide(IDictionary<string, Type> symbols)
        {
            if (_optionalExprEvalCtxRef != null) {
                symbols.Put(_optionalExprEvalCtxRef.Ref, typeof(ExprEvaluatorContext));
            }

            if (_optionalEventRef != null) {
                symbols.Put(_optionalEventRef.Ref, typeof(EventBean));
            }
        }

        public CodegenExpressionRef GetAddExprEvalCtx(CodegenMethodScope scope)
        {
            if (_optionalExprEvalCtxRef == null) {
                _optionalExprEvalCtxRef = ExprForgeCodegenNames.REF_EXPREVALCONTEXT;
            }

            scope.AddSymbol(_optionalExprEvalCtxRef);
            return _optionalExprEvalCtxRef;
        }

        public CodegenExpressionRef GetAddEvent(CodegenMethodScope scope)
        {
            if (_optionalEventRef == null) {
                _optionalEventRef = Ref("@event");
            }

            scope.AddSymbol(_optionalEventRef);
            return _optionalEventRef;
        }
    }
} // end of namespace