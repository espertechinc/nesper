///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esper.core.deploy
{
    /// <summary>
    /// Deployment administrative implementation.
    /// </summary>
    public class EPDeploymentAdminImpl : EPDeploymentAdminSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly EPAdministratorSPI _epService;
        private readonly DeploymentStateService _deploymentStateService;
        private readonly StatementEventTypeRef _statementEventTypeRef;
        private readonly EventAdapterService _eventAdapterService;
        private readonly StatementIsolationService _statementIsolationService;
        private readonly StatementIdGenerator _optionalStatementIdGenerator;
        private readonly FilterService _filterService;

        private readonly ILockable _iLock = LockManager.CreateDefaultLock();

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="epService">administrative SPI</param>
        /// <param name="deploymentStateService">deployment state maintenance service</param>
        /// <param name="statementEventTypeRef">maintains statement-eventtype relationship</param>
        /// <param name="eventAdapterService">event wrap service</param>
        /// <param name="statementIsolationService">for isolated statement execution</param>
        /// <param name="optionalStatementIdGenerator">The optional statement id generator.</param>
        /// <param name="filterService">The filter service.</param>
        public EPDeploymentAdminImpl(EPAdministratorSPI epService,
                                     DeploymentStateService deploymentStateService,
                                     StatementEventTypeRef statementEventTypeRef,
                                     EventAdapterService eventAdapterService,
                                     StatementIsolationService statementIsolationService,
                                     StatementIdGenerator optionalStatementIdGenerator,
                                     FilterService filterService)
        {
            _epService = epService;
            _deploymentStateService = deploymentStateService;
            _statementEventTypeRef = statementEventTypeRef;
            _eventAdapterService = eventAdapterService;
            _statementIsolationService = statementIsolationService;
            _optionalStatementIdGenerator = optionalStatementIdGenerator;
            _filterService = filterService;
        }
    
        public Module Read(Stream stream, String uri)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("Reading module from input stream");
            }
            return EPLModuleUtil.ReadInternal(stream, uri);
        }
    
        public Module Read(FileInfo file)
        {
            var absolutePath = Path.GetFullPath(file.Name);

            Log.Debug("Reading resource '{0}'", absolutePath);

            using (var stream = File.OpenRead(absolutePath))
            {
                return EPLModuleUtil.ReadInternal(stream, absolutePath);
            }
        }

        public Module Read(Uri url)
        {
            Log.Debug("Reading resource from url: {0}", url);

            using (var webClient = new WebClient())
            {
                using (var stream = webClient.OpenRead(url))
                {
                    return EPLModuleUtil.ReadInternal(stream, url.ToString());
                }
            }
        }
    
        public Module Read(String resource)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("Reading resource '" + resource + "'");
            }
            return EPLModuleUtil.ReadResource(resource);
        }
    
        public DeploymentResult Deploy(Module module, DeploymentOptions options, String assignedDeploymentId)
        {
            using(_iLock.Acquire())
            {
                if (_deploymentStateService.GetDeployment(assignedDeploymentId) != null)
                {
                    throw new ArgumentException("Assigned deployment id '" + assignedDeploymentId +
                                                "' is already in use");
                }
                return DeployInternal(module, options, assignedDeploymentId, DateTime.Now);
            }
        }
    
        public DeploymentResult Deploy(Module module, DeploymentOptions options)
        {
            using(_iLock.Acquire())
            {
                var deploymentId = _deploymentStateService.NextDeploymentId;
                return DeployInternal(module, options, deploymentId, DateTime.Now);
            }
        }
    
        private DeploymentResult DeployInternal(Module module, DeploymentOptions options, String deploymentId, DateTime addedDate)
        {
            if (options == null) {
                options = new DeploymentOptions();
            }
    
            if (Log.IsDebugEnabled) {
                Log.Debug("Deploying module " + module);
            }
            IList<String> imports;
            if (module.Imports != null) {
                foreach (var imported in module.Imports) {
                    if (Log.IsDebugEnabled) {
                        Log.Debug("Adding import " + imported);
                    }
                    _epService.Configuration.AddImport(imported);
                }
                imports = new List<String>(module.Imports);
            }
            else {
                 imports = Collections.GetEmptyList<string>();
            }
    
            if (options.IsCompile) {
                var itemExceptions = new List<DeploymentItemException>();
                foreach (var item in module.Items) {
                    if (item.IsCommentOnly) {
                        continue;
                    }
    
                    try {
                        _epService.CompileEPL(item.Expression);
                    }
                    catch (Exception ex) {
                        itemExceptions.Add(new DeploymentItemException(ex.Message, item.Expression, ex, item.LineNumber));
                    }
                }
    
                if (itemExceptions.IsNotEmpty()) {
                    throw BuildException("Compilation failed", module, itemExceptions);
                }
            }
    
            if (options.IsCompileOnly) {
                return null;
            }
    
            var exceptions = new List<DeploymentItemException>();
            var statementNames = new List<DeploymentInformationItem>();
            var statements = new List<EPStatement>();
            var eventTypesReferenced = new HashSet<String>();
    
            foreach (var item in module.Items) {
                if (item.IsCommentOnly) {
                    continue;
                }
    
                String statementName = null;
                Object userObject = null;
                if (options.StatementNameResolver != null || options.StatementUserObjectResolver != null) {
                    var ctx = new StatementDeploymentContext(item.Expression, module, item, deploymentId);
                    statementName = options.StatementNameResolver != null ? options.StatementNameResolver.GetStatementName(ctx) : null;
                    userObject = options.StatementUserObjectResolver != null ? options.StatementUserObjectResolver.GetUserObject(ctx) : null;
                }
    
                try {
                    EPStatement stmt;
                    if (_optionalStatementIdGenerator == null) {
                        if (options.IsolatedServiceProvider == null) {
                            stmt = _epService.CreateEPL(item.Expression, statementName, userObject);
                        }
                        else {
                            var unit = _statementIsolationService.GetIsolationUnit(options.IsolatedServiceProvider, -1);
                            stmt = unit.EPAdministrator.CreateEPL(item.Expression, statementName, userObject);
                        }
                    }
                    else {
                        var statementId = _optionalStatementIdGenerator.Invoke();
                        if (options.IsolatedServiceProvider == null) {
                            stmt = _epService.CreateEPLStatementId(item.Expression, statementName, userObject, statementId);
                        }
                        else {
                            var unit = _statementIsolationService.GetIsolationUnit(options.IsolatedServiceProvider, -1);
                            var spi = (EPAdministratorIsolatedSPI) unit.EPAdministrator;
                            stmt = spi.CreateEPLStatementId(item.Expression, statementName, userObject, statementId);
                        }
                    }
                    statementNames.Add(new DeploymentInformationItem(stmt.Name, stmt.Text));
                    statements.Add(stmt);
    
                    ICollection<String> types = _statementEventTypeRef.GetTypesForStatementName(stmt.Name);
                    if (types != null) {
                        eventTypesReferenced.AddAll(types);
                    }
                }
                catch (EPException ex) {
                    exceptions.Add(new DeploymentItemException(ex.Message, item.Expression, ex, item.LineNumber));
                    if (options.IsFailFast) {
                        break;
                    }
                }
            }
    
            if (exceptions.IsNotEmpty()) {
                if (options.IsRollbackOnFail) {
                    Log.Debug("Rolling back intermediate statements for deployment");
                    foreach (var stmt in statements) {
                        try {
                            stmt.Dispose();
                        }
                        catch (Exception ex) {
                            Log.Debug("Failed to destroy created statement during rollback: " + ex.Message, ex);
                        }
                    }
                    EPLModuleUtil.UndeployTypes(eventTypesReferenced, _statementEventTypeRef, _eventAdapterService, _filterService);
                }
                var text = "Deployment failed";
                if (options.IsValidateOnly) {
                    text = "Validation failed";
                }
                throw BuildException(text, module, exceptions);
            }
    
            if (options.IsValidateOnly) {
                Log.Debug("Rolling back created statements for validate-only");
                foreach (var stmt in statements) {
                    try {
                        stmt.Dispose();
                    }
                    catch (Exception ex) {
                        Log.Debug("Failed to destroy created statement during rollback: " + ex.Message, ex);
                    }
                }
                EPLModuleUtil.UndeployTypes(eventTypesReferenced, _statementEventTypeRef, _eventAdapterService, _filterService);
                return null;
            }
    
            var deploymentInfoArr = statementNames.ToArray();
            var desc = new DeploymentInformation(deploymentId, module, addedDate, DateTime.Now, deploymentInfoArr, DeploymentState.DEPLOYED);
            _deploymentStateService.AddUpdateDeployment(desc);
    
            if (Log.IsDebugEnabled) {
                Log.Debug("Module " + module + " was successfully deployed.");
            }
            return new DeploymentResult(desc.DeploymentId, statements.AsReadOnlyList(), imports);
        }
    
        private static DeploymentActionException BuildException(String msg, Module module, List<DeploymentItemException> exceptions)
        {
            var message = msg;
            if (module.Name != null) {
                message += " in module '" + module.Name + "'";
            }
            if (module.Uri != null) {
                message += " in module url '" + module.Uri + "'";
            }
            if (exceptions.Count > 0) {
                message += " in expression '" + GetAbbreviated(exceptions[0].Expression) + "' : " + exceptions[0].Message;
            }
            return new DeploymentActionException(message, exceptions);
        }
    
        private static String GetAbbreviated(String expression) 
        {
            if (expression.Length < 60) {
                return ReplaceNewline(expression);
            }
            var subtext = expression.Substring(0, 50) + "...(" + expression.Length + " chars)";
            return ReplaceNewline(subtext);
        }
    
        private static String ReplaceNewline(String text)
        {
            text = text.RegexReplaceAll("\\n", " ");
            text = text.RegexReplaceAll("\\t", " ");
            text = text.RegexReplaceAll("\\r", " ");
            return text;
        }
    
        public Module Parse(String eplModuleText)
        {
            return EPLModuleUtil.ParseInternal(eplModuleText, null);
        }
    
        public UndeploymentResult UndeployRemove(String deploymentId)
        {
            using (_iLock.Acquire())
            {
                return UndeployRemoveInternal(deploymentId, new UndeploymentOptions());
            }
        }

        public UndeploymentResult UndeployRemove(String deploymentId, UndeploymentOptions undeploymentOptions)
        {
            using (_iLock.Acquire())
            {
                return UndeployRemoveInternal(
                    deploymentId, undeploymentOptions ?? new UndeploymentOptions());
            }
        }

        public UndeploymentResult Undeploy(String deploymentId)
        {
            using (_iLock.Acquire())
            {
                return UndeployInternal(deploymentId, new UndeploymentOptions());
            }
        }

        public UndeploymentResult Undeploy(String deploymentId, UndeploymentOptions undeploymentOptions)
        {
            using (_iLock.Acquire())
            {
                return UndeployInternal(deploymentId, undeploymentOptions ?? new UndeploymentOptions());
            }
        }

        public string[] Deployments
        {
            get
            {
                using (_iLock.Acquire())
                {
                    return _deploymentStateService.Deployments;
                }
            }
        }

        public DeploymentInformation GetDeployment(String deploymentId)
        {
            using(_iLock.Acquire())
            {
                return _deploymentStateService.GetDeployment(deploymentId);
            }
        }

        public DeploymentInformation[] DeploymentInformation
        {
            get
            {
                using (_iLock.Acquire())
                {
                    return _deploymentStateService.AllDeployments;
                }
            }
        }

        public DeploymentOrder GetDeploymentOrder(ICollection<Module> modules, DeploymentOrderOptions options)
        {
            using(_iLock.Acquire())
            {
                if (options == null)
                {
                    options = new DeploymentOrderOptions();
                }

                var deployments = _deploymentStateService.Deployments;
                var proposedModules = new List<Module>();
                proposedModules.AddAll(modules);

                ICollection<String> availableModuleNames = new HashSet<String>();
                foreach (var proposedModule in proposedModules)
                {
                    if (proposedModule.Name != null)
                    {
                        availableModuleNames.Add(proposedModule.Name);
                    }
                }

                // Collect all uses-dependencies of existing modules
                IDictionary<String, ICollection<String>> usesPerModuleName =
                    new Dictionary<String, ICollection<String>>();
                foreach (var deployment in deployments)
                {
                    var info = _deploymentStateService.GetDeployment(deployment);
                    if (info == null)
                    {
                        continue;
                    }
                    if ((info.Module.Name == null) || (info.Module.Uses == null))
                    {
                        continue;
                    }
                    var usesSet = usesPerModuleName.Get(info.Module.Name);
                    if (usesSet == null)
                    {
                        usesSet = new HashSet<String>();
                        usesPerModuleName.Put(info.Module.Name, usesSet);
                    }
                    usesSet.AddAll(info.Module.Uses);
                }

                // Collect uses-dependencies of proposed modules
                foreach (var proposedModule in proposedModules)
                {

                    // check uses-dependency is available
                    if (options.IsCheckUses)
                    {
                        if (proposedModule.Uses != null)
                        {
                            foreach (var uses in proposedModule.Uses)
                            {
                                if (availableModuleNames.Contains(uses))
                                {
                                    continue;
                                }
                                if (IsDeployed(uses))
                                {
                                    continue;
                                }
                                var message = "Module-dependency not found";
                                if (proposedModule.Name != null)
                                {
                                    message += " as declared by module '" + proposedModule.Name + "'";
                                }
                                message += " for uses-declaration '" + uses + "'";
                                throw new DeploymentOrderException(message);
                            }
                        }
                    }

                    if ((proposedModule.Name == null) || (proposedModule.Uses == null))
                    {
                        continue;
                    }
                    var usesSet = usesPerModuleName.Get(proposedModule.Name);
                    if (usesSet == null)
                    {
                        usesSet = new HashSet<String>();
                        usesPerModuleName.Put(proposedModule.Name, usesSet);
                    }
                    usesSet.AddAll(proposedModule.Uses);
                }

                var proposedModuleNames = new HashMap<String, SortedSet<int>>();
                var count = 0;
                foreach (var proposedModule in proposedModules)
                {
                    var moduleNumbers = proposedModuleNames.Get(proposedModule.Name);
                    if (moduleNumbers == null)
                    {
                        moduleNumbers = new SortedSet<int>();
                        proposedModuleNames.Put(proposedModule.Name, moduleNumbers);
                    }
                    moduleNumbers.Add(count);
                    count++;
                }

                var graph = new DependencyGraph(proposedModules.Count, false);
                var fromModule = 0;
                foreach (var proposedModule in proposedModules)
                {
                    if ((proposedModule.Uses == null) || (proposedModule.Uses.IsEmpty()))
                    {
                        fromModule++;
                        continue;
                    }
                    var dependentModuleNumbers = new SortedSet<int>();
                    foreach (var use in proposedModule.Uses)
                    {
                        var moduleNumbers = proposedModuleNames.Get(use);
                        if (moduleNumbers == null)
                        {
                            continue;
                        }
                        dependentModuleNumbers.AddAll(moduleNumbers);
                    }
                    dependentModuleNumbers.Remove(fromModule);
                    graph.AddDependency(fromModule, dependentModuleNumbers);
                    fromModule++;
                }

                if (options.IsCheckCircularDependency)
                {
                    var circular = graph.FirstCircularDependency;
                    if (circular != null)
                    {
                        var message = "";
                        var delimiter = "";
                        foreach (var i in circular)
                        {
                            message += delimiter;
                            message += "module '" + proposedModules[i].Name + "'";
                            delimiter = " uses (depends on) ";
                        }
                        throw new DeploymentOrderException(
                            "Circular dependency detected in module uses-relationships: " + message);
                    }
                }

                var reverseDeployList = new List<Module>();
                var ignoreList = new HashSet<int>();
                while (ignoreList.Count < proposedModules.Count)
                {

                    // seconardy sort according to the order of listing
                    ICollection<int> rootNodes = new SortedSet<int>(
                        new StandardComparer<int>((o1, o2) => -1*o1.CompareTo(o2)));

                    rootNodes.AddAll(graph.GetRootNodes(ignoreList));

                    if (rootNodes.IsEmpty())
                    {
                        // circular dependency could cause this
                        for (var i = 0; i < proposedModules.Count; i++)
                        {
                            if (!ignoreList.Contains(i))
                            {
                                rootNodes.Add(i);
                                break;
                            }
                        }
                    }

                    foreach (var root in rootNodes)
                    {
                        ignoreList.Add(root);
                        reverseDeployList.Add(proposedModules[root]);
                    }
                }

                reverseDeployList.Reverse();
                return new DeploymentOrder(reverseDeployList);
            }
        }
    
        public bool IsDeployed(String moduleName) {
            using(_iLock.Acquire())
            {
                var infos = _deploymentStateService.AllDeployments;
                if (infos == null)
                {
                    return false;
                }
                foreach (var info in infos)
                {
                    if ((info.Module.Name != null) && (info.Module.Name == moduleName))
                    {
                        return info.State == DeploymentState.DEPLOYED;
                    }
                }
                return false;
            }
        }
    
        public DeploymentResult ReadDeploy(Stream stream, String moduleURI, String moduleArchive, Object userObject)
        {
            using(_iLock.Acquire())
            {
                var module = EPLModuleUtil.ReadInternal(stream, moduleURI);
                return DeployQuick(module, moduleURI, moduleArchive, userObject);
            }
        }
    
        public DeploymentResult ReadDeploy(String resource, String moduleURI, String moduleArchive, Object userObject)
        {
            using(_iLock.Acquire())
            {
                var module = Read(resource);
                return DeployQuick(module, moduleURI, moduleArchive, userObject);
            }
        }
    
        public DeploymentResult ParseDeploy(String eplModuleText)
        {
            using(_iLock.Acquire())
            {
                return ParseDeploy(eplModuleText, null, null, null);
            }
        }
    
        public DeploymentResult ParseDeploy(String buffer, String moduleURI, String moduleArchive, Object userObject)
        {
            using(_iLock.Acquire())
            {
                var module = EPLModuleUtil.ParseInternal(buffer, moduleURI);
                return DeployQuick(module, moduleURI, moduleArchive, userObject);
            }
        }
    
        public void Add(Module module, String assignedDeploymentId)
        {
            using(_iLock.Acquire())
            {
                if (_deploymentStateService.GetDeployment(assignedDeploymentId) != null)
                {
                    throw new ArgumentException("Assigned deployment id '" + assignedDeploymentId +
                                                "' is already in use");
                }
                AddInternal(module, assignedDeploymentId);
            }
        }
    
        public String Add(Module module)
        {
            using(_iLock.Acquire())
            {
                var deploymentId = _deploymentStateService.NextDeploymentId;
                AddInternal(module, deploymentId);
                return deploymentId;
            }
        }
    
        private void AddInternal(Module module, String deploymentId)
        {
            var desc = new DeploymentInformation(
                deploymentId, module, DateTime.Now, DateTime.Now, new DeploymentInformationItem[0], DeploymentState.UNDEPLOYED);
            _deploymentStateService.AddUpdateDeployment(desc);
        }
    
        public DeploymentResult Deploy(String deploymentId, DeploymentOptions options)
        {
            using(_iLock.Acquire())
            {
                var info = _deploymentStateService.GetDeployment(deploymentId);
                if (info == null)
                {
                    throw new DeploymentNotFoundException("Deployment by id '" + deploymentId + "' could not be found");
                }
                if (info.State == DeploymentState.DEPLOYED)
                {
                    throw new DeploymentStateException("Module by deployment id '" + deploymentId +
                                                       "' is already in deployed state");
                }
                GetDeploymentOrder(Collections.SingletonList(info.Module), null);
                return DeployInternal(info.Module, options, deploymentId, info.AddedDate);
            }
        }
    
        public void Remove(String deploymentId)
        {
            using (_iLock.Acquire())
            {
                var info = _deploymentStateService.GetDeployment(deploymentId);
                if (info == null)
                {
                    throw new DeploymentNotFoundException("Deployment by id '" + deploymentId + "' could not be found");
                }
                if (info.State == DeploymentState.DEPLOYED)
                {
                    throw new DeploymentStateException("Deployment by id '" + deploymentId +
                                                       "' is in deployed state, please undeploy first");
                }
                _deploymentStateService.Remove(deploymentId);
            }
        }
    
        private UndeploymentResult UndeployRemoveInternal(String deploymentId, UndeploymentOptions options)
        {
            using (_iLock.Acquire())
            {
                var info = _deploymentStateService.GetDeployment(deploymentId);
                if (info == null)
                {
                    throw new DeploymentNotFoundException("Deployment by id '" + deploymentId + "' could not be found");
                }

                UndeploymentResult result = info.State == DeploymentState.DEPLOYED
                    ? UndeployRemoveInternal(info, options)
                    : new UndeploymentResult(deploymentId, Collections.GetEmptyList<DeploymentInformationItem>());
                _deploymentStateService.Remove(deploymentId);
                return result;
            }
        }

        private UndeploymentResult UndeployInternal(String deploymentId, UndeploymentOptions undeploymentOptions)
        {
            var info = _deploymentStateService.GetDeployment(deploymentId);
            if (info == null) {
                throw new DeploymentNotFoundException("Deployment by id '" + deploymentId + "' could not be found");
            }
            if (info.State == DeploymentState.UNDEPLOYED) {
                throw new DeploymentStateException("Deployment by id '" + deploymentId + "' is already in undeployed state");
            }

            var result = UndeployRemoveInternal(info, undeploymentOptions);
            var updated = new DeploymentInformation(deploymentId, info.Module, info.AddedDate, DateTime.Now, new DeploymentInformationItem[0], DeploymentState.UNDEPLOYED);
            _deploymentStateService.AddUpdateDeployment(updated);
            return result;
        }

        private UndeploymentResult UndeployRemoveInternal(DeploymentInformation info, UndeploymentOptions undeploymentOptions)
        {
            var reverted = new DeploymentInformationItem[info.Items.Length];
            for (var i = 0; i < info.Items.Length; i++) {
                reverted[i] = info.Items[info.Items.Length - 1 - i];
            }

            var revertedStatements = new List<DeploymentInformationItem>();
            if (undeploymentOptions.IsDestroyStatements) {
                var referencedTypes = new HashSet<String>();
                foreach (var item in reverted) {
                    var statement = _epService.GetStatement(item.StatementName);
                    if (statement == null) {
                        Log.Debug("Deployment id '" + info.DeploymentId + "' statement name '" + item + "' not found");
                        continue;
                    }
                    referencedTypes.AddAll(_statementEventTypeRef.GetTypesForStatementName(statement.Name));
                    if (statement.IsDisposed) {
                        continue;
                    }
                    try {
                        statement.Dispose();
                    }
                    catch (Exception ex) {
                        Log.Warn("Unexpected exception destroying statement: " + ex.Message, ex);
                    }
                    revertedStatements.Add(item);
                }
                EPLModuleUtil.UndeployTypes(referencedTypes, _statementEventTypeRef, _eventAdapterService, _filterService);
                revertedStatements.Reverse();
            }

            return new UndeploymentResult(info.DeploymentId, revertedStatements);
        }
        
        private DeploymentResult DeployQuick(Module module, String moduleURI, String moduleArchive, Object userObject)
        {
            module.Uri = moduleURI;
            module.ArchiveName = moduleArchive;
            module.UserObject = userObject;
            GetDeploymentOrder(Collections.SingletonList(module), null);
            return Deploy(module, null);
        }
    }
}
