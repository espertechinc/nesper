///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esperio
{
    /// <summary>Sender that sends without a threadpool. </summary>
    public class DirectSender : AbstractSender {
    
    	public override void SendEvent(AbstractSendableEvent theEvent, Object beanToSend) {
    		Runtime.SendEvent(beanToSend);
    	}
    
    	public override void SendEvent(AbstractSendableEvent theEvent, IDictionary<string, object> mapToSend, String eventTypeName) {
    		Runtime.SendEvent(mapToSend, eventTypeName);
    	}
    
    	public override void OnFinish() {
    		// do nothing
    	}
    }
}
