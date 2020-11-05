///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.serde
{
	/// <summary>
	///     Factory for serde providers.
	/// </summary>
	public interface SerdeProviderFactory
    {
	    /// <summary>
	    ///     Called by the runtime once at initialization time, returns a serde provider.
	    /// </summary>
	    /// <param name="context">runtime contextual information</param>
	    /// <returns>serde provide or null if none provided</returns>
	    SerdeProvider GetProvider(SerdeProviderFactoryContext context);
    }
} // end of namespace