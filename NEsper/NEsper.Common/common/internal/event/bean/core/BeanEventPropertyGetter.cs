///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
	/// <summary>
	/// Shortcut-evaluator for use with POJO-backed events only.
	/// </summary>
	public interface BeanEventPropertyGetter : EventPropertyGetterSPI {
	    /// <summary>
	    /// Returns the property as an object.
	    /// </summary>
	    /// <param name="object">to evaluate</param>
	    /// <returns>property of object</returns>
	    /// <throws>PropertyAccessException if access failed</throws>
	    object GetBeanProp(object @object) ;

	    /// <summary>
	    /// Returns true if the dynamic property exists.
	    /// </summary>
	    /// <param name="object">to evaluate</param>
	    /// <returns>indicator if property exists</returns>
	    bool IsBeanExistsProperty(object @object);

	    Type BeanPropType { get; }

	    Type TargetType { get; }
	}
} // end of namespace