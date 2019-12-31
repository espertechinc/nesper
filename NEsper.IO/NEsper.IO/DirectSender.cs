///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esperio
{
    /// <summary>Sender that sends without a threadpool. </summary>
    public class DirectSender : AbstractSender {
    
    	public override void SendEvent(
	        AbstractSendableEvent theEvent,
	        object beanToSend,
	        string eventTypeName) {
    		Runtime.SendEventBean(beanToSend, eventTypeName);
    	}
    
    	public override void SendEvent(
	        AbstractSendableEvent theEvent, 
	        IDictionary<string, object> mapToSend,
	        string eventTypeName) {
    		Runtime.SendEventMap(mapToSend, eventTypeName);
    	}
    
    	public override void OnFinish() {
    		// do nothing
    	}
    }
}
