///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // cast

namespace com.espertech.esper.common.@internal.@event.json.deserializers.forge
{
    public class JsonDeserializerForgeCast : JsonDeserializerForge
    {
        private readonly Type _target;
        private readonly string _targetClassName;

        public JsonDeserializerForgeCast(Type target)
        {
            this._target = target;
            _targetClassName = null;
        }

        public JsonDeserializerForgeCast(string targetClassName)
        {
            _targetClassName = targetClassName;
            _target = null;
        }

        public CodegenExpression CodegenDeserialize(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression elementExpr)
        {
            throw new NotImplementedException();
        }

        public CodegenExpression CaptureValue(
            JsonEndValueRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (_target != null) {
                return Cast(_target, refs.ValueObject);
            }

            return Cast(_targetClassName, refs.ValueObject);
        }
    }
} // end of namespace