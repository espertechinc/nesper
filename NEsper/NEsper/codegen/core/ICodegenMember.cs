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
    public interface ICodegenMember
    {
        Type MemberType { get; }
        Type OptionalTypeParam { get; }
        string MemberName { get; }
        object Value { get; }

        void MergeClasses(ICollection<Type> classes);
    }
} // end of namespace