///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.compat
{
    /// <summary>
    /// A simple delegate that can be observed.  Observable delegates are primarily
    /// designed to be used with calls that wrap the child call for collection of
    /// diagnostics.
    /// </summary>
    public delegate void ObservableCall();
}
