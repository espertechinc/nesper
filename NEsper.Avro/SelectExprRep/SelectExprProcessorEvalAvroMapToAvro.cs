///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.SelectExprRep
{
    public class SelectExprProcessorEvalAvroMapToAvro : ExprEvaluator,
        ExprForge,
        ExprNodeRenderable
    {
        private readonly ExprForge _forge;
        private readonly Schema _inner;
        private ExprEvaluator _eval;

        public SelectExprProcessorEvalAvroMapToAvro(
            ExprForge forge,
            Schema schema,
            string columnName)
        {
            _forge = forge;
            _inner = schema.GetField(columnName).Schema;
            if (_inner.Tag != Schema.Type.Record) {
                throw new IllegalStateException("Column '" + columnName + "' is not a record but schema " + _inner);
            }
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            IDictionary<string, object> map = (IDictionary<string, object>) _eval.Evaluate(
                eventsPerStream,
                isNewData,
                context);
            return SelectExprProcessAvroMap(map, _inner);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField schema =
                codegenClassScope.AddOrGetFieldSharable(new AvroSchemaFieldSharable(_inner));
            return CodegenExpressionBuilder.StaticMethod(
                typeof(SelectExprProcessorEvalAvroMapToAvro),
                "selectExprProcessAvroMap",
                _forge.EvaluateCodegen(requiredType, codegenMethodScope, exprSymbol, codegenClassScope),
                schema);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="map">map</param>
        /// <param name="inner">inner</param>
        /// <returns>record</returns>
        public static object SelectExprProcessAvroMap(
            IDictionary<string, object> map,
            Schema inner)
        {
            if (map == null) {
                return null;
            }

            var record = new GenericRecord(inner.AsRecordSchema());
            foreach (KeyValuePair<string, object> row in map) {
                record.Put(row.Key, row.Value);
            }

            return record;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                if (_eval == null) {
                    _eval = _forge.ExprEvaluator;
                }

                return this;
            }
        }

        public Type EvaluationType {
            get => typeof(GenericRecord);
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public ExprNodeRenderable ForgeRenderable {
            get => this;
        }

        public ExprNodeRenderable ExprForgeRenderable => ForgeRenderable;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace