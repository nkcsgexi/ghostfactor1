using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using NLog;
using Roslyn.Services;
using Roslyn.Services.Editor;

namespace warnings.util
{
    /* The archive for multiple refactoring services provided from Roslyn. */
    public class ServiceArchive
    {
        private ServiceArchive()
        {
        }
        /* Singleton the service archive. */
        private static ServiceArchive instance;

        public static ServiceArchive getInstance()
        {
            if(instance == null)
                instance = new ServiceArchive();
            return instance;
        }

        /* Service for discovering the workspaces. */
        public IWorkspaceDiscoveryService WorkspaceDiscoveryService { set; get; }
        
        /* The rename service, retriving from CodeIssueProvider.cs. */
        public IRenameService RenameService { set; get; }

        /* The extract method service, no idea how to retrieve. */
        public IExtractMethodService ExtractMethodService { set; get; }

        /* Get the description of available services, for testing purposes. */
        public String ToString()
        {
            Logger logger = NLoggerUtil.getNLogger(typeof (ServiceArchive));
            logger.Debug("tostring");
            StringBuilder sb = new StringBuilder();
            if (WorkspaceDiscoveryService != null)
                sb.AppendLine("WorkspaceDiscoverySpace");
            if (RenameService != null)
                sb.AppendLine("RenameService");
            if (ExtractMethodService != null)
                sb.AppendLine("ExtractMethodService");
            return sb.ToString();
        }
    }
}
