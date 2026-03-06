///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Castle.Windsor;

namespace com.espertech.esper.container
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Creates the default service collection.
        /// </summary>
        /// <returns></returns>
        public static IContainer CreateDefaultContainer(bool initialize = true)
        {
            var wrapper = new ContainerImpl(new WindsorContainer());
            if (initialize) {
                wrapper.InitializeDefaultServices();
            }

            return wrapper;
        }

        public static void CheckContainer(this IContainer container)
        {
            if (container == null) {
                throw new ArgumentException(
                    "container is null, please initialize",
                    nameof(container));
            }
        }
    }
}
