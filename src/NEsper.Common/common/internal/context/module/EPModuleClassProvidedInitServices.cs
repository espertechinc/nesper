///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.epl.classprovided.core;

namespace com.espertech.esper.common.@internal.context.module
{
    public interface EPModuleClassProvidedInitServices
    {
        IRuntimeArtifact ResolveArtifact(string artifactName);

        ClassProvidedCollector ClassProvidedCollector { get; }
    }
} // end of namespace