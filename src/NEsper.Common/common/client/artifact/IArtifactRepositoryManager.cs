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
        /// <param name="createIfMissing">create the repository if it is missing</param>
        IArtifactRepository GetArtifactRepository(
            string deploymentId,
            bool createIfMissing = false);

        /// <summary>
        /// Deletes (and unloads) the named repository.
        /// </summary>
        /// <param name="deploymentId"></param>
        void RemoveArtifactRepository(string deploymentId);
    }
}