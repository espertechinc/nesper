///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
	/// <summary>
	///     Provides method information for dynamic (unchecked) properties of each class for use in obtaining property values.
	/// </summary>
	public class DynamicPropertyDescriptorByProperty
    {
	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="clazz">the class to match when looking for a method</param>
	    /// <param name="property">the property to use</param>
	    public DynamicPropertyDescriptorByProperty(
            Type clazz,
            PropertyInfo property)
        {
            Clazz = clazz;
            Property = property;
        }

	    /// <summary>
	    ///     Returns the class for the method.
	    /// </summary>
	    /// <value>class to match on</value>
	    public Type Clazz { get; }

	    /// <summary>
	    ///     Returns the property.
	    /// </summary>
	    /// <value>property to use</value>
	    public PropertyInfo Property { get; }
    }
} // end of namespace