///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     Tag interface for derived-value views. Derived-value views follow the view interface and do not keep a window over
    ///     the
    ///     data received by their parent view. They simply derive a set of data points from a stream and
    ///     do not retain events.
    ///     <para />
    ///     Derived-Value views generally follow the following behavior:
    ///     <para />
    ///     They publish the output data when receiving insert or remove stream data from their parent view,
    ///     directly and not time-driven.
    ///     <para />
    ///     They typically change event type compared to their parent view, since they derive new information
    ///     or add information to events.
    /// </summary>
    public interface DerivedValueView : View
    {
    }
} // end of namespace