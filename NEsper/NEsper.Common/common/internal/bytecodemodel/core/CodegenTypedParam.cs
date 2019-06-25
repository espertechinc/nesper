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

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenTypedParam
    {
        private readonly string typeName;
        private readonly Type type;
        private readonly string name;
        private readonly bool memberWhenCtorParam;
        private readonly bool isPublic;
        private bool isFinal = true;
        private bool isStatic = false;

        public CodegenTypedParam(
            string typeName,
            Type type,
            string name,
            bool memberWhenCtorParam,
            bool isPublic)
        {
            if (type == null && typeName == null)
            {
                throw new ArgumentException("Invalid null type");
            }

            this.typeName = typeName;
            this.type = type;
            this.name = name;
            this.memberWhenCtorParam = memberWhenCtorParam;
            this.isPublic = isPublic;
        }

        public CodegenTypedParam(
            string typeName,
            Type type,
            string name)
            : this(typeName, type, name, true, false)
        {
        }

        public CodegenTypedParam(
            Type type,
            string name)
            : this(null, type, name)
        {
        }

        public CodegenTypedParam(
            Type type,
            string name,
            bool memberWhenCtorParam)
            : this(null, type, name, memberWhenCtorParam, false)
        {
        }

        public CodegenTypedParam(
            Type type,
            string name,
            bool memberWhenCtorParam,
            bool isPublic)
            : this(null, type, name, memberWhenCtorParam, isPublic)
        {
        }

        public CodegenTypedParam(
            string typeName,
            string name,
            bool memberWhenCtorParam,
            bool isPublic)
            : this(typeName, null, name, memberWhenCtorParam, isPublic)
        {
        }

        public CodegenTypedParam(
            string type,
            string name)
            : this(type, null, name)
        {
        }

        public CodegenTypedParam WithFinal(bool aFinal)
        {
            isFinal = aFinal;
            return this;
        }

        public CodegenTypedParam WithStatic(bool aStatic)
        {
            isStatic = aStatic;
            return this;
        }

        public string Name
        {
            get => name;
        }

        public void RenderAsParameter(
            StringBuilder builder,
            IDictionary<Type, string> imports)
        {
            if (type != null)
            {
                AppendClassName(builder, type, null, imports);
            }
            else
            {
                builder.Append(typeName);
            }

            builder.Append(" ").Append(name);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            if (type != null)
            {
                classes.Add(type);
            }
        }

        public void RenderAsMember(
            StringBuilder builder,
            IDictionary<Type, string> imports)
        {
            if (type != null)
            {
                AppendClassName(builder, type, null, imports);
            }
            else
            {
                builder.Append(typeName);
            }

            builder.Append(" ").Append(name);
        }

        public void RenderType(
            StringBuilder builder,
            IDictionary<Type, string> imports)
        {
            if (type != null)
            {
                AppendClassName(builder, type, null, imports);
            }
            else
            {
                builder.Append(typeName);
            }
        }

        public bool IsMemberWhenCtorParam
        {
            get => memberWhenCtorParam;
        }

        public bool IsPublic
        {
            get => isPublic;
        }

        public bool IsFinal
        {
            get => isFinal;
            set => isFinal = value;
        }

        public bool IsStatic
        {
            get => isStatic;
            set => isStatic = value;
        }

        public override string ToString()
        {
            return "CodegenTypedParam{" +
                   "typeName='" + typeName + '\'' +
                   ", type=" + type +
                   ", name='" + name + '\'' +
                   ", memberWhenCtorParam=" + memberWhenCtorParam +
                   '}';
        }
    }
} // end of namespace