///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Connection factory settings for using a DriverManager.
    /// </summary>
    [Serializable]
    public class DriverManagerConnection : ConnectionFactoryDesc
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="className">is the driver class name</param>
        /// <param name="connectionString">is the database URL</param>
        /// <param name="optionalProperties">is connection properties</param>
        public DriverManagerConnection(
            string className,
            string connectionString,
            Properties optionalProperties)
        {
            ClassName = className;
            ConnectionString = connectionString;
            OptionalProperties = optionalProperties;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="className">is the driver class name</param>
        /// <param name="connectionString">is the database URL</param>
        /// <param name="optionalUserName">is a user name for connecting</param>
        /// <param name="optionalPassword">is a password for connecting</param>
        public DriverManagerConnection(
            string className,
            string connectionString,
            string optionalUserName,
            string optionalPassword)
        {
            ClassName = className;
            ConnectionString = connectionString;
            OptionalUserName = optionalUserName;
            OptionalPassword = optionalPassword;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="className">is the driver class name</param>
        /// <param name="connectionString">is the database URL</param>
        /// <param name="optionalUserName">is a user name for connecting</param>
        /// <param name="optionalPassword">is a password for connecting</param>
        /// <param name="optionalProperties">is connection properties</param>
        public DriverManagerConnection(
            string className,
            string connectionString,
            string optionalUserName,
            string optionalPassword,
            Properties optionalProperties)
        {
            ClassName = className;
            ConnectionString = connectionString;
            OptionalUserName = optionalUserName;
            OptionalPassword = optionalPassword;
            OptionalProperties = optionalProperties;
        }

        /// <summary>
        ///     Returns the driver manager class name.
        /// </summary>
        /// <returns>class name of driver manager</returns>
        public string ClassName { get; }

        /// <summary>
        ///     Returns the database connection string.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        ///     Returns the user name to connect to the database, or null if none supplied,
        ///     since the user name can also be supplied through properties.
        /// </summary>
        /// <returns>user name or null if none supplied</returns>
        public string OptionalUserName { get; }

        /// <summary>
        ///     Returns the password to connect to the database, or null if none supplied,
        ///     since the password can also be supplied through properties.
        /// </summary>
        /// <returns>password or null if none supplied</returns>
        public string OptionalPassword { get; }

        /// <summary>
        ///     Returns the properties, if supplied, to use for obtaining a connection via driver manager.
        /// </summary>
        /// <returns>properties to obtain a driver manager connection, or null if none supplied</returns>
        public Properties OptionalProperties { get; }
    }
}