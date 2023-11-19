///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Text.Json.Serialization;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.client.configuration.common
{
    public class ImportNamespace : Import
    {
        public ImportNamespace()
        {
        }

        [JsonConstructor]
        public ImportNamespace(
            string @namespace,
            string assemblyName = null)
        {
            Namespace = @namespace;
            AssemblyName = assemblyName;
        }

        public ImportNamespace(Type typeInNamespace)
        {
            Namespace = typeInNamespace.Namespace;
            AssemblyName = typeInNamespace.Assembly.FullName;
        }

        public string Namespace { get; set; }
        public string AssemblyName { get; set; }

        public override Type Resolve(
            string providedTypeName,
            TypeResolver typeResolver)
        {
            try {
                if (Namespace == null) {
                    return typeResolver.ResolveType(providedTypeName, false);
                }

                return typeResolver.ResolveType($"{Namespace}.{providedTypeName}", false);
            }
            catch (TypeLoadException e) {
                if (Log.IsDebugEnabled) {
                    Log.Debug($"TypeLoadException while resolving typeName = '{providedTypeName}'", e);
                }

                return null;
            }
        }

        protected bool Equals(ImportNamespace other)
        {
            return string.Equals(AssemblyName, other.AssemblyName) &&
                   string.Equals(Namespace, other.Namespace);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ImportNamespace)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((AssemblyName != null ? AssemblyName.GetHashCode() : 0) * 397) ^
                       (Namespace != null ? Namespace.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"ImportNamespace: {nameof(Namespace)}: {Namespace}, {nameof(AssemblyName)}: {AssemblyName}";
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}