///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace NEsper.Avro.Core
{
    public class AvroSchemaFieldSharable : CodegenFieldSharable
    {
        private readonly Schema _schema;

        public AvroSchemaFieldSharable(Schema schema)
        {
            _schema = schema;
        }

        public Type Type()
        {
            return typeof(Schema);
        }

        public CodegenExpression InitCtorScoped()
        {
            return CodegenExpressionBuilder.StaticMethod<Schema>(
                "Parse",
                CodegenExpressionBuilder.Constant(_schema.ToString()));
        }
    }
} // end of namespace