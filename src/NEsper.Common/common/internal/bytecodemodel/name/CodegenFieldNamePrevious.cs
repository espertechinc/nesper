///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;

namespace com.espertech.esper.common.@internal.bytecodemodel.name
{
    public class CodegenFieldNamePrevious : CodegenFieldName
    {
        private readonly int _streamNumber;

        public CodegenFieldNamePrevious(int streamNumber)
        {
            _streamNumber = streamNumber;
        }

        public string Name => CodegenNamespaceScopeNames.Previous(_streamNumber);

        public int StreamNumber => _streamNumber;

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var previous = (CodegenFieldNamePrevious)o;

            return _streamNumber == previous._streamNumber;
        }

        public override int GetHashCode()
        {
            return _streamNumber;
        }
    }
} // end of namespace