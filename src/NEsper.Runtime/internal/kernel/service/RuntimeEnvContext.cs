///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.directory;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    /// <summary>
    ///     Implements a JNDI context for providing a directory for runtime-external resources such as adapters.
    /// </summary>
    public class RuntimeEnvContext : INamingContext
    {
        private readonly IDictionary<string, object> context;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public RuntimeEnvContext()
        {
            context = new Dictionary<string, object>();
        }

        public IDictionary Environment => System.Environment.GetEnvironmentVariables();

        public string NameInNamespace => throw new UnsupportedOperationException();

        public void Dispose()
        {
        }

        public object Lookup(string name)
        {
            return context.Get(name);
        }

        public void Bind(
            string name,
            object obj)
        {
            context[name] = obj;
        }

        public void Rebind(
            string name,
            object obj)
        {
            context[name] = obj;
        }

        public void Unbind(string name)
        {
            context.Remove(name);
        }

        public void Rename(
            string oldName,
            string newName)
        {
            if (context.TryRemove(oldName, out var value))
            {
                context[newName] = value;
            }
        }

        public IEnumerator<string> List(string name)
        {
            throw new NotSupportedException();
        }
    }
} // end of namespace