using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatisticCommon;
using System.Collections.Generic;
using System;

namespace UnitTest
{
    [TestClass]
    public class KeyDeviceTest
    {
        [TestMethod]
        public void TestMethodToArray ()
        {
            string join = string.Empty;
            List<FormChangeMode.KeyDevice> keys = new List<FormChangeMode.KeyDevice>();

            for (int i = 0; i < 10; i++)
                keys.Add (new FormChangeMode.KeyDevice () { Id = i * (i + 1), Mode = FormChangeMode.MODE_TECCOMPONENT.VYVOD });

            try {
                join = string.Join (",", keys.Select (key => key.ToString ()).ToArray ());
                join = string.Join (", ", keys.ConvertAll<string> (key => key.Id.ToString ()).ToArray ());
            } catch {
                join = string.Empty;
                Assert.IsTrue (false);
            }

            Assert.IsTrue (keys.Count > 0 ? join.Length > 0 : join.Length == 0);
        }

        [TestMethod]
        public void TestMethodToString ()
        {
            FormChangeMode.KeyDevice key = new FormChangeMode.KeyDevice () { Id = -1, Mode = FormChangeMode.MODE_TECCOMPONENT.PC };

            StringAssert.Matches (key.ToString(), new Regex(FormChangeMode.MODE_TECCOMPONENT.PC.ToString ()));
        }
    }
}
