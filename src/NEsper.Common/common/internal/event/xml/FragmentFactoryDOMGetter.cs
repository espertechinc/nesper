///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Factory for fragments for DOM getters.
    /// </summary>
    public class FragmentFactoryDOMGetter : FragmentFactorySPI
    {
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly string _propertyExpression;
        private readonly BaseXMLEventType _xmlEventType;
        private volatile EventType _fragmentType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventBeanTypedEventFactory">for event type lookup</param>
        /// <param name="xmlEventType">the originating type</param>
        /// <param name="propertyExpression">property expression</param>
        public FragmentFactoryDOMGetter(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType xmlEventType,
            string propertyExpression)
        {
            this._eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this._xmlEventType = xmlEventType;
            this._propertyExpression = propertyExpression;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var xmlType = Cast(
                typeof(BaseXMLEventType),
                EventTypeUtility.ResolveTypeCodegen(_xmlEventType, EPStatementInitServicesConstants.REF));
            return NewInstance<FragmentFactoryDOMGetter>(factory, xmlType, Constant(_propertyExpression));
        }

        public EventBean GetEvent(XmlNode result)
        {
            if (_fragmentType == null) {
                var type = _xmlEventType.GetFragmentType(_propertyExpression);
                if (type == null) {
                    return null;
                }

                _fragmentType = type.FragmentType;
            }

            return _eventBeanTypedEventFactory.AdapterForTypedDOM(result, _fragmentType);
        }
    }
} // end of namespace