///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.serde;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.writer
{
    public class JsonEventBeanPropertyWriter : EventPropertyWriterSPI
    {
        private readonly JsonSerializationContext SerializationContext;
        /// <summary>
        /// The field being assigned.
        /// </summary>
        private readonly JsonUnderlyingField _field;

        public JsonEventBeanPropertyWriter(
            JsonSerializationContext serializationContext,
            JsonUnderlyingField field)
        {
            this.SerializationContext = serializationContext;
            this._field = field;
        }

        /// <summary>
        /// The field being assigned.
        /// </summary>
        public JsonUnderlyingField Field => _field;

        /// <summary>
        /// Assigns (sets) the named property in the target (underlying)
        /// to the provided value.
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
        /// Assigns (sets) the named property in the underlying to the
        /// provided value.
        /// </summary>
        /// <param name="value">value to be set</param>
        /// <param name="und">underlying event (json) where the event lives</param>
        public virtual void Write(
            object value,
            object und)
        {
            SerializationContext.SetValue(_field.FieldName, value, und);
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