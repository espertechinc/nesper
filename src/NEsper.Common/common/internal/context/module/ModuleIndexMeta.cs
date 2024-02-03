///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.module
{
    public class ModuleIndexMeta
    {
        public static readonly ModuleIndexMeta[] EMPTY_ARRAY = Array.Empty<ModuleIndexMeta>();
        
        private readonly bool namedWindow;
        private readonly string infraName;
        private readonly string infraModuleName;
        private readonly string indexName;
        private readonly string indexModuleName;

        public ModuleIndexMeta(
            bool namedWindow,
            string infraName,
            string infraModuleName,
            string indexName,
            string indexModuleName)
        {
            this.namedWindow = namedWindow;
            this.infraName = infraName;
            this.infraModuleName = infraModuleName;
            this.indexName = indexName;
            this.indexModuleName = indexModuleName;
        }

        public bool IsNamedWindow => namedWindow;

        public static CodegenExpression MakeArrayNullIfEmpty(ICollection<ModuleIndexMeta> names)
        {
            if (names.IsEmpty()) {
                return ConstantNull();
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
                typeof(ModuleIndexMeta),
                Constant(namedWindow),
                Constant(infraName),
                Constant(infraModuleName),
                Constant(indexName),
                Constant(indexModuleName));
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

            var that = (ModuleIndexMeta)o;
            if (namedWindow != that.namedWindow) {
                return false;
            }

            if (!infraName?.Equals(that.infraName) ?? that.infraName != null) {
                return false;
            }

            if (!infraModuleName?.Equals(that.infraModuleName) ?? that.infraModuleName != null) {
                return false;
            }

            if (!indexName?.Equals(that.indexName) ?? that.indexName != null) {
                return false;
            }

            return indexModuleName?.Equals(that.indexModuleName) ?? that.indexModuleName == null;
        }

        public override int GetHashCode()
        {
            var result = namedWindow ? 1 : 0;
            result = 31 * result + (infraName != null ? infraName.GetHashCode() : 0);
            result = 31 * result + (infraModuleName != null ? infraModuleName.GetHashCode() : 0);
            result = 31 * result + (indexName != null ? indexName.GetHashCode() : 0);
            result = 31 * result + (indexModuleName != null ? indexModuleName.GetHashCode() : 0);
            return result;
        }

        public string InfraName => infraName;

        public string IndexName => indexName;

        public string InfraModuleName => infraModuleName;

        public string IndexModuleName => indexModuleName;
    }
} // end of namespace