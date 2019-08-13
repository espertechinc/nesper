///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.client.configuration.common
{
    [Serializable]
    public class ImportType : Import
    {
        public ImportType()
        {
        }

        public ImportType(Type type) : this(type.FullName)
        {
        }

        public ImportType(string typeName)
        {
            TypeName = typeName;

            int lastIndex = typeName.LastIndexOf('+');
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

        public string Namespace { get; set; }
        public string TypeNameBase { get; set; }
        public string TypeName { get; set; }

        public override Type Resolve(
            string providedTypeName,
            ClassForNameProvider classForNameProvider)
        {
            try {
                if (Namespace == null) {
                    if (providedTypeName == TypeName) {
                        return classForNameProvider.ClassForName(providedTypeName);
                    }
                }
                else {
                    if (providedTypeName == TypeNameBase) {
                        return classForNameProvider.ClassForName(TypeName);
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

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}