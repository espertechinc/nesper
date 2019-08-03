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

using com.espertech.esper.common.@internal.bytecodemodel.util;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenTypedParam
    {
        private readonly string _typeName;
        private readonly Type _type;
        private readonly string _name;
        private readonly bool _memberWhenCtorParam;
        private readonly bool _isPublic;
        private bool _isReadonly = true;
        private bool _isStatic = false;

        public CodegenTypedParam(
            string typeName,
            Type type,
            string name,
            bool memberWhenCtorParam,
            bool isPublic)
        {
            if (type == null && typeName == null) {
                throw new ArgumentException("Invalid null type");
            }

            this._typeName = typeName;
            this._type = type;
            this._name = name;
            this._memberWhenCtorParam = memberWhenCtorParam;
            this._isPublic = isPublic;
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
            _isReadonly = aFinal;
            return this;
        }

        public CodegenTypedParam WithStatic(bool aStatic)
        {
            _isStatic = aStatic;
            return this;
        }

        public string Name {
            get => _name;
        }

        public void RenderAsParameter(StringBuilder builder)
        {
            if (_type != null) {
                AppendClassName(builder, _type);
            }
            else {
                builder.Append(_typeName);
            }

            builder.Append(" ").Append(_name);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            if (_type != null) {
                classes.AddToSet(_type);
            }
        }

        public void RenderAsMember(
            StringBuilder builder)
        {
            if (_type != null) {
                AppendClassName(builder, _type);
            }
            else {
                builder.Append(_typeName);
            }

            builder.Append(" ").Append(_name);
        }

        public void RenderType(
            StringBuilder builder)
        {
            if (_type != null) {
                AppendClassName(builder, _type);
            }
            else {
                builder.Append(_typeName);
            }
        }

        public bool IsMemberWhenCtorParam {
            get => _memberWhenCtorParam;
        }

        public bool IsPublic {
            get => _isPublic;
        }

        public bool IsReadonly {
            get => _isReadonly;
            set => _isReadonly = value;
        }

        public bool IsStatic {
            get => _isStatic;
            set => _isStatic = value;
        }

        public override string ToString()
        {
            return "CodegenTypedParam{" +
                   "typeName='" +
                   _typeName +
                   '\'' +
                   ", type=" +
                   _type +
                   ", name='" +
                   _name +
                   '\'' +
                   ", memberWhenCtorParam=" +
                   _memberWhenCtorParam +
                   '}';
        }
    }
} // end of namespace