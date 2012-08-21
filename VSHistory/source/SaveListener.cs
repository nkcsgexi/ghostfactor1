using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System.Diagnostics;

namespace Microsoft.VSHistory
{
    class SaveListener : IVsRunningDocTableEvents3
    {
        private IVsRunningDocumentTable m_RDT;
        private uint m_rdtCookie = 0;
   
        public String ContextRepository
        {
            get { return ContextRepository; }
            set { ContextRepository = value; }
        }

        public String SolutionBaseDirectory
        {
            get { return SolutionBaseDirectory;  }
            set { SolutionBaseDirectory = value; }
        }


        public bool Register()
        {
            m_RDT = (IVsRunningDocumentTable)Package.GetGlobalService(typeof(SVsRunningDocumentTable));
            m_RDT.AdviseRunningDocTableEvents(this, out m_rdtCookie);
            return true;
        }

        public int OnAfterSave(uint docCookie)
        {
            uint flags, readlocks, editlocks;
            string name; IVsHierarchy hier;
            uint itemid; IntPtr docData;
            m_RDT.GetDocumentInfo(docCookie, out flags, out readlocks, out editlocks, out name, out hier, out itemid, out docData);
            CopyFileToCache(name);
            return VSConstants.S_OK;
        }

  

        public void CopyFileToCache(string file)
        {
            try
            {
                var newPath = PrepareDocumentCache(file, DateTime.Now);
                System.IO.File.Copy(file, newPath, true);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private string PrepareDocumentCache(string file, DateTime day)
        {
            string relativePath = file.Replace(SolutionBaseDirectory + "\\", "");
            var dayStr = MakeDayString(day);
            var time = MakeTimeString(day);
            var newPath = System.IO.Path.Combine(ContextRepository, dayStr, relativePath + "$" + time);
            var dirPath = System.IO.Path.GetDirectoryName(newPath);
            if (!System.IO.File.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }
            return newPath;
        }

        private static string MakeDayString(DateTime day)
        {
            return string.Format("{0:0000}", day.Year) + "\\" + string.Format("{0:00}", day.Month) + "\\" + string.Format("{0:00}", day.Day);
        }

        private static string MakeTimeString(DateTime time)
        {
            return time.ToString("hh.mm.ss.fff.tt");
        }

        public void Shutdown()
        {
            if (m_RDT != null)
            {
                m_RDT.UnadviseRunningDocTableEvents(m_rdtCookie);
                m_RDT = null;
            }
        }
    }
}
