///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


/*
 * Created on Apr 23, 2006
 *
 */

using System.Collections.Generic;

namespace NEsper.Examples.Transaction.sim
{
    /// <summary>
    /// Interface to output events in your preferred format.
    /// </summary>

    public interface OutputStream
    {
        void Output(IList<TxnEventBase> bucket);
    }
}
