///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.supportregression.epl
{
	public class SupportDriverManagerDataSource
    {
	    private Properties properties;

	    public SupportDriverManagerDataSource(Properties properties) {
	        this.properties = properties;
	    }

	    public Connection GetConnection() {
	        return ConnectionInternal;
	    }

	    public Connection GetConnection(string username, string password) {
	        throw new UnsupportedOperationException();
	    }

	    private Connection GetConnectionInternal() {
	        // load driver class
	        string driverClassName = properties.GetProperty("driverClassName");
	        try {
	            ClassLoader cl = Thread.CurrentThread().ContextClassLoader;
	            Type.ForName(driverClassName, true, cl);
	        } catch (ClassNotFoundException ex) {
	            throw new RuntimeException("Error loading driver class '" + driverClassName + '\'', ex);
	        } catch (RuntimeException ex) {
	            throw new RuntimeException("Error loading driver class '" + driverClassName + '\'', ex);
	        }

	        // use driver manager to get a connection
	        Connection connection;
	        string url = properties.GetProperty("url");
	        string user = properties.GetProperty("username");
	        string pwd = properties.GetProperty("password");

	        try {
	            connection = DriverManager.GetConnection(url, user, pwd);
	        } catch (SQLException ex) {
	            string detail = "SQLException: " + ex.Message +
	                            " SQLState: " + ex.SQLState +
	                            " VendorError: " + ex.ErrorCode;

	            throw new RuntimeException("Error obtaining database connection using url '" + url +
	                                       "' with detail " + detail
	                                       , ex);
	        }

	        return connection;
	    }

	    public TextWriter GetLogWriter() {
	        return null;
	    }

	    public void SetLogWriter(TextWriter outWriter) {
	    }

	    public void SetLoginTimeout(int seconds) {
	    }

	    public int GetLoginTimeout() {
	        return 0;
	    }

	    public T Unwrap<T>(Type<T> iface) {
	        return null;
	    }

	    public bool IsWrapperFor(Type<?> iface) {
	        return false;
	    }

	    public ILog GetParentLogger() {
	        throw new SQLFeatureNotSupportedException();
	    }
	}
} // end of namespace
