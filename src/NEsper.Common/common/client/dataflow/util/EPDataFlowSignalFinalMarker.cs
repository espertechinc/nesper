///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.dataflow.util
{
    /// <summary>Final marker for data flows. </summary>
    public interface EPDataFlowSignalFinalMarker : EPDataFlowSignal
    {
    }

    public class EPDataFlowSignalFinalMarkerImpl : EPDataFlowSignalFinalMarker
    {
    }
}