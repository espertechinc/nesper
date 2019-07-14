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
    public class ImportNamespace : Import
    {
        public ImportNamespace()
        {
        }

        public ImportNamespace(string ns)
        {
            Namespace = ns;
        }

        public ImportNamespace(Type typeInNamespace)
        {
            Namespace = typeInNamespace.Namespace;
        }

        public string Namespace { get; set; }

        public override Type Resolve(
            string providedTypeName,
            ClassForNameProvider classForNameProvider)
        {
            try {
                if (Namespace == null) {
                    return classForNameProvider.ClassForName(providedTypeName);
                }

                return classForNameProvider.ClassForName(
                    Namespace + '.' + providedTypeName);
            }
            catch (TypeLoadException e) {
                if (Log.IsDebugEnabled) {
                    Log.Debug($"TypeLoadException while resolving typeName = '{providedTypeName}'", e);
                }

                return null;
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}