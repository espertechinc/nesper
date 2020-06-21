///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenClassBaseList
    {
        public CodegenTypeReference BaseType { get; private set; }
        
        public ISet<CodegenTypeReference> Interfaces { get;  }

        /// <summary>
        /// Constructs a new class base list.
        /// </summary>
        public CodegenClassBaseList()
        {
            BaseType = null;
            Interfaces = new HashSet<CodegenTypeReference>();
        }

        public void AssignType(Type baseType)
        {
            if (baseType != null) {
                if (baseType.IsInterface) {
                    Interfaces.Add(new CodegenTypeReference(baseType));
                }
                else {
                    BaseType = new CodegenTypeReference(baseType);
                }
            }
        }
        
        public void AddReferenced(ISet<Type> classes)
        {
            BaseType?.AddReferenced(classes);
            foreach(var @interface in Interfaces) {
                @interface.AddReferenced(classes);
            }
        }
    }
}