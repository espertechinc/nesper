using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat;

using Microsoft.CodeAnalysis;

namespace com.espertech.esper.common.client.artifact
{
    public interface IArtifactRepositoryManager
    {
        /// <summary>
        /// Returns the default repository
        /// </summary>
        IArtifactRepository DefaultRepository { get; }

        /// <summary>
        /// Returns a named repository (based on deployment)
        /// </summary>
        /// <param name="deploymentId"></param>
        IArtifactRepository GetArtifactRepository(string deploymentId);

        /// <summary>
        /// Deletes (and unloads) the named repository.
        /// </summary>
        /// <param name="deploymentId"></param>
        void RemoveArtifactRepository(string deploymentId);
    }
}