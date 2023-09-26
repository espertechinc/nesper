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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class NameParamNumAndModule
    {
        public static readonly NameParamNumAndModule[] EMPTY_ARRAY = Array.Empty<NameParamNumAndModule>();

        private readonly string name;
        private readonly int paramNum;
        private readonly string moduleName;

        public NameParamNumAndModule(
            string name,
            int paramNum,
            string moduleName)
        {
            this.name = name;
            this.paramNum = paramNum;
            this.moduleName = moduleName;
        }

        public string Name => name;

        public int ParamNum => paramNum;

        public string ModuleName => moduleName;

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (NameParamNumAndModule)o;

            if (paramNum != that.paramNum) {
                return false;
            }

            if (!name.Equals(that.name)) {
                return false;
            }

            return moduleName?.Equals(that.moduleName) ?? that.moduleName == null;
        }

        public override int GetHashCode()
        {
            var result = name.GetHashCode();
            result = 31 * result + paramNum;
            result = 31 * result + (moduleName != null ? moduleName.GetHashCode() : 0);
            return result;
        }

        public static CodegenExpression MakeArrayNullIfEmpty(ICollection<NameParamNumAndModule> names)
        {
            if (names.IsEmpty()) {
                return ConstantNull();
            }

            var expressions = new CodegenExpression[names.Count];
            var count = 0;
            foreach (var entry in names) {
                expressions[count++] = entry.Make();
            }

            return NewArrayWithInit(typeof(NameParamNumAndModule), expressions);
        }

        private CodegenExpression Make()
        {
            return NewInstance<NameParamNumAndModule>(Constant(name), Constant(paramNum), Constant(moduleName));
        }
    }
} // end of namespace