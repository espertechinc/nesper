///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private readonly string infraModuleName;
        private readonly string infraName;
        private readonly NameAccessModifier visibility;
        private readonly bool namedWindow;
        private readonly string indexName;
        private readonly string indexModuleName;

        public IndexCompileTimeKey(string infraModuleName, string infraName, NameAccessModifier visibility, bool namedWindow, string indexName, string indexModuleName)
        {
            this.infraModuleName = infraModuleName;
            this.infraName = infraName;
            this.visibility = visibility;
            this.namedWindow = namedWindow;
            this.indexName = indexName;
            this.indexModuleName = indexModuleName;
        }

        public string InfraModuleName
        {
            get => infraModuleName;
        }

        public string InfraName
        {
            get => infraName;
        }

        public NameAccessModifier Visibility
        {
            get => visibility;
        }

        public bool IsNamedWindow()
        {
            return namedWindow;
        }

        public string IndexName
        {
            get => indexName;
        }

        public string IndexModuleName
        {
            get => indexModuleName;
        }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            return NewInstance(typeof(IndexCompileTimeKey), Constant(infraModuleName), Constant(infraName), Constant(visibility), Constant(namedWindow), Constant(indexName), Constant(indexModuleName));
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            IndexCompileTimeKey that = (IndexCompileTimeKey)o;

            if (namedWindow != that.namedWindow) return false;
            if (infraModuleName != null ? !infraModuleName.Equals(that.infraModuleName) : that.infraModuleName != null)
                return false;
            if (!infraName.Equals(that.infraName)) return false;
            if (visibility != that.visibility) return false;
            if (!indexName.Equals(that.indexName)) return false;
            return indexModuleName != null ? indexModuleName.Equals(that.indexModuleName) : that.indexModuleName == null;
        }

        public override int GetHashCode()
        {
            int result = infraModuleName != null ? infraModuleName.GetHashCode() : 0;
            result = 31 * result + infraName.GetHashCode();
            result = 31 * result + visibility.GetHashCode();
            result = 31 * result + (namedWindow ? 1 : 0);
            result = 31 * result + indexName.GetHashCode();
            result = 31 * result + (indexModuleName != null ? indexModuleName.GetHashCode() : 0);
            return result;
        }
    }
} // end of namespace