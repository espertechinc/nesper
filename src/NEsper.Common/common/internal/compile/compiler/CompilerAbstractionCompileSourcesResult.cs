///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.artifact;

namespace com.espertech.esper.common.@internal.compile.compiler
{
    public class CompilerAbstractionCompileSourcesResult
    {
        public CompilerAbstractionCompileSourcesResult(
            IDictionary<string, IList<string>> codeToClassNames,
            ICompileArtifact artifact)
        {
            CodeToClassNames = codeToClassNames;
            Artifact = artifact;
        }

        public ICompileArtifact Artifact { get; }

        public IDictionary<string, IList<string>> CodeToClassNames { get; }
    }
} // end of namespace