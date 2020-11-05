///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;

namespace com.espertech.esper.common.@internal.bytecodemodel.name
{
    public class CodegenFieldNameMatchRecognizeAgg : CodegenFieldName
    {
        private readonly int _aggregationNumber;

        public CodegenFieldNameMatchRecognizeAgg(int aggregationNumber)
        {
            this._aggregationNumber = aggregationNumber;
        }

        public string Name {
            get => CodegenNamespaceScopeNames.AggregationMatchRecognize(_aggregationNumber);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            CodegenFieldNameMatchRecognizeAgg that = (CodegenFieldNameMatchRecognizeAgg) o;

            return _aggregationNumber == that._aggregationNumber;
        }

        public override int GetHashCode()
        {
            return _aggregationNumber;
        }
    }
} // end of namespace