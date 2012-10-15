using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarningTest
{
    public class TestUtil
    {
        public static String generateRandomString(int size)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static int generateRandomInt(int low, int high)
        {
            Random random = new Random((int) DateTime.Now.Ticks);
            return random.Next(low, high);
        }

        public static string getTestProjectPath()
        {
            return @"D:\VS workspaces\BeneWar\warnings\WarningTest\";
        }

        public static string GetTestProjectFilePath()
        {
            return @"D:\VS workspaces\BeneWar\warnings\WarningTest\WarningTest.csproj";
        }

        public static string GetFakeSourceFolder()
        {
            return @"D:\VS workspaces\BeneWar\warnings\WarningTest\fakesource\";
        }

        public static string getSolutionPath()
        {
            return @"D:\VS workspaces\BeneWar\warnings\warnings.sln";
        }

        public static string GetAnotherSolutionPath()
        {
            return @"D:\VS workspaces\BeneWarShadow\CodeIssue1\CodeIssue1.sln";
        }
    }
}
