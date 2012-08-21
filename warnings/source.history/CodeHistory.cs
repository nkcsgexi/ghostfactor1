using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using warnings.util;

namespace warnings.source.history
{
    public class CodeHistory : ICodeHistory
    {
        /* Singleton the code history instance. */
        private static CodeHistory instance = new CodeHistory();
        public static ICodeHistory getInstance()
        {
            return instance;
        }

        /* Combined key and the latest record under that key. */
        private readonly Dictionary<String, ICodeHistoryRecord> latestRecordDictionary;

        private CodeHistory()
        {
            latestRecordDictionary = new Dictionary<String, ICodeHistoryRecord>();

            // Delete and recreate the folder for saving the source code record.
            FileUtil.deleteDirectory(CompilationUnitRecord.ROOT);
            FileUtil.createDirectory(CompilationUnitRecord.ROOT);

            // Delete and recreate the folder for saving the metadata record.
            FileUtil.deleteDirectory(RecordMetaData.ROOT);
            FileUtil.createDirectory(RecordMetaData.ROOT);
        }

        private String combineKey(String solution, String nameSpace, String file)
        {
            return solution + nameSpace + file;
        }

        /* Add a new record, the latest record will be replaced.*/
        public void addRecord(string solution, string nameSpace, string file, string source)
        {
            String key = combineKey(solution, nameSpace, file);
            if(hasRecord(solution, nameSpace, file))
            {
                ICodeHistoryRecord record = getLatestRecord(solution, nameSpace, file);
                ICodeHistoryRecord nextRecord = record.createNextRecord(source);
                latestRecordDictionary.Remove(key);
                latestRecordDictionary.Add(key, nextRecord);
            }
            else
            {
                ICodeHistoryRecord record = CompilationUnitRecord.createNewCodeRecord(solution, nameSpace, file, source);
                latestRecordDictionary.Add(key, record);
            }
        }

        public bool hasRecord(string solution, string nameSpace, string file)
        {
            String key = combineKey(solution, nameSpace, file);
            return latestRecordDictionary.ContainsKey(key);
        }

        public ICodeHistoryRecord getLatestRecord(string solution, string nameSpace, string file)
        {
            Contract.Requires(hasRecord(solution, nameSpace, file));
            String key = combineKey(solution, nameSpace, file);
            ICodeHistoryRecord record;
            latestRecordDictionary.TryGetValue(key, out record);
            return record;
        }
    }
}
