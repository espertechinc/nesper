///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.compile
{
    public class IndexCompileTimeKey
    {
        public IndexCompileTimeKey(
            string infraModuleName,
            string infraName,
            NameAccessModifier visibility,
            bool namedWindow,
            string indexName,
            string indexModuleName)
        {
            InfraModuleName = infraModuleName;
            InfraName = infraName;
            Visibility = visibility;
            IsNamedWindow = namedWindow;
            IndexName = indexName;
            IndexModuleName = indexModuleName;
        }

        public string InfraModuleName { get; }

        public string InfraName { get; }

        public NameAccessModifier Visibility { get; }

        public bool IsNamedWindow { get; }

        public string IndexName { get; }

        public string IndexModuleName { get; }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            return NewInstance<IndexCompileTimeKey>(
                Constant(InfraModuleName),
                Constant(InfraName),
                Constant(Visibility),
                Constant(IsNamedWindow),
                Constant(IndexName),
                Constant(IndexModuleName));
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (IndexCompileTimeKey)o;

            if (IsNamedWindow != that.IsNamedWindow) {
                return false;
            }

            if (!InfraModuleName?.Equals(that.InfraModuleName) ?? that.InfraModuleName != null) {
                return false;
            }

            if (!InfraName.Equals(that.InfraName)) {
                return false;
            }

            if (Visibility != that.Visibility) {
                return false;
            }

            if (!IndexName.Equals(that.IndexName)) {
                return false;
            }

            return IndexModuleName?.Equals(that.IndexModuleName) ?? that.IndexModuleName == null;
        }

        public override int GetHashCode()
        {
            var result = InfraModuleName != null ? InfraModuleName.GetHashCode() : 0;
            result = 31 * result + InfraName.GetHashCode();
            result = 31 * result + Visibility.GetHashCode();
            result = 31 * result + (IsNamedWindow ? 1 : 0);
            result = 31 * result + IndexName.GetHashCode();
            result = 31 * result + (IndexModuleName != null ? IndexModuleName.GetHashCode() : 0);
            return result;
        }
    }
} // end of namespace