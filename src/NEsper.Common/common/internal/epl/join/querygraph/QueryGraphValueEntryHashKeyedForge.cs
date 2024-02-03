///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    public abstract class QueryGraphValueEntryHashKeyedForge : QueryGraphValueEntryForge
    {
        public QueryGraphValueEntryHashKeyedForge(ExprNode keyExpr)
        {
            KeyExpr = keyExpr;
        }

        public ExprNode KeyExpr { get; }

        public abstract CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public abstract string ToQueryPlan();

        public static string ToQueryPlan(IList<QueryGraphValueEntryHashKeyedForge> keyProperties)
        {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (var item in keyProperties) {
                writer.Write(delimiter);
                writer.Write(item.ToQueryPlan());
                delimiter = ", ";
            }

            return writer.ToString();
        }

        public static EventPropertyGetterSPI[] GetGettersIfPropsOnly(QueryGraphValueEntryHashKeyedForge[] keys)
        {
            if (keys == null) {
                return null;
            }

            var getterSPIS = new EventPropertyGetterSPI[keys.Length];
            for (var i = 0; i < keys.Length; i++) {
                if (!(keys[i] is QueryGraphValueEntryHashKeyedForgeProp)) {
                    return null;
                }

                getterSPIS[i] = ((QueryGraphValueEntryHashKeyedForgeProp)keys[i]).EventPropertyGetter;
            }

            return getterSPIS;
        }

        public static ExprForge[] GetForges(QueryGraphValueEntryHashKeyedForge[] keys)
        {
            if (keys == null) {
                return null;
            }

            var forges = new ExprForge[keys.Length];
            for (var i = 0; i < keys.Length; i++) {
                forges[i] = keys[i].KeyExpr.Forge;
            }

            return forges;
        }
    }
} // end of namespace