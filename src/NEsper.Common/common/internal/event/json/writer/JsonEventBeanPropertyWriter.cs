///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.writer
{
    public class JsonEventBeanPropertyWriter : EventPropertyWriterSPI
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IJsonDelegate _delegate;

        /// <summary>
        /// The field being assigned.
        /// </summary>
        private readonly JsonUnderlyingField _field;

        public JsonEventBeanPropertyWriter(
            IJsonDelegate @delegate,
            JsonUnderlyingField field)
        {
            _delegate = @delegate;
            _field = field;
        }

        public IJsonDelegate Delegate => _delegate;

        public JsonUnderlyingField Field1 => _field;

        /// <summary>
        /// The field being assigned.
        /// </summary>
        public JsonUnderlyingField Field => _field;

        /// <summary>
        /// Writes the named property the underlying value to the serialization target.
        /// </summary>
        /// <param name="value">value to be set</param>
        /// <param name="target"></param>
        public void Write(
            object value,
            EventBean target)
        {
            Write(value, target.Underlying);
        }

        /// <summary>
        /// Writes the named property the underlying value to the serialization target.
        /// </summary>
        /// <param name="value">value to be set</param>
        /// <param name="und">underlying event (json) is being written to (target)</param>
        public virtual void Write(
            object value,
            object und)
        {
            if (!_delegate.TrySetProperty(_field.PropertyNumber, value, und)) {
                Log.Warn($"Attempted to write property \"{_field.PropertyNumber}\" failed");
            }
        }

        /// <summary>
        ///     Writes a single property value to a target event.
        /// </summary>
        public virtual CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression underlying,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return Assign(ExprDotName(underlying, _field.FieldName), assigned);
        }
    }
} // end of namespace