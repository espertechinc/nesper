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

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenField
    {
        public CodegenField(
            string clazz,
            string name,
            Type type,
            Type optionalTypeParam,
            bool isFinal)
        {
            Clazz = clazz;
            Name = name;
            Type = type;
            OptionalTypeParam = optionalTypeParam;
            IsFinal = isFinal;
        }

        public Type OptionalTypeParam { get; }

        public string Clazz { get; }

        public string Name { get; }

        public Type Type { get; }

        public bool IsFinal { get; }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (CodegenField) o;

            return Name.Equals(that.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.Add(Type);
            if (OptionalTypeParam != null) {
                classes.Add(OptionalTypeParam);
            }
        }

        public void Render(StringBuilder builder)
        {
            builder.Append(Clazz).Append('.').Append(Name);
        }
    }
} // end of namespace