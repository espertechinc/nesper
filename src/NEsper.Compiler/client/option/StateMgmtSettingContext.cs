///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.statemgmtsettings;

namespace com.espertech.esper.compiler.client.option
{
	/// <summary>
	///     For internal-use-only and subject-to-change-between-versions: Provides the environment to
	///     <seealso cref="StateMgmtSettingOption" />.
	/// </summary>
	public class StateMgmtSettingContext : StatementOptionContextBase
    {
	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="raw">statement info</param>
	    /// <param name="appliesTo">applies</param>
	    /// <param name="configured">config</param>
	    public StateMgmtSettingContext(
            StatementRawInfo raw,
            AppliesTo appliesTo,
            StateMgmtSettingBucket configured) : base(raw)
        {
            AppliesTo = appliesTo;
            Configured = configured;
        }

	    /// <summary>
	    ///     For internal-use-only and subject-to-change-between-versions: Returns applies-to
	    /// </summary>
	    /// <value>applies-to</value>
	    public AppliesTo AppliesTo { get; }

	    /// <summary>
	    ///     For internal-use-only and subject-to-change-between-versions: Returns settings
	    /// </summary>
	    /// <value>settings</value>
	    public StateMgmtSettingBucket Configured { get; }
    }
} // end of namespace