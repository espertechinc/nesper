///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.type
{
    public class NameAndModule
    {
        public static readonly NameAndModule[] EMPTY_ARRAY = new NameAndModule[0];

        private readonly string name;
        private readonly string moduleName;

        public NameAndModule(
            string name,
            string moduleName)
        {
            this.name = name;
            this.moduleName = moduleName;
        }

        public string Name {
            get => name;
        }

        public string ModuleName {
            get => moduleName;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            NameAndModule that = (NameAndModule) o;

            if (!name.Equals(that.name)) return false;
            return moduleName != null ? moduleName.Equals(that.moduleName) : that.moduleName == null;
        }

        public override int GetHashCode()
        {
            int result = name.GetHashCode();
            result = 31 * result + (moduleName != null ? moduleName.GetHashCode() : 0);
            return result;
        }

        public static CodegenExpression MakeArray(ICollection<NameAndModule> names)
        {
            if (names.IsEmpty()) {
                return EnumValue(typeof(NameAndModule), "EMPTY_ARRAY");
            }

            CodegenExpression[] expressions = new CodegenExpression[names.Count];
            int count = 0;
            foreach (NameAndModule entry in names) {
                expressions[count++] = entry.Make();
            }

            return NewArrayWithInit(typeof(NameAndModule), expressions);
        }

        private CodegenExpression Make()
        {
            return NewInstance<NameAndModule>(Constant(name), Constant(moduleName));
        }

        public static NameAndModule FindName(
            string searchForName,
            NameAndModule[] names)
        {
            NameAndModule found = null;
            foreach (NameAndModule item in names) {
                if (item.Name.Equals(searchForName)) {
                    if (found != null) {
                        throw new IllegalStateException("Found multiple entries for name '" + searchForName + "'");
                    }

                    found = item;
                }
            }

            if (found == null) {
                throw new IllegalStateException("Failed to find name '" + searchForName + "'");
            }

            return found;
        }
    }
} // end of namespace