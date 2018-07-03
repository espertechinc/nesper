using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.espertech.esper.compat.container
{
    /// <summary>
    /// This is a container that gets used in cases where no container is provided.  It
    /// should be deprecated when we have identified that all 'static' usage cases have
    /// been eliminated.  By default, the fallback container contains very barebones
    /// items.
    /// </summary>
    public class FallbackContainer
    {
        private static IContainer _fallbackContainer;

        /// <summary>
        /// Gets or sets the instance.  Applications can set the fallback container if they
        /// want to.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static IContainer Instance {
            get => _fallbackContainer;
            set => _fallbackContainer = value;
        }

        public static IContainer GetInstance()
        {
            if (_fallbackContainer == null) {
                _fallbackContainer = ContainerExtensions.CreateDefaultContainer(false)
                    .InitializeDefaultServices()
                    .InitializeDatabaseDrivers();
            }

            return _fallbackContainer;
        }

        public static IContainer GetInstance(IContainer preferredContainer)
        {
            return preferredContainer ?? GetInstance();
        }
    }
}
