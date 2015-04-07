///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;

using XLR8.CGLib;

namespace com.espertech.esper.events.bean
{
    public class InstanceManufacturerFactory
    {
        public static InstanceManufacturer GetManufacturer(
            Type targetClass,
            EngineImportService engineImportService,
            ExprNode[] childNodes)
        {
            var evalsUnmodified = ExprNodeUtility.GetEvaluators(childNodes);
            var returnTypes = new object[evalsUnmodified.Length];
            for (int i = 0; i < evalsUnmodified.Length; i++)
            {
                returnTypes[i] = evalsUnmodified[i].ReturnType;
            }

            Pair<FastConstructor, ExprEvaluator[]> ctor = InstanceManufacturerUtil.GetManufacturer(
                targetClass, engineImportService, evalsUnmodified, returnTypes);
            return new InstanceManufacturerFastCtor(targetClass, ctor.First, ctor.Second);
        }
    }
} // end of namespace