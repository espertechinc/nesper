///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.codegen.core
{
    public interface ICodegenMethod
    {
        Type ReturnType { get; }
        string MethodName { get; }
        IList<CodegenNamedParam> Parameters { get; }
        void MergeClasses(ICollection<Type> classes);
        ICodegenBlock Statements { get; }
        void Render(TextWriter textWriter, bool isPublic);
    }
} // end of namespace