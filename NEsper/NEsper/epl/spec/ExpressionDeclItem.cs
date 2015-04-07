///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ExpressionDeclItem
    {
        public ExpressionDeclItem(string name, IList<string> parametersNames, ExprNode inner, bool isAlias)
        {
            Name = name;
            ParametersNames = parametersNames;
            Inner = inner;
            IsAlias = isAlias;
        }

        public String Name { get; private set; }

        public ExprNode Inner { get; private set; }

        public IList<string> ParametersNames { get; private set; }

        public bool IsAlias { get; set; }
    }
}