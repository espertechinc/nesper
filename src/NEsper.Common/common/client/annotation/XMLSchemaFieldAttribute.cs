///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.annotation
{
	/// <summary>
	///     Annotation for use with XML to set a given event property name to use XPath.
	///     The name, xpath and type are required.
	/// </summary>
	public class XMLSchemaFieldAttribute : Attribute
    {
	    /// <summary>
	    ///     Property name
	    /// </summary>
	    /// <returns>name</returns>
	    public virtual string Name { get; set; }

	    /// <summary>
	    ///     XPath expression
	    /// </summary>
	    /// <returns>xpath</returns>
	    public virtual string XPath { get; set; }

	    /// <summary>
	    ///     Type as a string, i.e. "string" or "nodeset" and others
	    /// </summary>
	    /// <returns>type</returns>
	    public virtual string Type { get; set; }

	    /// <summary>
	    ///     For use when event properties themselves has an xml event type
	    /// </summary>
	    /// <returns>type name</returns>
	    public virtual string EventTypeName { get; set; } = "";

	    /// <summary>
	    ///     For casting the xpath evaluation result to a given type
	    /// </summary>
	    /// <returns>type to cast to</returns>
	    public virtual string CastToType { get; set; } = "";
    }
} // end of namespace