///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    public class TableLookupIndexReqKey : CodegenMakeable
    {
        public TableLookupIndexReqKey(
            string indexName,
            string indexModuleName)
            : this(indexName, indexModuleName, null)
        {
        }

        public TableLookupIndexReqKey(
            string indexName,
            string indexModuleName,
            string tableName)
        {
            IndexName = indexName;
            IndexModuleName = indexModuleName;
            TableName = tableName;
        }

        public string IndexName { get; }

        public string TableName { get; }

        public string IndexModuleName { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenSymbolProvider symbols,
            CodegenClassScope classScope)
        {
            return NewInstance<TableLookupIndexReqKey>(
                Constant(IndexName),
                Constant(IndexModuleName),
                Constant(TableName));
        }

        public override string ToString()
        {
            if (TableName == null) {
                return IndexName;
            }

            return "table '" + TableName + "' index '" + IndexName + "'";
        }

        protected bool Equals(TableLookupIndexReqKey other)
        {
            return string.Equals(IndexName, other.IndexName) &&
                   string.Equals(TableName, other.TableName) &&
                   string.Equals(IndexModuleName, other.IndexModuleName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((TableLookupIndexReqKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = (IndexName != null ? IndexName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TableName != null ? TableName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (IndexModuleName != null ? IndexModuleName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
} // end of namespace