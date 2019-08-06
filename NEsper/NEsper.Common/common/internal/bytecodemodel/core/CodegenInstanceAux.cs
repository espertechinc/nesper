///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenInstanceAux
    {
        private IList<CodegenTypedParam> members;

        public CodegenInstanceAux(CodegenCtor serviceCtor)
        {
            ServiceCtor = serviceCtor;
        }

        public CodegenCtor ServiceCtor { get; }

        public IList<CodegenTypedParam> Members =>
            members ?? Collections.GetEmptyList<CodegenTypedParam>();

        public CodegenNamedMethods Methods { get; } = new CodegenNamedMethods();

        public void AddMember(
            string name,
            Type type)
        {
            if (members == null) {
                members = new List<CodegenTypedParam>(2);
            }

            foreach (var member in members) {
                if (member.Name.Equals(name)) {
                    throw new IllegalStateException("Member by name '" + name + "' already added");
                }
            }

            members.Add(new CodegenTypedParam(type, name));
        }

        public bool HasMember(string name)
        {
            if (members == null) {
                return false;
            }

            foreach (var member in members) {
                if (member.Name.Equals(name)) {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace