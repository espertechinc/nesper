///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.annotation
{
	/// <summary>
	/// Annotation for use in EPL statements to suppress any statement-level locking (use with caution, see below).
	/// <para />Caution: We provide this annotation for the purpose of identifing locking overhead,
	/// or when your application is single-threaded, or when using an external mechanism for concurreny control
	/// or for example with virtual data windows or plug-in data windows to allow customizing concurrency
	/// for application-provided data windows.
	/// Using this annotation may have unpredictable results unless your application is taking concurrency under consideration.
	/// </summary>
	public class NoLockAttribute : Attribute {
	}
} // end of namespace
