///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenInstanceAux
    {
        public CodegenInstanceAux(CodegenCtor serviceCtor)
        {
            ServiceCtor = serviceCtor;
            Members = new List<CodegenTypedParam>();
            Properties = new CodegenNamedProperties();
            Methods = new CodegenNamedMethods();
        }

        public CodegenCtor ServiceCtor { get; }

        public IList<CodegenTypedParam> Members { get; }

        public CodegenNamedProperties Properties { get; }

        public CodegenNamedMethods Methods { get; }

        public void AddMember(
            string name,
            Type type)
        {
            if (Members.Any(member => member.Name == name)) {
                throw new IllegalStateException("Member by name '" + name + "' already added");
            }

            Members.Add(new CodegenTypedParam(type, name));
        }

        public bool HasMember(string name)
        {
            return Members.Any(member => member.Name == name);
        }
    }
} // end of namespace