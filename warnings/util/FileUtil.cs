using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace warnings.util
{
    public class FileUtil
    {
        //Xi: read specified lines in a file, starts with line start to line end, inclusively.
        //the minumum value of start is 0.
        public static String[] readFileLines(String path, int start, int end)
        {
            List<String> lines = new List<string>();
            int counter = 0;
            string line;
            StreamReader file =
               new System.IO.StreamReader(path);
            while ((line = file.ReadLine()) != null){
                Console.WriteLine(line);
                if(counter >= start && counter <= end){
                    lines.Add(line);
                }
                if(counter > end)
                    break;
                counter++;
            }
            return lines.ToArray();
        }

        //Xi: to read a file, from the specified line number to the end of such file.
        public static String readFileFromLine(String path, int start)
        {
            StringBuilder sb = new StringBuilder();
            int end = int.MaxValue;
            String[] lines = readFileLines(path, start, end);
            foreach(String line in lines){
                sb.Append(line);
            }
            return sb.ToString();
        }

        //Xi: append multiple lines to a file with specified path.
        public static void appendMultipleLines(string path, string[] lines)
        {
            File.AppendAllLines(path, lines);
        }

        //Xi: create file by the given path.
        public static FileStream createFile(string path)
        {
            if (File.Exists(path)){
                File.Delete(path);
            }
            return File.Create(path);
        }

        
        public static void writeToFileStream(FileStream fs, string text)
        {
            Byte[] info = new UTF8Encoding(true).GetBytes(text);
            fs.Write(info, 0, info.Length);
            fs.Close();
        }

        public static string readAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public static void delete(string sourcePath)
        {
            if (File.Exists(sourcePath))
                File.Delete(sourcePath);
        }

        /* Create a directory if such directory does not exist. */
        public static void createDirectory(string root)
        {
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
        }

        /* Delete a directory if such directory exists. All the contents in the directory will be deleted. */
        public static void deleteDirectory(String root)
        {
            if(Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }
}
