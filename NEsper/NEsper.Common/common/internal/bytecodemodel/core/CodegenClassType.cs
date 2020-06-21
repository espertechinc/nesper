///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public enum CodegenClassType
    {
        APPDECLARED,
        KEYPROVISIONING,
        KEYPROVISIONINGSERDE,
        STATEMENTFIELDS,
        JSONNESTEDCLASSDELEGATEANDFACTORY,
        JSONEVENT,
        JSONDELEGATE,
        JSONDELEGATEFACTORY,
        EVENTSERDE,
        RESULTSETPROCESSORFACTORYPROVIDER,
        OUTPUTPROCESSVIEWFACTORYPROVIDER,
        STATEMENTAIFACTORYPROVIDER,
        STATEMENTPROVIDER,
        FAFQUERYMETHODPROVIDER,
        FAFPROVIDER,
        MODULEPROVIDER
    }

    public static class CodegenClassTypeExtensions
    {
        public static int GetSortCode(this CodegenClassType value)
        {
            switch (value) {
                    case CodegenClassType.APPDECLARED: return 5;
                    case CodegenClassType.KEYPROVISIONING: return 10;
                    case CodegenClassType.KEYPROVISIONINGSERDE: return 20;
                    case CodegenClassType.STATEMENTFIELDS: return 30;
                    case CodegenClassType.JSONNESTEDCLASSDELEGATEANDFACTORY: return 40;
                    case CodegenClassType.JSONEVENT: return 42;
                    case CodegenClassType.JSONDELEGATE: return 43;
                    case CodegenClassType.JSONDELEGATEFACTORY: return 44;
                    case CodegenClassType.EVENTSERDE: return 50;
                    case CodegenClassType.RESULTSETPROCESSORFACTORYPROVIDER: return 60;
                    case CodegenClassType.OUTPUTPROCESSVIEWFACTORYPROVIDER: return 70;
                    case CodegenClassType.STATEMENTAIFACTORYPROVIDER: return 80;
                    case CodegenClassType.STATEMENTPROVIDER: return 90;
                    case CodegenClassType.FAFQUERYMETHODPROVIDER: return 100;
                    case CodegenClassType.FAFPROVIDER: return 110;
                    case CodegenClassType.MODULEPROVIDER: return 120;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}