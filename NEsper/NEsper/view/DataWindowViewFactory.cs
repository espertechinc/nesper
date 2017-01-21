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
    /// Marker interface for use with view factories that create data window views only.
    /// <para/>
    /// Please <see cref="DataWindowView"/> for details on views that meet data window requirements.
    /// </summary>
	public interface DataWindowViewFactory : ViewFactory
	{
	}
} // End of namespace
