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

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationClassAssignment
    {
        public const string STATEMENT_FIELDS = "statementFields";

        private readonly int _offset;
        private readonly CodegenMemberCol _members;
        private readonly CodegenCtor _ctor;
        private readonly IList<ExprForge[]> _methodForges;
        private readonly IList<AggregationForgeFactory> _methodFactories;
        private readonly IList<AggregationStateFactoryForge> _accessStateFactories;
        private readonly IList<AggregationVColMethod> _vcolMethods = new List<AggregationVColMethod>(8);
        private readonly IList<AggregationVColAccess> _vcolAccess = new List<AggregationVColAccess>(8);
        private string _className;
        private string _memberName;

        public AggregationClassAssignment(
            int offset,
            Type forgeClass,
            CodegenClassScope classScope)
        {
            _offset = offset;
            _members = new CodegenMemberCol();
            _methodForges = new List<ExprForge[]>();
            _methodFactories = new List<AggregationForgeFactory>();
            _accessStateFactories = new List<AggregationStateFactoryForge>();
            _ctor = GetConstructor(forgeClass, classScope);
        }

        private CodegenCtor GetConstructor(
            Type forgeClass,
            CodegenClassScope classScope)
        {
            var namespaceScope = classScope.NamespaceScope;
            if (namespaceScope.FieldsClassNameOptional != null) {
                var ctorParams = new List<CodegenTypedParam>() {
                    new CodegenTypedParam(
                        classScope.NamespaceScope.FieldsClassNameOptional,
                        STATEMENT_FIELDS,
                        true,
                        false)
                };

                return new CodegenCtor(forgeClass, classScope, ctorParams);
            }

            return new CodegenCtor(forgeClass, classScope, EmptyList<CodegenTypedParam>.Instance);
        }

        public void AddMethod(AggregationVColMethod vcol)
        {
            _vcolMethods.Add(vcol);
        }

        public void AddAccess(AggregationVColAccess vcol)
        {
            _vcolAccess.Add(vcol);
        }

        public void Add(
            AggregationForgeFactory methodFactory,
            ExprForge[] forges)
        {
            _methodFactories.Add(methodFactory);
            _methodForges.Add(forges);
        }

        public void Add(AggregationStateFactoryForge factory)
        {
            _accessStateFactories.Add(factory);
        }

        public int Count => _members.MembersPerColumn.Count;

        public CodegenMemberCol Members => _members;

        public CodegenCtor Ctor => _ctor;

        public AggregationForgeFactory[] MethodFactories => _methodFactories.ToArray();

        public AggregationStateFactoryForge[] AccessStateFactories => _accessStateFactories.ToArray();

        public ExprForge[][] MethodForges => _methodForges.ToArray();

        public int MemberSize => _members.Members.Count;

        public string ClassName {
            get => _className;
            set => _className = value;
        }

        public string MemberName {
            get => _memberName;
            set => _memberName = value;
        }

        public int Offset => _offset;

        public IList<AggregationVColMethod> VcolMethods => _vcolMethods;

        public IList<AggregationVColAccess> VcolAccess => _vcolAccess;
    }
} // end of namespace