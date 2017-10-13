///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.view
{
    /// <summary>
    /// Views that require initialization after view instantiation and after view hook-up with the parent view
    /// can impleeent this interface and get invoked to initialize.
    /// </summary>
	public interface InitializableView
	{
	    /// <summary>Initializes a view.</summary>
	    void Initialize();
	}
} // End of namespace
