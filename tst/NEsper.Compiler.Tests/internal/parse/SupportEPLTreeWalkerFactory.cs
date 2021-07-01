///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Antlr4.Runtime;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

namespace com.espertech.esper.compiler.@internal.parse
{
	public class SupportEPLTreeWalkerFactory
	{
		public static EPLTreeWalkerListener MakeWalker(
			CommonTokenStream tokenStream,
			ImportServiceCompileTime engineImportService)
		{
			StatementSpecMapEnv mapEnv = SupportStatementSpecMapEnv.Make(engineImportService);
			return new EPLTreeWalkerListener(
				tokenStream,
				SelectClauseStreamSelectorEnum.ISTREAM_ONLY,
				EmptyList<string>.Instance,
				EmptyList<string>.Instance,
				mapEnv);
		}

		public static EPLTreeWalkerListener MakeWalker(
			IContainer container, 
			CommonTokenStream tokenStream)
		{
			return MakeWalker(tokenStream, SupportClasspathImport.GetInstance(container));
		}
	}
} // end of namespace
