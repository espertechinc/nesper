///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Send a map containing event property values to the event stream processing runtime.
    /// Use the route method for sending events into the runtime from within UpdateListener code.
    /// </summary>
    /// <param name="mappedEvent">map that contains event property values. Keys are expected to be of type String while values
    /// can be of any type. Keys and values should match those declared via Configuration for the given eventTypeAlias. 
    /// </param>
    /// <throws>  EPException - when the processing of the event leads to an error </throws>
    public delegate void EPSender(DataMap mappedEvent);
}
