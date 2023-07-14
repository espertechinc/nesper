using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.compat;

#if NETCORE
using System.Runtime.Loader;
#endif

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.client.artifact
{
    public class DefaultArtifactRepositoryManager : IArtifactRepositoryManager, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDictionary<string, IArtifactRepository> _repositoryTable;
        private readonly TypeResolver _parentTypeResolver;
        private readonly AssemblyResolver _assemblyResolver;

#if NETCORE
        public DefaultArtifactRepositoryManager(
            TypeResolver parentTypeResolver,
            AssemblyResolver assemblyResolver)
#else
        public DefaultArtifactRepositoryManager(TypeResolver parentTypeResolver)
#endif
        {
            _parentTypeResolver = parentTypeResolver;
            _assemblyResolver = assemblyResolver;
            _repositoryTable = new Dictionary<string, IArtifactRepository>();

#if NETCORE
            DefaultRepository = new ArtifactRepositoryAssemblyLoadContext(
                _parentTypeResolver, _assemblyResolver);
#else
            DefaultRepository = new ArtifactRepositoryAppDomain(AppDomain.CurrentDomain);
#endif
        }

        /// <summary>
        /// Clean up and dispose of the repository manager
        /// </summary>
        public void Dispose()
        {
            lock (this) {
                // Copy the repositories into a temporary list
                var repositories = _repositoryTable.Values.ToList();
                Log.Info($"Dispose(): cleaning up {repositories.Count} repositories");
                // Clear the repositories table to avoid lingering references
                _repositoryTable.Clear();
                // Dispose of all repositories that were created by this manager
                foreach (var repository in repositories) {
                    repository.Dispose();
                }
            }
        }

        /// <summary>
        /// The default artifact repository
        /// </summary>
        public IArtifactRepository DefaultRepository { get; }

        /// <summary>
        /// Returns a named repository (based on deployment)
        /// </summary>
        /// <param name="deploymentId"></param>
        /// <param name="createIfMissing"></param>
        public IArtifactRepository GetArtifactRepository(string deploymentId, bool createIfMissing)
        {
            lock (_repositoryTable) {
                if (!_repositoryTable.TryGetValue(deploymentId, out var artifactRepository) && createIfMissing) {
#if NETCORE
                    artifactRepository = new ArtifactRepositoryAssemblyLoadContext(
                        deploymentId,
                        _parentTypeResolver,
                        _assemblyResolver);
#else
                    artifactRepository = new ArtifactRepositoryAppDomain(AppDomain.CurrentDomain);
#endif

                    _repositoryTable[deploymentId] = artifactRepository;
                }

                return artifactRepository;
            }
        }

        public void RemoveArtifactRepository(string deploymentId)
        {
            lock (_repositoryTable) {
                if (_repositoryTable.TryGetValue(deploymentId, out var artifactRepository)) {
                    _repositoryTable.Remove(deploymentId);
                    artifactRepository.Dispose();
                }
            }
        }
    }
}