///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.module
{
    public class ModuleIndexMeta
    {
        public static readonly ModuleIndexMeta[] EMPTY_ARRAY = new ModuleIndexMeta[0];

        public ModuleIndexMeta(
            bool namedWindow,
            string infraName,
            string infraModuleName,
            string indexName,
            string indexModuleName)
        {
            IsNamedWindow = namedWindow;
            InfraName = infraName;
            InfraModuleName = infraModuleName;
            IndexName = indexName;
            IndexModuleName = indexModuleName;
        }

        public bool IsNamedWindow { get; }

        public string InfraName { get; }

        public string IndexName { get; }

        public string InfraModuleName { get; }

        public string IndexModuleName { get; }

        public static CodegenExpression MakeArray(ICollection<ModuleIndexMeta> names)
        {
            if (names.IsEmpty()) {
                return EnumValue(typeof(ModuleIndexMeta), "EMPTY_ARRAY");
            }

            var expressions = new CodegenExpression[names.Count];
            var count = 0;
            foreach (var entry in names) {
                expressions[count++] = entry.Make();
            }

            return NewArrayWithInit(typeof(ModuleIndexMeta), expressions);
        }

        private CodegenExpression Make()
        {
            return NewInstance(
                typeof(ModuleIndexMeta), Constant(IsNamedWindow), Constant(InfraName), Constant(InfraModuleName),
                Constant(IndexName), Constant(IndexModuleName));
        }

        public static ModuleIndexMeta[] ToArray(ISet<ModuleIndexMeta> moduleIndexes)
        {
            if (moduleIndexes.IsEmpty()) {
                return EMPTY_ARRAY;
            }

            return moduleIndexes.ToArray();
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ModuleIndexMeta) o;

            if (IsNamedWindow != that.IsNamedWindow) {
                return false;
            }

            if (!InfraName?.Equals(that.InfraName) ?? that.InfraName != null) {
                return false;
            }

            if (!InfraModuleName?.Equals(that.InfraModuleName) ?? that.InfraModuleName != null) {
                return false;
            }

            if (!IndexName?.Equals(that.IndexName) ?? that.IndexName != null) {
                return false;
            }

            return IndexModuleName?.Equals(that.IndexModuleName) ?? that.IndexModuleName == null;
        }

        public override int GetHashCode()
        {
            var result = IsNamedWindow ? 1 : 0;
            result = 31 * result + (InfraName != null ? InfraName.GetHashCode() : 0);
            result = 31 * result + (InfraModuleName != null ? InfraModuleName.GetHashCode() : 0);
            result = 31 * result + (IndexName != null ? IndexName.GetHashCode() : 0);
            result = 31 * result + (IndexModuleName != null ? IndexModuleName.GetHashCode() : 0);
            return result;
        }
    }
} // end of namespace