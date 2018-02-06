///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.blocks;
using com.espertech.esper.codegen.model.expression;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayNestedEntryPropertyGetterObjectArray : ObjectArrayNestedEntryPropertyGetterBase
    {
        private readonly ObjectArrayEventPropertyGetter _arrayGetter;

        public ObjectArrayNestedEntryPropertyGetterObjectArray(int propertyIndex, EventType fragmentType, EventAdapterService eventAdapterService, ObjectArrayEventPropertyGetter arrayGetter)
            : base(propertyIndex, fragmentType, eventAdapterService)
        {
            _arrayGetter = arrayGetter;
        }

        public override Object HandleNestedValue(Object value)
        {
            if (!(value is Object[]))
            {
                if (value is EventBean)
                {
                    return _arrayGetter.Get((EventBean)value);
                }
                return null;
            }
            return _arrayGetter.GetObjectArray((Object[])value);
        }

        public override Object HandleNestedValueFragment(Object value)
        {
            if (!(value is Object[]))
            {
                if (value is EventBean)
                {
                    return _arrayGetter.GetFragment((EventBean)value);
                }
                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            var eventBean = EventAdapterService.AdapterForTypedObjectArray((Object[])value, FragmentType);
            return _arrayGetter.GetFragment(eventBean);
        }

        public override bool HandleNestedValueExists(Object value)
        {
            if (!(value is Object[]))
            {
                if (value is EventBean)
                {
                    return _arrayGetter.IsExistsProperty((EventBean)value);
                }
                return false;
            }
            return _arrayGetter.IsObjectArrayExistsProperty((Object[])value);
        }

        public override ICodegenExpression HandleNestedValueCodegen(ICodegenExpression name, ICodegenContext context)
        {
            return LocalMethod(GenerateMethod(context, CodegenBlockPropertyBeanOrUnd.AccessType.GET), name);
        }

        public override ICodegenExpression HandleNestedValueExistsCodegen(ICodegenExpression refName, ICodegenContext context)
        {
            return LocalMethod(GenerateMethod(context, CodegenBlockPropertyBeanOrUnd.AccessType.EXISTS), refName);
        }

        public override ICodegenExpression HandleNestedValueFragmentCodegen(ICodegenExpression refName, ICodegenContext context)
        {
            return LocalMethod(GenerateMethod(context, CodegenBlockPropertyBeanOrUnd.AccessType.FRAGMENT), refName);
        }

        private string GenerateMethod(ICodegenContext context, CodegenBlockPropertyBeanOrUnd.AccessType accessType)
        {
            return CodegenBlockPropertyBeanOrUnd.From(context, typeof(Object[]), _arrayGetter, accessType, this.GetType());
        }
    }
} // end of namespace