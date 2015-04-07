///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.magic;
using com.espertech.esper.schedule;

namespace com.espertech.esperio
{
    /// <summary>
    /// An implementation of SendableEvent that wraps a Map event for sending into the runtime.
    /// </summary>
    public class SendableBeanEvent : AbstractSendableEvent
    {
    	private readonly Object beanToSend;
    
    	/// <summary>Converts mapToSend to an instance of beanType </summary>
    	/// <param name="mapToSend">the map containing data to send into the runtime</param>
    	/// <param name="beanType">type of the bean to create from mapToSend</param>
    	/// <param name="eventTypeName">the event type alias for the map event</param>
    	/// <param name="timestamp">the timestamp for this event</param>
    	/// <param name="scheduleSlot">the schedule slot for the entity that created this event</param>
    	public SendableBeanEvent(IEnumerable<KeyValuePair<string, object>> mapToSend, Type beanType, String eventTypeName, long timestamp, ScheduleSlot scheduleSlot)
    		: base(timestamp, scheduleSlot)
    	{
    	    MagicType magicType = MagicType.GetCachedType(beanType);

    		try {
    		    beanToSend = Activator.CreateInstance(beanType);
                // pre-create nested properties if any
                foreach(var mProperty in magicType.GetAllProperties(false)) {
                    if ((mProperty.PropertyType != typeof(string)) &&
                        (mProperty.PropertyType.IsPrimitive == false) &&
                        (mProperty.CanWrite)) {
                        mProperty.SetFunction(beanToSend, Activator.CreateInstance(mProperty.PropertyType));
                    }
                }

    			// this method silently ignores read only properties on the dest bean but we should 
    			// have caught them in CSVInputAdapter.constructPropertyTypes.

                foreach( KeyValuePair<String,Object> entry in mapToSend ) {
                    var magicProperty = magicType.ResolveComplexProperty(entry.Key, PropertyResolutionStyle.CASE_SENSITIVE);
                    var magicAssignment = magicProperty.SetFunction;
                    if (magicAssignment == null)
                        continue;
                    
                    magicAssignment(beanToSend, entry.Value);
                }
    		} catch (Exception e) {
    			throw new EPException("Cannot populate bean instance", e);
    		}
    	}

        /// <summary>
        /// Sends the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public override void Send(AbstractSender sender)
        {
            sender.SendEvent(this, beanToSend);
    	}
    
    	public override String ToString()
    	{
    		return beanToSend.ToString();
    	}
    }
}
