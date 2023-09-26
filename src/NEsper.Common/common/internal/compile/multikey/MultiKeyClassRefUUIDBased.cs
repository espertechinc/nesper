///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.multikey
{
    public class MultiKeyClassRefUUIDBased : MultiKeyClassRef
    {
        private readonly string uuid;
        private readonly Type[] mkTypes;
        private readonly DataInputOutputSerdeForge[] serdes;
        private string classPostfix;

        public MultiKeyClassRefUUIDBased(
            Type[] mkTypes,
            DataInputOutputSerdeForge[] serdes)
        {
            uuid = CodeGenerationIDGenerator.GenerateClassNameUUID();
            this.mkTypes = mkTypes;
            this.serdes = serdes;
        }

        public string GetClassNameMK(string classPostfix)
        {
            AssignPostfix(classPostfix);
            return CodeGenerationIDGenerator.GenerateClassNameWithUUID(typeof(HashableMultiKey), classPostfix, uuid);
        }

        public string GetClassNameMKSerde(string classPostfix)
        {
            return CodeGenerationIDGenerator.GenerateClassNameWithUUID(
                typeof(DataInputOutputSerde),
                classPostfix,
                uuid);
        }

        public CodegenExpression GetExprMKSerde(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            CheckClassPostfix();
            return NewInstanceInner(GetClassNameMKSerde(classPostfix));
        }

        public override string ToString()
        {
            return "MultiKeyClassRefUUIDBased{" +
                   "uuid='" +
                   uuid +
                   '\'' +
                   ", mkTypes=" +
                   mkTypes.RenderAny() +
                   ", classPostfix='" +
                   classPostfix +
                   '\'' +
                   '}';
        }

        public T Accept<T>(MultiKeyClassRefVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        private void CheckClassPostfix()
        {
            if (classPostfix == null) {
                throw new ArgumentException("Class postfix has not been assigned");
            }
        }

        private void AssignPostfix(string classPostfix)
        {
            if (this.classPostfix == null) {
                this.classPostfix = classPostfix;
                return;
            }

            if (!this.classPostfix.Equals(classPostfix)) {
                throw new ArgumentException("Invalid class postfix");
            }
        }

        public NameOrType ClassNameMK {
            get {
                CheckClassPostfix();
                return new NameOrType(GetClassNameMK(classPostfix));
            }
        }

        public Type[] MKTypes => mkTypes;

        public DataInputOutputSerdeForge[] Serdes => serdes;

        public DataInputOutputSerdeForge[] SerdeForges => serdes;
    }
} // end of namespace