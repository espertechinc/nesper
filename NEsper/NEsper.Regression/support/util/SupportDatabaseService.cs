///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportDatabaseService
    {
        public static string DBUSER =>
            Environment.GetEnvironmentVariable("ESPER_MYSQL_HOST") ?? "esper";

        public static string DBPWD =>
            Environment.GetEnvironmentVariable("ESPER_MYSQL_PASSWORD") ?? "3sp3rP@ssw0rd";

        public static string DBHOST =>
            Environment.GetEnvironmentVariable("ESPER_MYSQL_HOST") ?? "nesper-mysql-integ.local";

        public static string DBNAME =>
            Environment.GetEnvironmentVariable("ESPER_MYSQL_DBNAME") ?? "test";

        public static string DRIVER => "PgSQL";

        public static Builder NewBuilder()
        {
            return new Builder(DefaultProperties);
        }

        public static Properties DefaultProperties {
            get {
                var properties = new Properties();
                properties["Server"] = DBHOST;
                properties["Uid"] = DBUSER;
                properties["Pwd"] = DBPWD;
                properties["Database"] = DBNAME;

                return properties;
            }
        }

        public class Builder
        {
            private Properties properties;

            public Builder(Properties properties)
            {
                this.properties = properties;
            }

            public Builder WithHost(string value)
            {
                properties["Server"] = value;
                return this;
            }

            public Builder WithUser(string value)
            {
                properties["UId"] = value;
                return this;
            }

            public Builder WithPassword(string value)
            {
                properties["Pwd"] = value;
                return this;
            }

            public Builder WithDatabase(string value)
            {
                properties["Database"] = value;
                return this;
            }

            public Properties Build()
            {
                return properties;
            }
        }
    }
} // end of namespace