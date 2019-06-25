///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    /// Marker interface for use with view factories that create data window views
    /// that are asymetric in posting insert and remove stream data:
    /// Data windows that post only a partial insert and remove stream as output when compared to
    /// the insert and remove stream received.
    /// </summary>
    public interface AsymetricDataWindowViewForge : DataWindowViewForge
    {
    }
} // end of namespace
