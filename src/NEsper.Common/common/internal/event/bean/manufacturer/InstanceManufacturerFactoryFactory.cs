///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    public class InstanceManufacturerFactoryFactory
    {
        public static InstanceManufacturerFactory GetManufacturer(
            Type targetClass,
            ImportServiceCompileTime importService,
            ExprNode[] childNodes)
        {
            var forgesUnmodified = ExprNodeUtilityQuery.GetForges(childNodes);
            var returnTypes = new object[forgesUnmodified.Length];
            for (var i = 0; i < forgesUnmodified.Length; i++) {
                returnTypes[i] = forgesUnmodified[i].EvaluationType;
            }

            var ctor = InstanceManufacturerUtil.GetManufacturer(
                targetClass,
                importService,
                forgesUnmodified,
                returnTypes);
            return new InstanceManufacturerFactoryFastCtor(targetClass, ctor.First, ctor.Second);
        }
    }
} // end of namespace