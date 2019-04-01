///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
	public class PropertyInfo {
	    private Type clazz;
	    private EventPropertyGetterSPIFactory getterFactory;
	    private PropertyStem stem;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="clazz">is the class</param>
	    /// <param name="getterFactory">is the getter</param>
	    /// <param name="stem">is the property info</param>
	    public PropertyInfo(Type clazz, EventPropertyGetterSPIFactory getterFactory, PropertyStem stem) {
	        this.clazz = clazz;
	        this.getterFactory = getterFactory;
	        this.stem = stem;
	    }

	    /// <summary>
	    /// Returns the return type.
	    /// </summary>
	    /// <returns>return type</returns>
	    public Type Clazz {
	        get => clazz;	    }

	    /// <summary>
	    /// Returns the getter.
	    /// </summary>
	    /// <returns>getter</returns>
	    public EventPropertyGetterSPIFactory GetterFactory {
	        get => getterFactory;	    }

	    /// <summary>
	    /// Returns the property info.
	    /// </summary>
	    /// <returns>property info</returns>
	    public PropertyStem Descriptor {
	        get => stem;	    }
	}
} // end of namespace