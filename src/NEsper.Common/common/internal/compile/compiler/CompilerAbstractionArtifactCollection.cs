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
    public interface CompilerAbstractionArtifactCollection
    {
        /// <summary>
        /// Returns the artifacts.
        /// </summary>
        ICollection<IArtifact> Artifacts { get; }

        /// <summary>
        /// Adds a set of artifacts to the collection.
        /// </summary>
        /// <param name="artifacts"></param>
        void Add(IEnumerable<IArtifact> artifacts);
        
        /// <summary>
        /// Adds a single artifact to the collection.
        /// </summary>
        /// <param name="artifact"></param>
        void Add(IArtifact artifact);
        
        // IDictionary<string, byte[]> Classes { get; }
        // void Add(IDictionary<string, byte[]> bytes);

        void Remove(IArtifact artifact);
    }
} // end of namespace