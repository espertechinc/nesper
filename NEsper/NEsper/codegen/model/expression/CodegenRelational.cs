///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.codegen.model.expression
{
    public enum CodegenRelational
    {
        GE,
        GT,
        LE,
        LT
    }

    public static class CodegenRelationalExtensions
    {
        public static string GetOp(this CodegenRelational value)
        {
            switch(value)
            {
                case CodegenRelational.GE:
                    return ">=";
                case CodegenRelational.GT:
                    return ">";
                case CodegenRelational.LE:
                    return "<=";
                case CodegenRelational.LT:
                    return "<";
                default:
                    throw new ArgumentException("invalid value");
            }
        }
    }
}
