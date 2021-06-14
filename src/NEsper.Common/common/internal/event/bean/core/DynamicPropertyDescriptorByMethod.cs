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
	/// Provides method information for dynamic (unchecked) properties of each class for use in obtaining property values.
	/// </summary>
	public class DynamicPropertyDescriptorByMethod {
		/// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="clazz">the class to match when looking for a method</param>
	    /// <param name="method">the fast method to call</param>
	    /// <param name="hasParameters">true if the method takes parameters</param>
	    public DynamicPropertyDescriptorByMethod(Type clazz, MethodInfo method, bool hasParameters) {
	        this.Clazz = clazz;
	        this.Method = method;
	        this.HasParameters = hasParameters;
	    }

	    /// <summary>
	    /// Returns the class for the method.
	    /// </summary>
	    /// <value>class to match on</value>
	    public Type Clazz { get; }

	    /// <summary>
	    /// Returns the method to invoke.
	    /// </summary>
	    /// <value>method to invoke</value>
	    public MethodInfo Method { get; }

	    /// <summary>
	    /// Returns true if the method takes parameters.
	    /// </summary>
	    /// <value>indicator if parameters are required</value>
	    public bool HasParameters { get; }
	}
} // end of namespace
