///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.compile;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.classprovided.compiletime
{
    public class ClassProvidedPrecompileResult
    {
        public static readonly ClassProvidedPrecompileResult EMPTY = new ClassProvidedPrecompileResult(
            null,
            EmptyList<Type>.Instance);

        public ClassProvidedPrecompileResult(
            IRuntimeArtifact artifact,
            IList<Type> classes)
        {
            Artifact = artifact;
            Classes = classes;
        }

        public IRuntimeArtifact Artifact { get; }
        public IList<Type> Classes { get; }
    }
} // end of namespace