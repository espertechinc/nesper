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
    public interface ICodegenClass
    {
        string Namespace { get; }
        string ClassName { get; }
        Type InterfaceImplemented { get; }
        IList<ICodegenMember> Members { get; }
        IList<ICodegenMethod> PublicMethods { get; }
        IList<ICodegenMethod> PrivateMethods { get; }

        ICollection<Type> GetReferencedClasses();
    }
}
