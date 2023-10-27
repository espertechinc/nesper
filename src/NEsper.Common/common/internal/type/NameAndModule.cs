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
        public static readonly NameAndModule[] EMPTY_ARRAY = Array.Empty<NameAndModule>();

        private readonly string name;
        private readonly string moduleName;

        public NameAndModule(
            string name,
            string moduleName)
        {
            this.name = name;
            this.moduleName = moduleName;
        }

        public string Name => name;

        public string ModuleName => moduleName;

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (NameAndModule)o;

            if (!name.Equals(that.name)) {
                return false;
            }

            return moduleName?.Equals(that.moduleName) ?? that.moduleName == null;
        }

        public override int GetHashCode()
        {
            var result = name.GetHashCode();
            result = 31 * result + (moduleName != null ? moduleName.GetHashCode() : 0);
            return result;
        }

        public static CodegenExpression MakeArrayNullIfEmpty(ICollection<NameAndModule> names)
        {
            if (names.IsEmpty()) {
                return ConstantNull();
            }

            var expressions = new CodegenExpression[names.Count];
            var count = 0;
            foreach (var entry in names) {
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
            foreach (var item in names) {
                if (item.Name.Equals(searchForName)) {
                    if (found != null) {
                        throw new IllegalStateException($"Found multiple entries for name '{searchForName}'");
                    }

                    found = item;
                }
            }

            if (found == null) {
                throw new IllegalStateException($"Failed to find name '{searchForName}'");
            }

            return found;
        }
    }
} // end of namespace