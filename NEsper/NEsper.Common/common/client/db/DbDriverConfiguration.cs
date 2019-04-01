///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.db
{
    public class DbDriverConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the driver.
        /// </summary>
        /// <value>The name of the driver.</value>
        public string DriverName { get; set; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The properties.</value>
        public Properties Properties { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDriverConfiguration"/> class.
        /// </summary>
        public DbDriverConfiguration()
        {
            DriverName = null;
            Properties = new Properties();
        }
    }
}
