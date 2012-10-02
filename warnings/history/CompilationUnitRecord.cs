using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using warnings.util;

namespace warnings.source.history
{

    public interface ICodeHistoryRecord
    {
        String getSolution();
        String getNameSpace();
        String GetFile();
        String getSource();
        String getKey();
        SyntaxTree getSyntaxTree();
        long getTime();
        bool HasPreviousRecord();
        ICodeHistoryRecord GetPreviousRecord();
        ICodeHistoryRecord createNextRecord(string source);
        IDocument Convert2Document();
        void delete();
    }

    class CompilationUnitRecord : ICodeHistoryRecord
    {
        /* The metadata describing this souce version. */
        private readonly IRecordMetaData metaData;

        /* The root folder to where this file is stored. */
        public static readonly String ROOT = "Source";

        /* File extension for the source record. */
        private static readonly String EXTENSION = ".rec";

        public static ICodeHistoryRecord createNewCodeRecord(String solution, String package, String file,
                String source){

            // current time in ticks
            long time = DateTime.Now.Ticks;

            // record file name
            string recordfilename = file + time + EXTENSION;
            string sourcePath = ROOT + Path.DirectorySeparatorChar + recordfilename;
            FileUtil.WriteToFileStream(FileUtil.CreateFile(sourcePath), source);
            IRecordMetaData metaData =
                RecordMetaData.createMetaData(solution, package, file, sourcePath, "", time);
            return new CompilationUnitRecord(metaData);
        }


        public string getSolution()
        {
            return metaData.getSolution();
        }

        public string getNameSpace()
        {
            return metaData.getNameSpace();
        }

        public string GetFile()
        {
            return metaData.getFile();
        }

        public string getSource()
        {
            return FileUtil.ReadAllText(metaData.getSourcePath());
        }

        public string getKey()
        {
            return getSolution() + getNameSpace() + GetFile();
        }

        public SyntaxTree getSyntaxTree()
        {
            throw new NotImplementedException();
        }

        public long getTime()
        {
            return metaData.getTime();
        }

        public bool HasPreviousRecord()
        {
            return File.Exists(metaData.getPreviousMetaPath());
        }

        public ICodeHistoryRecord GetPreviousRecord()
        {      
            IRecordMetaData previousMetaData = RecordMetaData.readMetaData(metaData.getPreviousMetaPath());
            return new CompilationUnitRecord(previousMetaData);
        }

        public ICodeHistoryRecord createNextRecord(string source)
        {
            long time = DateTime.Now.Ticks;
            string recordfilename = metaData.getFile() + time + EXTENSION;
            string sourcePath = ROOT + Path.DirectorySeparatorChar + recordfilename;
            FileStream fs = FileUtil.CreateFile(sourcePath);
            FileUtil.WriteToFileStream(fs, source);
            IRecordMetaData nextMetaData =
                RecordMetaData.createMetaData(metaData.getSolution(), metaData.getNameSpace(), metaData.getFile(),
                    sourcePath, metaData.getMetaDataPath(), time);
            return new CompilationUnitRecord(nextMetaData);
        }

        /* Convert the source code to an IDocument instance. */
        public IDocument Convert2Document()
        {
            var converter = new String2IDocumentConverter();
            return (IDocument)converter.Convert(getSource(), null, null, null);
        }

        public void delete()
        {
            FileUtil.Delete(metaData.getSourcePath());
            FileUtil.Delete(metaData.getMetaDataPath());
        }

        private CompilationUnitRecord(IRecordMetaData metaData)
        {
            this.metaData = metaData;
        }


    }
}
