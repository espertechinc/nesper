///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Marker interface for event types that need not transpose their property.
    ///     <para />
    ///     Transpose is the process of taking a fragment event property and adding the fragment
    ///     to the resulting type rather then the underlying property object.
    /// </summary>
    public interface NativeEventType
    {
    }
}