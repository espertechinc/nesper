///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


/*
 * Created on Apr 22, 2006
 *
 */

using System.Collections.Generic;

namespace NEsper.Examples.Transaction.sim
{
    /// <summary>
    /// An iterable source of events
    /// </summary>
    
    public abstract class EventSource : IEnumerable<TxnEventBase>
    {
        abstract public IEnumerator<TxnEventBase> GetEnumerator() ;
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }   
}
