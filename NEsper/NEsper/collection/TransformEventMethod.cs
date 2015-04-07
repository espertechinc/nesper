///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.collection
{
    /// <summary>Transform event returning the transformed event.</summary>
    /// <param name="_event">event to transform</param>
    /// <returns>transformed event</returns>
	public delegate EventBean TransformEventMethod( EventBean _event ) ;
} // End of namespace
