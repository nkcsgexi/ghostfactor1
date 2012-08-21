using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using warnings.util;

namespace warnings.source.history
{
    class CompilationUnitRecord : ICodeHistoryRecord
    {
        private readonly IRecordMetaData metaData;

        public static readonly String ROOT = "Source";
        private static readonly String EXTENSION = ".rec";

        public static ICodeHistoryRecord createNewCodeRecord(String solution, String package, String file,
                String source){
            long time = DateTime.Now.Ticks;
            string recordfilename = file + time + EXTENSION;
            string sourthPath = ROOT + Path.DirectorySeparatorChar + recordfilename;
            FileStream fs = FileUtil.createFile(sourthPath);
            FileUtil.writeToFileStream(fs, source);
            IRecordMetaData metaData =
                RecordMetaData.createMetaData(solution, package, file, sourthPath, "", time);
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

        public string getFile()
        {
            return metaData.getFile();
        }

        public string getSource()
        {
            return FileUtil.readAllText(metaData.getSourcePath());
        }

        public SyntaxTree getSyntaxTree()
        {
            throw new NotImplementedException();
        }

        public long getTime()
        {
            return metaData.getTime();
        }

        public bool hasPreviousRecord()
        {
            return File.Exists(metaData.getPreviousMetaPath());
        }

        public ICodeHistoryRecord getPreviousRecord()
        {      
            IRecordMetaData previousMetaData = RecordMetaData.readMetaData(metaData.getPreviousMetaPath());
            return new CompilationUnitRecord(previousMetaData);
        }

        public ICodeHistoryRecord createNextRecord(string source)
        {
            long time = DateTime.Now.Ticks;
            string recordfilename = metaData.getFile() + time + EXTENSION;
            string sourcePath = ROOT + Path.PathSeparator + recordfilename;
            FileStream fs = FileUtil.createFile(sourcePath);
            FileUtil.writeToFileStream(fs, source);
            IRecordMetaData nextMetaData =
                RecordMetaData.createMetaData(metaData.getSolution(), metaData.getNameSpace(), metaData.getFile(),
                    sourcePath, metaData.getMetaDataPath(), time);
            return new CompilationUnitRecord(nextMetaData);
        }

        public void delete()
        {
            FileUtil.delete(metaData.getSourcePath());
            FileUtil.delete(metaData.getMetaDataPath());
        }

        private CompilationUnitRecord(IRecordMetaData metaData)
        {
            this.metaData = metaData;
        }


    }
}
