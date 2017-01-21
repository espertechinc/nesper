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
    /// Configuration information for plugging in a custom view.
    /// </summary>

    [Serializable]
    public class ConfigurationPlugInView
	{
	    /// <summary>Gets or sets  the namespace</summary>
	    /// <returns>namespace</returns>
	    public String Namespace { get; set; }

	    /// <summary>Gets or sets  the view name.</summary>
	    /// <returns>view name</returns>
	    public String Name { get; set; }

	    /// <summary>Gets or sets the view factory class name.</summary>
	    /// <returns>factory class name</returns>
	    public String FactoryClassName { get; set; }
	}
} // End of namespace
