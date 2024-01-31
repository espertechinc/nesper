///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public interface EPVariableServiceSPI : EPVariableService
    {
        IDictionary<DeploymentIdNamePair, Type> VariableTypeAll { get; }

        Type GetVariableType(string deploymentId, string variableName);
    }
} // end of namespace