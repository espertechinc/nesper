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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public class ExprTableEvalStrategyUtil
    {
        public static AggregationRow GetRow(ObjectArrayBackedEventBean eventBean)
        {
            return (AggregationRow) eventBean.Properties[0];
        }

        public static CodegenExpression CodegenInitMap(
            IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccesses,
            Type generator,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            // int, ExprTableEvalStrategyFactory
            CodegenMethod method = parent.MakeChild(typeof(IDictionary<int, ExprTableEvalStrategyFactory>), generator, classScope);
            method.Block
                .DeclareVar<IDictionary<int, ExprTableEvalStrategyFactory>>(
                    "ta", NewInstance(typeof(LinkedHashMap<int, ExprTableEvalStrategyFactory>)));
            foreach (KeyValuePair<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> entry in tableAccesses) {
                method.Block.AssignArrayElement(
                    Ref("ta"),
                    Constant(entry.Key.TableAccessNumber),
                    entry.Value.Make(method, symbols, classScope));

                //method.Block.ExprDotMethod(
                //    @Ref("ta"),
                //    "Put",
                //    Constant(entry.Key.TableAccessNumber),
                //    entry.Value.Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("ta"));
            return LocalMethod(method);
        }

        protected internal static IDictionary<string, object> EvalMap(
            ObjectArrayBackedEventBean @event,
            AggregationRow row,
            IDictionary<string, TableMetadataColumn> items,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            Dictionary<string, object> cols = new Dictionary<string, object>();
            foreach (KeyValuePair<string, TableMetadataColumn> entry in items) {
                if (entry.Value is TableMetadataColumnPlain) {
                    TableMetadataColumnPlain plain = (TableMetadataColumnPlain) entry.Value;
                    cols.Put(entry.Key, @event.Properties[plain.IndexPlain]);
                }
                else {
                    TableMetadataColumnAggregation aggcol = (TableMetadataColumnAggregation) entry.Value;
                    cols.Put(entry.Key, row.GetValue(aggcol.Column, eventsPerStream, isNewData, exprEvaluatorContext));
                }
            }

            return cols;
        }

        protected internal static object[] EvalTypable(
            ObjectArrayBackedEventBean @event,
            AggregationRow row,
            IDictionary<string, TableMetadataColumn> items,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            object[] values = new object[items.Count];
            int count = 0;
            foreach (KeyValuePair<string, TableMetadataColumn> entry in items) {
                if (entry.Value is TableMetadataColumnPlain) {
                    TableMetadataColumnPlain plain = (TableMetadataColumnPlain) entry.Value;
                    values[count] = @event.Properties[plain.IndexPlain];
                }
                else {
                    TableMetadataColumnAggregation aggcol = (TableMetadataColumnAggregation) entry.Value;
                    values[count] = row.GetValue(aggcol.Column, eventsPerStream, isNewData, exprEvaluatorContext);
                }

                count++;
            }

            return values;
        }
    }
} // end of namespace