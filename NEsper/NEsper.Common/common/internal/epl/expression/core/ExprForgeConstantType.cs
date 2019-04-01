///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprForgeConstantType
    {
        public static readonly ExprForgeConstantType COMPILETIMECONST = new ExprForgeConstantType();
        public static readonly ExprForgeConstantType DEPLOYCONST = new ExprForgeConstantType();
        public static readonly ExprForgeConstantType NONCONST = new ExprForgeConstantType();

        public static readonly ExprForgeConstantType[] VALUES = {
            COMPILETIMECONST,
            DEPLOYCONST,
            NONCONST
        };

        private ExprForgeConstantType()
        {
        }

        public bool IsCompileTimeConstant => this == COMPILETIMECONST;

        public bool IsDeployTimeTimeConstant => this == DEPLOYCONST;

        public bool IsConstant => this == COMPILETIMECONST || this == DEPLOYCONST;
    }
} // end of namespace