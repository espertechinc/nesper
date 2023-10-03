///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.compiler
{
    public class CompilerAbstractionArtifactCollectionImpl : CompilerAbstractionArtifactCollection
    {
        private readonly IList<IArtifact> _artifacts = new List<IArtifact>();

        public ICollection<IArtifact> Artifacts => _artifacts;

        public void Add(IEnumerable<IArtifact> artifacts)
        {
            _artifacts.AddAll(artifacts);
        }

        public void Add(IArtifact artifact)
        {
            _artifacts.Add(artifact);
        }

        public void Remove(string name)
        {
            throw new NotSupportedException();
        }
    }
} // end of namespace