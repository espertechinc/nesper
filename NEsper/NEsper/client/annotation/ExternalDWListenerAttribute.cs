///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.annotation
{
    /// <summary>Annotation for configuring external data window listeners. </summary>
    public class ExternalDWListenerAttribute : Attribute
    {
        public ExternalDWListenerAttribute()
        {
            Threaded = true;
            NumThreads = 1;
        }

        /// <summary>Returns indicator whether a listener thread is required or not. </summary>
        /// <returns>indicator</returns>
        public bool Threaded { get; set; }

        /// <summary>Returns indicator the number of listener threads. </summary>
        /// <returns>number of threads</returns>
        public int NumThreads { get; set; }
    }
}