using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatisticTrans;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class StatisticTrans
    {
        [TestMethod]
        public void TestMethodIsTomorrow ()
        {
            List<TimeSpan> timeSpanes = new List<TimeSpan> { new TimeSpan (3, 4, 5) };

            List<Tuple<DateTime, DateTime, bool>> assert = new List<Tuple<DateTime, DateTime, bool>> {
                Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 27, 20, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 27, 21, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 27, 22, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 27, 23, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 28, 1, 0, 0), false)

                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 28, 20, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 28, 21, 0, 0), true)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 28, 23, 0, 0), true)

                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 28, 23, 46, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 29, 0, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 29, 1, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 29, 21, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 30, 9, 0, 0), false)
            };

            assert.ForEach(verify => 
                timeSpanes.ForEach( timeSpan =>
                    Assert.AreEqual(verify.Item3, FormMainTrans.IsTomorrow (verify.Item1, verify.Item2, timeSpan), $"[{verify.Item1}, {verify.Item2}] - разница=<{verify.Item1.AddDays(1)- verify.Item2.Add (timeSpan)}, {(verify.Item1.AddDays (1) - verify.Item2.Add(timeSpan)).TotalDays}>")));
        }

        [TestMethod]
        public void TestMethodParseTimeSpan ()
        {
            List<Tuple<string[], DateTime, TimeSpan>> assert = new List<Tuple<string[], DateTime, TimeSpan>> {
                Tuple.Create(new string[] { "HH:mm:ss", "04:54:56" }, DateTime.Now.Date.Add(TimeSpan.Parse("03:05:06")), TimeSpan.Parse("00:49:50"))
                , Tuple.Create(new string[] { "HH:mm:ss", "00:47:47" }, DateTime.Now.Date.Add(TimeSpan.Parse("23:48:48")), TimeSpan.Parse("00:58:59"))
                , Tuple.Create(new string[] { "HH:mm:ss", "00:47:47" }, DateTime.Now.Date.Add(TimeSpan.Parse("01:47:46")), TimeSpan.Parse("00:00:01"))
                , Tuple.Create(new string[] { "HH:mm:ss", "18:09:10" }, DateTime.Now.Date.Add(TimeSpan.Parse("01:09:11")), TimeSpan.Parse("00:59:59"))
                , Tuple.Create(new string[] { "HH:mm:ss", "23:23:24" }, DateTime.Now.Date.Add(TimeSpan.Parse("02:22:24")), TimeSpan.Parse("00:01:00"))
                , Tuple.Create(new string[] { "HH:mm:ss", "23:23:24" }, DateTime.Now.Date.Add(TimeSpan.Parse("03:23:24")), TimeSpan.Parse("01:00:00"))
            };

            assert.ForEach (verify =>
                Assert.AreEqual (FileAppSettings.parseTimeSpan(verify.Item1, verify.Item2, FileAppSettings.TIMESPAN_PARSE_FUNC.DIFFERENCE, verify.Item1[1]).CompareTo(verify.Item3), 0)
            );
        }
    }
}
