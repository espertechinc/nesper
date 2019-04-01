///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
	/// <summary>
	/// Precompiled statement that is prepared with substitution parameters and that
	/// can be created and started efficiently multiple times with different actual values for parameters.
	/// <para>
	/// When a precompiled statement is prepared via the prepare method on <see cref="EPAdministrator"/>,
	/// it typically has one or more substitution parameters in the statement text,
	/// for which the placeholder character is the question mark. This class provides methods to set
	/// the actual value for the substitution parameter.
	/// </para>
	/// <para>
	/// A precompiled statement can only be created and started when actual values for all
	/// substitution parameters are set.
	/// </para>
	/// </summary>
	public interface EPPreparedStatement
	{
	    /// <summary>
	    /// Sets the value of the designated parameter using the given object.
	    /// </summary>
	    /// <param name="parameterIndex">the first parameter is 1, the second is 2, ...</param>
	    /// <param name="value">the object containing the input parameter value</param>
	    void SetObject(int parameterIndex, Object value);

        /// <summary>
        /// Sets the value of the designated parameter using the given object.
        /// </summary>
        /// <param name="parameterName">the name of the parameter</param>
        /// <param name="value">the object containing the input parameter value</param>
        void SetObject(String parameterName, Object value);
	}
} // End of namespace
