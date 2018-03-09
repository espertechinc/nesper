///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Configuration;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using Configuration = com.espertech.esper.client.Configuration;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Handles custom configuration sections for Esper.
    /// </summary>

    public class EsperSectionHandler : IConfigurationSectionHandler
    {
        private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="EsperSectionHandler" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public EsperSectionHandler(IContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EsperSectionHandler" /> class.
        /// </summary>
        public EsperSectionHandler()
        {
            _container = ContainerExtensions.CreateDefaultContainer(true);
        }

        #region IConfigurationSectionHandler Members

        /// <summary>
        /// Creates the section from the node information.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext">Configuration context object.</param>
        /// <param name="section"></param>
        /// <returns>The created section handler object.</returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            var configuration = new Configuration(_container);
            _container.Resolve<IConfigurationParser>()
                .DoConfigure(configuration, (XmlElement) section);
            return configuration;
        }

        #endregion
    }
}
