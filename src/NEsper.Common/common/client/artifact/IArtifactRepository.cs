﻿using System;
using System.Collections.Generic;

using com.espertech.esper.compat;

using Microsoft.CodeAnalysis;

namespace com.espertech.esper.common.client.artifact
{
    /// <summary>
    /// A model for a distributed assembly repository.  In the single process context, this is simply an
    /// in-memory assembly repository context.  The assembly repository model separates how an assembly
    /// is distributed from it is loaded into the current context.
    /// </summary>
    public interface IArtifactRepository : IDisposable
    {
        /// <summary>
        /// Returns an enumerable of all artifacts
        /// </summary>
        IEnumerable<IArtifact> Artifacts { get; }

        IEnumerable<ICompileArtifact> CompileArtifacts { get; }

        IEnumerable<IRuntimeArtifact> RuntimeArtifacts { get; }

        /// <summary>
        /// Registers a byte image with the repository and returns a unique id for that artifact.
        /// </summary>
        /// <returns>Returns a registered artifact</returns>
        ICompileArtifact Register(EPCompilationUnit compilationUnit);

        /// <summary>
        /// Resolves an artifact from the repository.
        /// </summary>
        /// <param name="artifactId"></param>
        /// <returns></returns>
        IRuntimeArtifact Resolve(string artifactId);

        /// <summary>
        /// Takes a compile artifact and deploys it.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        ICompileArtifact Deploy(ICompileArtifact source);

        /// <summary>
        /// Returns an enumeration of all metadata references.
        /// </summary>
        IEnumerable<MetadataReference> AllMetadataReferences { get; }

        /// <summary>
        /// Creates a classLoader.
        /// </summary>
        /// <value></value>
        TypeResolver TypeResolver { get; }
    }
}