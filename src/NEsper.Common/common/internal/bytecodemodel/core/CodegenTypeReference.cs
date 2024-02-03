///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenTypeReference
    {
        private readonly Type _type;
        private readonly string _typeName;

        public Type Type => _type;

        public string TypeName => _typeName;

        public CodegenTypeReference(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _typeName = null;
        }

        public CodegenTypeReference(string typeName)
        {
            _typeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            _type = null;
        }

        public void AddReferenced(ISet<Type> classes)
        {
            if (_type != null) {
                // TBR: clazz.traverseClasses(classes::add);
                classes.Add(_type);
            }
        }

        public void Render(StringBuilder builder)
        {
            if (_type != null) {
                CodeGenerationHelper.AppendClassName(builder, _type);
            }
            else {
                builder.Append(_typeName);
            }
        }

        protected bool Equals(CodegenTypeReference other)
        {
            if (_type != null) {
                return other._type != null
                    ? _type == other._type
                    : Equals(_type.FullName, other._typeName);
            }

            return Equals(
                _typeName,
                other._type != null
                    ? other._type.FullName
                    : other._typeName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((CodegenTypeReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((_type != null ? _type.GetHashCode() : 0) * 397) ^
                       (_typeName != null ? _typeName.GetHashCode() : 0);
            }
        }
    }
}