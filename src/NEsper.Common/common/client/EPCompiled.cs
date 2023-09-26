///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client.artifact;

namespace com.espertech.esper.common.client
{
    /// <summary>
    ///     The assembly of a compile EPL module or EPL fire-and-forget query.
    /// </summary>
    [Serializable]
    public class EPCompiled
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="artifacts">assemblies containing classes</param>
        /// <param name="manifest">the manifest</param>
        public EPCompiled(
            IArtifactRepository artifactRepository,
            ICollection<ICompileArtifact> artifacts,
            EPCompiledManifest manifest)
        {
            ArtifactRepository = artifactRepository;
            Artifacts = artifacts;
            Manifest = manifest;
        }

        /// <summary>
        /// Returns the artifact repository.
        /// </summary>
        public IArtifactRepository ArtifactRepository { get; set; }

        /// <summary>
        ///     Returns a set of assemblies.
        /// </summary>
        public IEnumerable<Assembly> Assemblies => Artifacts.Select(_ => _.Runtime.Assembly);

        /// <summary>
        ///     Returns a set of compiled artifacts.
        /// </summary>
        public ICollection<ICompileArtifact> Artifacts { get; }

        /// <summary>
        ///     Returns a manifest object
        /// </summary>
        /// <returns>manifest</returns>
        public EPCompiledManifest Manifest { get; }
    }
} // end of namespace