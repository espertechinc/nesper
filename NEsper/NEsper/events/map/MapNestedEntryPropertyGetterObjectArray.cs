///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.blocks;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events.arr;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    /// <summary>
    ///     A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class MapNestedEntryPropertyGetterObjectArray : MapNestedEntryPropertyGetterBase
    {
        private readonly ObjectArrayEventPropertyGetter _arrayGetter;

        public MapNestedEntryPropertyGetterObjectArray(string propertyMap, EventType fragmentType,
            EventAdapterService eventAdapterService, ObjectArrayEventPropertyGetter arrayGetter)
            : base(propertyMap, fragmentType, eventAdapterService)
        {
            this._arrayGetter = arrayGetter;
        }

        public override object HandleNestedValue(object value)
        {
            if (!(value is object[]))
            {
                if (value is EventBean) return _arrayGetter.Get((EventBean) value);
                return null;
            }

            return _arrayGetter.GetObjectArray((object[]) value);
        }

        public override object HandleNestedValueFragment(object value)
        {
            if (!(value is object[]))
            {
                if (value is EventBean) return _arrayGetter.GetFragment((EventBean) value);
                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            var eventBean = EventAdapterService.AdapterForTypedObjectArray((object[]) value, FragmentType);
            return _arrayGetter.GetFragment(eventBean);
        }

        public override ICodegenExpression HandleNestedValueCodegen(ICodegenExpression name, ICodegenContext context)
        {
            string method = CodegenBlockPropertyBeanOrUnd.From(context, typeof(object[]), _arrayGetter,
                CodegenBlockPropertyBeanOrUnd.AccessType.GET, GetType());
            return LocalMethod(method, name);
        }

        public override ICodegenExpression HandleNestedValueFragmentCodegen(ICodegenExpression name,
            ICodegenContext context)
        {
            string method = CodegenBlockPropertyBeanOrUnd.From(context, typeof(object[]), _arrayGetter,
                CodegenBlockPropertyBeanOrUnd.AccessType.FRAGMENT, GetType());
            return LocalMethod(method, name);
        }
    }
} // end of namespace