///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
	public class ConfigurationPlugInPatternObject
	{
	    private String _namespace;
	    private String name;
	    private String factoryClassName;
	    private PatternObjectTypeEnum? patternObjectType;

	    /// <summary>
		/// Gets or sets the view namespace
		/// </summary>
	    public String Namespace
	    {
	        get { return _namespace; }
			set { _namespace = value; }
	    }

	    /// <summary>
		/// Gets or sets the view name.
		/// </summary>
	    public String Name
	    {
	        get { return name; }
			set { this.name = value ; }
	    }

	    /// <summary>
		/// Gets or sets the view factory class name.
		/// </summary>
	    public String FactoryClassName
	    {
	        get { return factoryClassName; }
			set { this.factoryClassName = value ; }
	    }

	    /// <summary>
		/// Gets or sets the type of the pattern object for the plug-in.
		/// </summary>
	    public PatternObjectTypeEnum? PatternObjectType
	    {
	        get { return patternObjectType; }
			set { this.patternObjectType = value; }
	    }

	    /// <summary>Choice for type of pattern object.</summary>
	    public enum PatternObjectTypeEnum
	    {
	        /// <summary>Observer observes externally-supplied events.</summary>
	        OBSERVER,

	        /// <summary>Guard allows or disallows events from child expressions to pass.</summary>
	        GUARD
	    }
	}
}
