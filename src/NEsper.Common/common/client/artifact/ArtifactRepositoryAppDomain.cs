using System;
using System.Reflection;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.artifact
{
    public class ArtifactRepositoryAppDomain : BaseArtifactRepository
    {
        public AppDomain AppDomain { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ArtifactRepositoryAppDomain() : this(AppDomain.CurrentDomain)
        {
        }

        /// <summary>
        /// Constructor with AppDomain.
        /// </summary>
        /// <param name="appDomain"></param>
        public ArtifactRepositoryAppDomain(AppDomain appDomain)
        {
            AppDomain = appDomain;
        }

        protected override Assembly MaterializeAssembly(byte[] image)
        {
            return AppDomain.Load(image);
        }
    }
}