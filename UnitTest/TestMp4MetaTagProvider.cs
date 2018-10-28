using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using TagLibExtentions;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass]
    public class TestMp4MetaTagProvider
    {
        [TestMethod]
        public void TestSetGetMetaTag()
        {
            var mp4Path = Path.Combine(Environment.CurrentDirectory, "Test.mp4");
            var setTag = new[] { "test1", "テスト２", "test３", "ﾃｽﾄ4", DateTime.Now.ToString("yyyyMMddhhmmss") };

            var setTask = SetMetaTagAsync(mp4Path, setTag);
            setTask.Wait();

            var getTask = GetMetaTagAsync(mp4Path);
            var getTag = getTask.Result;

            CollectionAssert.AreEqual(setTag.ToList(), getTag.ToList());
        }

        private async Task SetMetaTagAsync(string mp4Path, IEnumerable<string> setTag)
        {
            await TagLibMP4Extentions.SetMetaTagAsync(mp4Path, setTag);
        }

        private async Task<IEnumerable<string>> GetMetaTagAsync(string mp4Path)
        {
            return await TagLibMP4Extentions.GetMetaTagAsync(mp4Path);
        }
    }
}
