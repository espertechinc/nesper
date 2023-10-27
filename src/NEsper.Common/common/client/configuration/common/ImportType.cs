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
    [Serializable]
    public class ImportType : Import
    {
        public ImportType()
        {
        }

        public ImportType(Type type) : this(type.FullName, type.Assembly.FullName)
        {
        }

        [JsonConstructor]
        public ImportType(
            string typeName,
            string assemblyName = null)
        {
            TypeName = typeName;
            AssemblyName = assemblyName;

            var lastIndex = typeName.LastIndexOf('+');
            if (lastIndex == -1) {
                lastIndex = typeName.LastIndexOf('.');
                if (lastIndex == -1) {
                    Namespace = null;
                    TypeNameBase = typeName;
                }
                else {
                    Namespace = typeName.Substring(0, lastIndex);
                    TypeNameBase = typeName.Substring(lastIndex + 1);
                }
            }
            else {
                Namespace = typeName.Substring(0, lastIndex);
                TypeNameBase = typeName.Substring(lastIndex + 1);
            }
        }

        public string TypeName { get; set; }
        public string AssemblyName { get; set; }

        [JsonIgnore]
        public string Namespace { get; set; }
        [JsonIgnore]
        public string TypeNameBase { get; set; }

        public override Type Resolve(
            string providedTypeName,
            TypeResolver typeResolver)
        {
            try {
                if (Namespace == null) {
                    if (providedTypeName == TypeName) {
                        return typeResolver.ResolveType(providedTypeName, false);
                    }
                }
                else {
                    if (providedTypeName == TypeNameBase) {
                        return typeResolver.ResolveType(TypeName, false);
                    }
                }
            }
            catch (TypeLoadException e) {
                if (Log.IsDebugEnabled) {
                    Log.Debug($"TypeLoadException while resolving typeName = '{providedTypeName}'", e);
                }

                return null;
            }

            return null;
        }

        protected bool Equals(ImportType other)
        {
            return string.Equals(Namespace, other.Namespace) &&
                   string.Equals(TypeNameBase, other.TypeNameBase) &&
                   string.Equals(TypeName, other.TypeName) &&
                   string.Equals(AssemblyName, other.AssemblyName);
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

            return Equals((ImportType)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = Namespace != null ? Namespace.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (TypeNameBase != null ? TypeNameBase.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AssemblyName != null ? AssemblyName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return
                $"ImportType: {nameof(Namespace)}: {Namespace}, {nameof(TypeNameBase)}: {TypeNameBase}, {nameof(TypeName)}: {TypeName}, {nameof(AssemblyName)}: {AssemblyName}";
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}