///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.funcs
{
    /// <summary>
    /// Represents the INSTANCEOF(a,b,...) function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprInstanceofNode : ExprNodeBase, ExprEvaluator
    {
        private readonly String[] _classIdentifiers;

        private Type[] _classes;
        private readonly CopyOnWriteList<Pair<Type, Boolean>> _resultCache = new CopyOnWriteList<Pair<Type, Boolean>>();
        [NonSerialized]
        private ExprEvaluator _evaluator;

        private readonly ILockable _oLock;

        public ExprInstanceofNode(string[] classIdentifiers, IContainer container)
            : this(classIdentifiers, container.Resolve<ILockManager>())
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="classIdentifiers">is a list of type names to check type for</param>
        /// <param name="lockManager">The lock manager.</param>
        public ExprInstanceofNode(string[] classIdentifiers, ILockManager lockManager)
        {
            _oLock = lockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _classIdentifiers = classIdentifiers;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count != 1)
            {
                throw new ExprValidationException("Instanceof node must have 1 child expression node supplying the expression to test");
            }
            if ((_classIdentifiers == null) || (_classIdentifiers.Length == 0))
            {
                throw new ExprValidationException("Instanceof node must have 1 or more class identifiers to verify type against");
            }

            _evaluator = ChildNodes[0].ExprEvaluator;

            var classList = GetClassSet(_classIdentifiers, validationContext.EngineImportService);
            using (_oLock.Acquire())
            {
                _classes = classList.ToArray();
            }

            return null;
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public Type ReturnType
        {
            get { return typeof(bool?); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprInstanceof(this); }

            var result = _evaluator.Evaluate(evaluateParams);
            if (result == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprInstanceof(false); }
                return false;
            }

            // return cached value
            foreach (var pair in _resultCache)
            {
                if (pair.First == result.GetType())
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprInstanceof(pair.Second); }
                    return pair.Second;
                }
            }

            var @out = CheckAddType(result.GetType());
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprInstanceof(@out); }
            return @out;
        }

        // Checks type and adds to cache
        private bool? CheckAddType(Type type)
        {
            using (_oLock.Acquire())
            {
                // check again in synchronized block
                foreach (Pair<Type, Boolean> pair in _resultCache)
                {
                    if (pair.First == type)
                    {
                        return pair.Second;
                    }
                }

                // get the types superclasses and interfaces, and their superclasses and interfaces
                ICollection<Type> classesToCheck = new HashSet<Type>();
                TypeHelper.GetBase(type, classesToCheck);
                classesToCheck.Add(type);

                // check type against each class
                bool fits = false;
                foreach (Type clazz in _classes)
                {
                    if (classesToCheck.Contains(clazz))
                    {
                        fits = true;
                        break;
                    }
                }

                _resultCache.Add(new Pair<Type, Boolean>(type, fits));
                return fits;
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("instanceof(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(",");

            String delimiter = "";
            for (int i = 0; i < _classIdentifiers.Length; i++)
            {
                writer.Write(delimiter);
                writer.Write(_classIdentifiers[i]);
                delimiter = ",";
            }
            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var other = node as ExprInstanceofNode;
            return other != null && Collections.AreEqual(other._classIdentifiers, _classIdentifiers);
        }

        /// <summary>Returns the list of class names or types to check instance of. </summary>
        /// <value>class names</value>
        public string[] ClassIdentifiers
        {
            get { return _classIdentifiers; }
        }

        private ICollection<Type> GetClassSet(string[] classIdentifiers, EngineImportService engineImportService)
        {
            var classList = new HashSet<Type>();
            foreach (String className in classIdentifiers)
            {
                // try the primitive names including "string"
                var clazz = TypeHelper.GetPrimitiveTypeForName(className.Trim());
                if (clazz != null)
                {
                    classList.Add(clazz);
                    classList.Add(clazz.GetBoxedType());
                    continue;
                }

                // try to look up the class, not a primitive type name
                try
                {
                    clazz = TypeHelper.GetClassForName(className.Trim(), engineImportService.GetClassForNameProvider());
                }
                catch (TypeLoadException e)
                {
                    throw new ExprValidationException("Class as listed in is function by name '" + className + "' cannot be loaded", e);
                }

                // Add primitive and boxed types, or type itself if not built-in
                classList.Add(clazz.GetPrimitiveType());
                classList.Add(clazz.GetBoxedType());
            }
            return classList;
        }
    }
}
