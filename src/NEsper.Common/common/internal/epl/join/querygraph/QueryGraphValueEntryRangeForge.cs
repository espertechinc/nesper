///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    [Serializable]
    public abstract class QueryGraphValueEntryRangeForge : QueryGraphValueEntryForge
    {
        internal readonly QueryGraphRangeEnum type;

        protected QueryGraphValueEntryRangeForge(QueryGraphRangeEnum type)
        {
            this.type = type;
        }

        public QueryGraphRangeEnum Type => type;


        public abstract ExprNode[] Expressions { get; }

        protected abstract Type ResultType { get; }

        public abstract CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public abstract string ToQueryPlan();

        public abstract CodegenExpression Make(
            Type optionalCoercionType,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public static string ToQueryPlan(IList<QueryGraphValueEntryRangeForge> rangeKeyPairs)
        {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (var item in rangeKeyPairs) {
                writer.Write(delimiter);
                writer.Write(item.ToQueryPlan());
                delimiter = ", ";
            }

            return writer.ToString();
        }

        public static CodegenExpression MakeArray(
            QueryGraphValueEntryRangeForge[] ranges,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var expressions = new CodegenExpression[ranges.Length];
            for (var i = 0; i < ranges.Length; i++) {
                expressions[i] = ranges[i].Make(method, symbols, classScope);
            }

            return NewArrayWithInit(typeof(QueryGraphValueEntryRange), expressions);
        }

        public static Type[] GetRangeResultTypes(QueryGraphValueEntryRangeForge[] ranges)
        {
            var types = new Type[ranges.Length];
            for (var i = 0; i < ranges.Length; i++) {
                types[i] = ranges[i].ResultType;
            }

            return types;
        }
    }
} // end of namespace