///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.client.serde
{
	/// <summary>
	///     For use with high-availability and scale-out only, this class provides contextual information about the event type that we
	///     looking to serialize or de-serialize, for use with <seealso cref="SerdeProvider" />
	/// </summary>
	public class SerdeProviderEventTypeContext : SerdeProviderAdditionalInfo
    {
	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="raw">statement information</param>
	    /// <param name="eventType">event type</param>
	    public SerdeProviderEventTypeContext(
            StatementRawInfo raw,
            EventType eventType) : base(raw)
        {
            EventType = eventType;
        }

	    /// <summary>
	    ///     Returns the event type
	    /// </summary>
	    /// <value>event type</value>
	    public EventType EventType { get; }
    }
} // end of namespace