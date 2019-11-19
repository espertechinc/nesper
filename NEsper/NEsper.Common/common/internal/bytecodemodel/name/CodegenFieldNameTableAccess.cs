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
    public class CodegenFieldNameTableAccess : CodegenFieldName
    {
        private readonly int _tableAccessNumber;

        public CodegenFieldNameTableAccess(int tableAccessNumber)
        {
            this._tableAccessNumber = tableAccessNumber;
        }

        public string Name {
            get => CodegenNamespaceScopeNames.TableAccessResultFuture(_tableAccessNumber);
        }

        public int TableAccessNumber {
            get => _tableAccessNumber;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            CodegenFieldNameTableAccess that = (CodegenFieldNameTableAccess) o;

            return _tableAccessNumber == that._tableAccessNumber;
        }

        public override int GetHashCode()
        {
            return _tableAccessNumber;
        }
    }
} // end of namespace