///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeNRSymbol : ExprSubselectEvalMatchSymbol
    {
        public const string NAME_LEFTRESULT = "leftResult";

        public static readonly CodegenExpressionRef REF_LEFTRESULT = Ref(NAME_LEFTRESULT);

        private CodegenExpressionRef optionalLeftResult;

        public SubselectForgeNRSymbol(Type leftResultType)
        {
            LeftResultType = leftResultType;
        }

        public Type LeftResultType { get; }

        public CodegenExpressionRef GetAddLeftResult(CodegenMethodScope scope)
        {
            if (optionalLeftResult == null) {
                optionalLeftResult = REF_LEFTRESULT;
            }

            scope.AddSymbol(optionalLeftResult);
            return optionalLeftResult;
        }

        public override void Provide(IDictionary<string, Type> symbols)
        {
            if (optionalLeftResult != null) {
                symbols.Put(optionalLeftResult.Ref, LeftResultType);
            }

            base.Provide(symbols);
        }
    }
} // end of namespace