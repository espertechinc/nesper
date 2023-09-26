///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.xml;

namespace com.espertech.esper.common.@internal.@event.property
{
    /// <summary>
    ///     All properties have a property name and this is the abstract base class that serves up the property name.
    /// </summary>
    public abstract class PropertyBase : Property
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">is the name of the property</param>
        protected PropertyBase(string propertyName)
        {
            PropertyNameAtomic = PropertyParser.UnescapeBacktickForProperty(propertyName);
        }

        /// <summary>
        ///     Returns the atomic property name, which is a part of all of the full (complex) property name.
        /// </summary>
        /// <returns>atomic name of property</returns>
        public string PropertyNameAtomic { get; internal set; }

        public virtual bool IsDynamic => false;

        public abstract EventPropertyGetterSPI GetterDOM { get; }

        public abstract Type GetPropertyType(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory);

        public abstract EventPropertyGetterSPI GetGetter(
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory);

        public abstract Type GetPropertyTypeMap(
            IDictionary<string, object> optionalMapPropTypes,
            BeanEventTypeFactory beanEventTypeFactory);

        public abstract MapEventPropertyGetter GetGetterMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory);

        public abstract ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty,
            IDictionary<string, object> nestableTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory);

        public abstract SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty);

        public abstract EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType xmlEventType,
            string propertyExpression);

        public abstract void ToPropertyEPL(TextWriter writer);
        public abstract string[] ToPropertyArray();
    }
} // end of namespace