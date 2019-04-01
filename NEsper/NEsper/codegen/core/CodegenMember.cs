///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.codegen.core
{
    public class CodegenMember : ICodegenMember
    {
        private readonly string _memberName;
        private readonly Type _memberType;
        private readonly Type _optionalTypeParam;
        private readonly object _value;

        internal CodegenMember(string memberName, Type clazz, object value)
        {
            this._memberName = memberName;
            this._memberType = clazz;
            this._optionalTypeParam = null;
            this._value = value;
        }

        internal CodegenMember(string memberName, Type clazz, Type optionalTypeParam, object value)
        {
            this._memberName = memberName;
            this._memberType = clazz;
            this._optionalTypeParam = optionalTypeParam;
            this._value = value;
        }

        public Type MemberType => _memberType;

        public Type OptionalTypeParam => _optionalTypeParam;

        public string MemberName => _memberName;

        public object Value => _value;

        public bool Equals(CodegenMember other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._memberName == _memberName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(CodegenMember)) return false;
            return Equals((CodegenMember)obj);
        }

        public override int GetHashCode()
        {
            return _memberName.GetHashCode();
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_memberType);
            if (_optionalTypeParam != null)
            {
                classes.Add(_optionalTypeParam);
            }
        }
    }
} // end of namespace