///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportunit.epl
{
	public class SupportInitialContextFactory : DirectoryFactory
    {
	    private static readonly IDictionary<string, object> contextEntries = new Dictionary<string, object>();

	    public static void AddContextEntry(string name, object value)
	    {
	        contextEntries.Put(name, value);
	    }

	    public void Dispose()
	    {
	    }

	    public Directory GetInitialContext(IDictionary<string, object> environment) {
	        return new SupportContext(contextEntries);
	    }

	    public class SupportContext : Directory
        {
	        private IDictionary<string, object> contextEntries;

	        public SupportContext(IDictionary<string, object> contextEntries)
	                {
	            this.contextEntries = contextEntries;
	        }

	        public object Lookup(string name)
	        {
	            if (!contextEntries.ContainsKey(name))
	            {
	                throw new DirectoryException("Name '" + name + "' not found");
	            }
	            return contextEntries.Get(name);
	        }

	        public void Dispose()
	        {
	        }

	        public void Bind(string name, object obj)
	        {
	            throw new NotSupportedException();
	        }

	        public void Rebind(string name, object obj)
	        {
                throw new NotSupportedException();
            }

	        public void Rename(string oldName, string newName)
	        {
                throw new NotSupportedException();
            }

	        public IEnumerator<string> List(string name)
	        {
                throw new NotSupportedException();
	        }

	        public void Unbind(string name)
	        {
                throw new NotSupportedException();
            }
        }
	}
} // end of namespace
