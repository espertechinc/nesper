///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.codegen.compile;

namespace com.espertech.esper.codegen.core
{
    public interface ICodegenContext
    {
        void AddMember(string memberName, Type clazz, Object @object);
        void AddMember(string memberName, Type clazz, Type optionalTypeParam, Object @object);
        void AddMember(ICodegenMember entry);
        ICodegenMember MakeMember(Type clazz, Object @object);
        ICodegenMember MakeAddMember(Type clazz, Object @object);
        ICodegenMember MakeMember(Type clazz, Type optionalTypeParam, Object @object);
        ICodegenBlock AddMethod(Type returnType, Type paramType, string paramName, Type generator);
        ICodegenBlock AddMethod(Type returnType, Type generator);
        IList<ICodegenMember> Members { get; }
        IList<ICodegenMethod> Methods { get; }
        bool IsDebugEnabled { get; }

        ICodegenCompiler Compiler { get; }
    }
} // end of namespace
