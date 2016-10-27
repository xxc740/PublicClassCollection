using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using ToolsClass.HttpHelper;
using ToolsClass.SystemHelper;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestRegistryWriteIsReight()
        {
            RegistryTest model = new RegistryTest
            {
                Name = "HelloWorld",
                Age = 100
            };

            RegistryKey register = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, Environment.Is64BitOperatingSystem?RegistryView.Registry64 : RegistryView.Registry32);
            RegistryKey writeKey = register.CreateSubKey("TestTool");
            if (writeKey != null)
            {
                var result = model.Save(writeKey);
                writeKey.Dispose();
                Assert.IsTrue(result,"写入失败！");
            }

            register.Dispose();
        }

        [TestMethod]
        public void TestRegistryReadIsReight()
        {
            RegistryTest model = new RegistryTest();
            RegistryKey register = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            RegistryKey writeKey = register.OpenSubKey("TestTool");

            if (writeKey != null)
            {
                var result = model.Read(writeKey);
                writeKey.Dispose();
                Assert.IsTrue(result, "读取失败！");
            }

            register.Dispose();
        }

        [TestMethod]
        public void TestGetHtmlIsRight()
        {
            HttpItem item = new HttpItem();
            item.URL = "https://accounts.douban.com/login";
            var result = new HttpHelper().GetHtml(item);
            var cookielist = HttpCookieHelper.GetCookieList(result.Cookie);
            Assert.AreNotEqual(result.StatusCode,"OK","获取失败!");
            Assert.AreNotEqual(cookielist.Count,0, "获取失败!");
        }
    }


    [RegistryRoot(Name = "Henrry.xu")]
    public class RegistryTest : IRegistry
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
}
