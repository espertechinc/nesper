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
	///     Annotation for use with Json to provide an adapter for a given event property name.
	/// </summary>
	public class JsonSchemaFieldAttribute : Attribute
    {
        public virtual string Name { get; set; }
        public virtual string Adapter { get; set; }
    }
} // end of namespace