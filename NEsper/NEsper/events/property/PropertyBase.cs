///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.xml;

namespace com.espertech.esper.events.property
{
	/// <summary>
    /// All properties have a property name and this is the abstract base class
    /// that serves up the property name.
    /// </summary>
	
    public abstract class PropertyBase : Property
	{
	    /// <summary> Returns the property name.</summary>
	    /// <returns> name of property
	    /// </returns>
	    public string PropertyNameAtomic { get; internal set; }

	    /// <summary> Ctor.</summary>
		/// <param name="propertyName">is the name of the property
		/// </param>
        protected PropertyBase(String propertyName)
		{
            PropertyNameAtomic = PropertyParser.UnescapeBacktick(propertyName);
		}

	    /// <summary>
	    /// Returns the property type.
	    /// </summary>
        /// <param name="eventType">is the event type representing the object</param>
	    /// <param name="eventAdapterService">for event adapters</param>
	    /// <returns>
	    /// property type class
	    /// </returns>
	    public abstract Type GetPropertyType(BeanEventType eventType, EventAdapterService eventAdapterService);

	    /// <summary>
	    /// Returns the property type plus its generic type parameter, if any.
	    /// </summary>
        /// <param name="eventType">is the event type representing the object</param>
	    /// <param name="eventAdapterService">for event adapters</param>
	    /// <returns>
	    /// type and generic descriptor
	    /// </returns>
	    public abstract GenericPropertyDesc GetPropertyTypeGeneric(BeanEventType eventType, EventAdapterService eventAdapterService);

	    /// <summary>
	    /// Returns value getter for the property of an event of the given event type.
	    /// </summary>
	    /// <param name="eventType">is the type of event to make a getter for</param>
	    /// <param name="eventAdapterService">factory for event beans and event types</param>
	    /// <returns>
	    /// fast property value getter for property
	    /// </returns>
        public abstract EventPropertyGetterSPI GetGetter(BeanEventType eventType, EventAdapterService eventAdapterService);

	    /// <summary>
	    /// Returns the property type for use with Map event representations.
	    /// </summary>
	    /// <param name="optionalMapPropTypes">a map-within-map type definition, if supplied, or null if not supplied</param>
	    /// <param name="eventAdapterService">for resolving further map event types that are property types</param>
	    /// <returns>
	    /// property type @param optionalMapPropTypes
	    /// </returns>
	    public abstract Type GetPropertyTypeMap(IDictionary<string, object> optionalMapPropTypes, EventAdapterService eventAdapterService);

	    /// <summary>
	    /// Returns the getter-method for use with Map event representations.
	    /// </summary>
	    /// <param name="optionalMapPropTypes">a map-within-map type definition, if supplied, or null if not supplied</param>
	    /// <param name="eventAdapterService">for resolving further map event types that are property types</param>
	    /// <returns>
	    /// getter for maps
	    /// </returns>
	    public abstract MapEventPropertyGetter GetGetterMap(IDictionary<string, object> optionalMapPropTypes, EventAdapterService eventAdapterService);

	    /// <summary>
	    /// Returns the property type for use with DOM event representations.
	    /// </summary>
	    /// <param name="complexProperty">a element-within-element type definition</param>
	    /// <param name="eventAdapterService">for resolving further element event types if defined</param>
	    /// <returns>
	    /// property type
	    /// </returns>
	    public abstract SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService);

	    /// <summary>
	    /// Returns the getter-method for use with XML DOM event representations.
	    /// </summary>
	    /// <param name="complexProperty">a element-within-element type definition</param>
	    /// <param name="eventAdapterService">for resolving or creating further event types that are property types</param>
	    /// <param name="xmlEventType">the event type</param>
	    /// <param name="propertyExpression">the full property expression</param>
	    /// <returns>
	    /// getter
	    /// </returns>
	    public abstract EventPropertyGetterSPI GetGetterDOM(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService, BaseXMLEventType xmlEventType, string propertyExpression);

	    /// <summary>
	    /// Returns the getter-method for use with XML DOM event representations.
	    /// </summary>
	    /// <returns>
	    /// getter
	    /// </returns>
	    public abstract EventPropertyGetterSPI GetGetterDOM();

        /// <summary>
        /// Gets the getter object array.
        /// </summary>
        /// <param name="indexPerProperty">The index per property.</param>
        /// <param name="nestableTypes">The nestable types.</param>
        /// <param name="eventAdapterService">The event adapter service.</param>
        /// <returns></returns>
	    public abstract ObjectArrayEventPropertyGetter GetGetterObjectArray(IDictionary<string, int> indexPerProperty, IDictionary<string, object> nestableTypes, EventAdapterService eventAdapterService);

	    /// <summary>
	    /// Return a String-array of atomic property names.
	    /// </summary>
	    /// <returns>
	    /// array of atomic names in a property expression
	    /// </returns>
	    public abstract string[] ToPropertyArray();

	    /// <summary>
	    /// Returns true for dynamic properties.
	    /// </summary>
	    /// <returns>
	    /// false for not-dynamic properties, true for dynamic properties.
	    /// </returns>
        public virtual bool IsDynamic
        {
            get { return false; }
        }

	    /// <summary>Write the EPL-representation of the property.</summary>
        /// <param name="writer">to write to</param>
        public virtual void ToPropertyEPL(TextWriter writer)
        {
        }
	}
}
