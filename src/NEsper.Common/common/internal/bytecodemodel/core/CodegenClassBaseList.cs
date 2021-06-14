///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

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

        public CodegenClassBaseList AssignType(Type baseType)
        {
            if (baseType != null) {
                if (baseType.IsInterface) {
                    Interfaces.Add(new CodegenTypeReference(baseType));
                }
                else {
                    BaseType = new CodegenTypeReference(baseType);
                }
            }

            return this;
        }
   
        public CodegenClassBaseList AssignBaseType(string baseTypeName)
        {
            if (!string.IsNullOrWhiteSpace(baseTypeName)) {
                BaseType = new CodegenTypeReference(baseTypeName);
            }

            return this;
        }

        public CodegenClassBaseList AddInterface(Type interfaceType)
        {
            if (interfaceType == null) {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            if (!interfaceType.IsInterface) {
                throw new ArgumentException("interfaceType type is not an interface", nameof(interfaceType));
            }

            var typeReference = new CodegenTypeReference(interfaceType);
            if (!Interfaces.Contains(typeReference)) {
                Interfaces.Add(typeReference);
            }

            return this;
        }

        public CodegenClassBaseList AddInterface(string interfaceName)
        {
            if (!string.IsNullOrWhiteSpace(interfaceName)) {
                Interfaces.Add(new CodegenTypeReference(interfaceName));
            }

            return this;
        }

        public void AddReferenced(ISet<Type> classes)
        {
            BaseType?.AddReferenced(classes);
            foreach(var @interface in Interfaces) {
                @interface.AddReferenced(classes);
            }
        }

        public void Render(StringBuilder builder)
        {
            var delimiter = " : ";
                
            if (BaseType != null) {
                builder.Append(delimiter);
                BaseType.Render(builder);
                delimiter = ", ";
            }

            foreach (var implement in Interfaces) {
                builder.Append(delimiter);
                implement.Render(builder);
                delimiter = ", ";
            }
        }
    }
}