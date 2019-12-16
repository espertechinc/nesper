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
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class ExprSubselectEvalMatchSymbol : ExprForgeCodegenSymbol
    {
        public const string NAME_MATCHINGEVENTS = "matchingEvents";

        public static readonly CodegenExpressionRef REF_MATCHINGEVENTS = Ref(NAME_MATCHINGEVENTS);

        private CodegenExpressionRef optionalMatchingEventRef;

        public ExprSubselectEvalMatchSymbol()
            : base(false, null)
        {
        }

        public CodegenExpressionRef GetAddMatchingEvents(CodegenMethodScope scope)
        {
            if (optionalMatchingEventRef == null) {
                optionalMatchingEventRef = REF_MATCHINGEVENTS;
            }

            scope.AddSymbol(optionalMatchingEventRef);
            return optionalMatchingEventRef;
        }

        public override void Provide(IDictionary<string, Type> symbols)
        {
            if (optionalMatchingEventRef != null) {
                symbols.Put(optionalMatchingEventRef.Ref, typeof(FlexCollection));
            }

            base.Provide(symbols);
        }
    }
} // end of namespace