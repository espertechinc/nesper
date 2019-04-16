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
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly string propertyExpression;
        private readonly BaseXMLEventType xmlEventType;
        private volatile EventType fragmentType;

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
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.xmlEventType = xmlEventType;
            this.propertyExpression = propertyExpression;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var xmlType = Cast(
                typeof(BaseXMLEventType),
                EventTypeUtility.ResolveTypeCodegen(xmlEventType, EPStatementInitServicesConstants.REF));
            return NewInstance(typeof(FragmentFactoryDOMGetter), factory, xmlType, Constant(propertyExpression));
        }

        public EventBean GetEvent(XmlNode result)
        {
            if (fragmentType == null) {
                var type = xmlEventType.GetFragmentType(propertyExpression);
                if (type == null) {
                    return null;
                }

                fragmentType = type.FragmentType;
            }

            return eventBeanTypedEventFactory.AdapterForTypedDOM(result, fragmentType);
        }
    }
} // end of namespace