using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using warnings.util;

namespace WarningTest
{
    [TestClass]
    public class UtilityTest
    {
        String first = "abcdefgh";
        String second = "123456";
        
        // regression to Insert
        [TestMethod]
        public void TestMethod1()
        {
            String result = StringUtil.replaceWith(first, second, 0, 0);
            Assert.IsTrue(result.Equals(second + first));
            result = StringUtil.replaceWith(first, second, 1, 0);
            Assert.IsTrue(result.Equals("a123456bcdefgh"));
            result = StringUtil.replaceWith(first, second, first.Length, 0);
            Assert.IsTrue(result.Equals(first + second));
        }

        [TestMethod]
        public void TestMethod2()
        {
            String result = StringUtil.replaceWith(first, second, 0, 1);
            Assert.IsTrue(result.Equals("123456bcdefgh"));
            result = StringUtil.replaceWith(first, second, 0, 2);
            Assert.IsTrue(result.Equals("123456cdefgh"));
        }

        [TestMethod]
        public void TestMethod3()
        {
            string first2 = "abcd";
            int near = StringUtil.getStringDistance(first, first2);
            int far = StringUtil.getStringDistance(first, second);
            Assert.IsTrue(far > near);
            string second2 = "123242";
            near = StringUtil.getStringDistance(second, second2);
            Assert.IsTrue(far > near);
            far = StringUtil.getStringDistance(second2, first);
            Assert.IsTrue(far > near);
        }

        [TestMethod]
        public void TestMethod4()
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Debug("debug");
            logger.Error("error");
            logger.Fatal("fatal");
            logger.Info("info");
        }
    }
}
