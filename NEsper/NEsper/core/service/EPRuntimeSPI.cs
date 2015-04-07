///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using com.espertech.esper.client;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// SPI interface of the runtime exposes fire-and-forget, non-continuous query functionality.
    /// </summary>
    public interface EPRuntimeSPI : EPRuntime, IDisposable
    {
        /// <summary>
        /// Returns all declared variable names and their types.
        /// </summary>
        /// <value>variable names and types</value>
        IDictionary<string, Type> VariableTypeAll { get; }

        /// <summary>
        /// Returns a variable's type.
        /// </summary>
        /// <param name="variableName">type or null if the variable is not declared</param>
        /// <returns>type of variable</returns>
        Type GetVariableType(String variableName);

        /// <summary>
        /// Number of events routed internally.
        /// </summary>
        /// <value>event count routed internally</value>
        long RoutedInternal { get; }

        /// <summary>
        /// Number of events routed externally.
        /// </summary>
        /// <value>event count routed externally</value>
        long RoutedExternal { get; }

        IDictionary<string, long> StatementNearestSchedules { get; }

        /// <summary>
        /// Send an event represented by a plain object to the event stream processing runtime. 
        /// <para/> 
        /// Use the route method for sending events into the runtime from within UpdateListener code, to avoid 
        /// the possibility of a stack overflow due to nested calls to sendEvent.
        /// </summary>
        /// <param name="object">is the event to sent to the runtime</param>
        /// <returns></returns>
        /// <throws>com.espertech.esper.client.EPException is thrown when the processing of the event lead to an error</throws>
        EventBean WrapEvent(Object @object);

        /// <summary>
        /// Send a map containing event property values to the event stream processing runtime.
        /// <para/> 
        /// Use the route method for sending events into the runtime from within UpdateListener code. to avoid
        /// the possibility of a stack overflow due to nested calls to sendEvent.
        /// </summary>
        /// <param name="map">map that contains event property values. Keys are expected to be of type String while valuescan be of any type. Keys and values should match those declared via Configuration for the given eventTypeName.</param>
        /// <param name="eventTypeName">the name for the Map event type that was previously configured</param>
        /// <returns></returns>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        EventBean WrapEvent(IDictionary<string,object> map, String eventTypeName);

        /// <summary>
        /// Send an event represented by a DOM node to the event stream processing runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within UpdateListener code. to avoid 
        /// the possibility of a stack overflow due to nested calls to sendEvent.
        /// </summary>
        /// <param name="node">is the DOM node as an event</param>
        /// <returns></returns>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        EventBean WrapEvent(XmlNode node);
    
        void ProcessThreadWorkQueue();
        void Dispatch();
        void Initialize();

        void ProcessWrappedEvent(EventBean eventBean);

        /// <summary>
        /// Gets the engine URI.
        /// </summary>
        /// <value>The engine URI.</value>
        string EngineURI { get; }

        /// <summary>
        /// Clear short-lived memory that may temporarily retain references to stopped or destroyed statements.
        /// <para/>
        /// Use this method after stopping and destroying statements for the purpose of clearing thread-local 
        /// or other short lived storage to statement handles of deleted statements. 
        /// <para/>
        /// NOT safe to use without first acquiring the engine lock.
        /// </summary>
        void ClearCaches();
    }
}
