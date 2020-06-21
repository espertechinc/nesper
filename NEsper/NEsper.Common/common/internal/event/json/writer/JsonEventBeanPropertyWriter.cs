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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.writer
{
    public class JsonEventBeanPropertyWriter : EventPropertyWriterSPI
    {
        protected readonly JsonDelegateFactory delegateFactory;
        protected readonly JsonUnderlyingField field;

        public JsonEventBeanPropertyWriter(
            JsonDelegateFactory delegateFactory,
            JsonUnderlyingField field)
        {
            this.delegateFactory = delegateFactory;
            this.field = field;
        }

        public void Write(
            object value,
            EventBean target)
        {
            Write(value, target.Underlying);
        }

        public virtual CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression und,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return Assign(ExprDotName(und, field.FieldName), assigned);
        }

        public virtual void Write(
            object value,
            object und)
        {
            delegateFactory.SetValue(field.PropertyNumber, value, und);
        }
    }
} // end of namespace