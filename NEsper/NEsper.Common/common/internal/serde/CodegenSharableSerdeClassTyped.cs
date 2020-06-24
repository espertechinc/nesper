///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.serdeset.additional;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.serde
{
    public class CodegenSharableSerdeClassTyped : CodegenFieldSharable
    {
        private readonly CodegenSharableSerdeName name;
        private readonly Type valueType;
        private readonly DataInputOutputSerdeForge forge;
        private readonly CodegenClassScope classScope;

        public CodegenSharableSerdeClassTyped(
            CodegenSharableSerdeName name,
            Type valueType,
            DataInputOutputSerdeForge forge,
            CodegenClassScope classScope)
        {
            this.name = name;
            this.valueType = valueType;
            this.forge = forge;
            this.classScope = classScope;
        }

        public Type Type()
        {
            return typeof(DataInputOutputSerdeWCollation<object>);
        }

        public CodegenExpression InitCtorScoped()
        {
            var serde = forge.Codegen(classScope.NamespaceScope.InitMethod, classScope, null);
            if (name == CodegenSharableSerdeName.VALUE_NULLABLE) {
                return serde;
            } else if (name == CodegenSharableSerdeName.REFCOUNTEDSET) {
                return NewInstance<DIORefCountedSet>(serde);
            } else if (name == CodegenSharableSerdeName.SORTEDREFCOUNTEDSET) {
                return NewInstance<DIOSortedRefCountedSet>(serde);
            } else {
                throw new ArgumentException("Unrecognized name " + name);
            }
        }

        protected bool Equals(CodegenSharableSerdeClassTyped other)
        {
            return Equals(name, other.name) && Equals(valueType, other.valueType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((CodegenSharableSerdeClassTyped) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((name != null ? name.GetHashCode() : 0) * 397) ^ (valueType != null ? valueType.GetHashCode() : 0);
            }
        }

        public class CodegenSharableSerdeName
        {
            public static readonly CodegenSharableSerdeName VALUE_NULLABLE =
                new CodegenSharableSerdeName("ValueNullable", typeof(object));

            public static readonly CodegenSharableSerdeName REFCOUNTEDSET =
                new CodegenSharableSerdeName("RefCountedSet", typeof(object));

            public static readonly CodegenSharableSerdeName SORTEDREFCOUNTEDSET =
                new CodegenSharableSerdeName("SortedRefCountedSet", typeof(object));

            private CodegenSharableSerdeName(string methodName, params Type[] methodTypeArgs)
            {
                MethodName = methodName;
                MethodTypeArgs = methodTypeArgs;
            }

            public string MethodName { get; }

            public Type[] MethodTypeArgs { get; set; }
        }
    }
} // end of namespace