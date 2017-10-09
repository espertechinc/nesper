///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.SelectExprRep
{
    public class SelectExprProcessorEvalAvroMapToAvro : ExprEvaluator
    {
        private readonly ExprEvaluator _eval;
        private readonly RecordSchema _inner;

        public SelectExprProcessorEvalAvroMapToAvro(ExprEvaluator eval, Schema schema, string columnName)
        {
            _eval = eval;
            _inner = schema.GetField(columnName).Schema.AsRecordSchema();
            if (_inner.Tag != Schema.Type.Record)
            {
                throw new IllegalStateException("Column '" + columnName + "' is not a record but schema " + _inner);
            }
        }

        public Type ReturnType
        {
            get { return typeof (GenericRecord); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var map = (IDictionary<string, Object>) _eval.Evaluate(evaluateParams);
            if (map == null)
            {
                return null;
            }
            var record = new GenericRecord(_inner);
            foreach (var row in map)
            {
                record.Put(row.Key, row.Value);
            }
            return record;
        }
    }
} // end of namespace