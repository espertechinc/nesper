///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.getter.provided;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
    public class JsonUnderlyingField
    {
        public JsonUnderlyingField(
            string fieldName,
            int propertyNumber,
            Type propertyType,
            FieldInfo optionalField)
        {
            FieldName = fieldName;
            PropertyNumber = propertyNumber;
            PropertyType = propertyType;
            OptionalField = optionalField;
        }

        public string FieldName { get; }

        public int PropertyNumber { get; }

        public Type PropertyType { get; }

        public FieldInfo OptionalField { get; }

        public CodegenExpression ToCodegenExpression()
        {
            var field = ConstantNull();
            if (OptionalField != null) {
                field = StaticMethod(
                    typeof(JsonFieldResolverProvided),
                    "ResolveJsonField",
                    Constant(OptionalField.DeclaringType),
                    Constant(OptionalField.Name));
            }

            return NewInstance<JsonUnderlyingField>(
                Constant(FieldName),
                Constant(PropertyNumber),
                Constant(PropertyType),
                field);
        }
    }
} // end of namespace