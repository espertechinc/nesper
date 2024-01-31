///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary> Invoked to perform output processing.</summary>
    /// <param name="doOutput">true if the batched events should actually be output as well as processed, false if they should just be processed
    /// </param>
    /// <param name="forceUpdate">true if output should be made even when no updating events have arrived
    /// </param>
    public delegate void OutputCallback(
        bool doOutput,
        bool forceUpdate);
}