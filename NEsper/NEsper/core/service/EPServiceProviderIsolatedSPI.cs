///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.core.service
{
    /// <summary>SPU for isolated service provider. </summary>
    public interface EPServiceProviderIsolatedSPI : EPServiceProviderIsolated
    {
        /// <summary>Return isolated services. </summary>
        /// <value>isolated services</value>
        EPIsolationUnitServices IsolatedServices { get; }
    }
}