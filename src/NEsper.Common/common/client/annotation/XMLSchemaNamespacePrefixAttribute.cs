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
	///     Annotation for use with XML schemas to define a namespace prefix.
	/// </summary>
	public class XMLSchemaNamespacePrefixAttribute : Attribute
    {
	    /// <summary>
	    ///     Prefix
	    /// </summary>
	    /// <returns>prefix</returns>
	    public virtual string Prefix { get; set; }

	    /// <summary>
	    ///     Namespace
	    /// </summary>
	    /// <returns>namespace</returns>
	    public virtual string Namespace { get; set; }
    }
} // end of namespace