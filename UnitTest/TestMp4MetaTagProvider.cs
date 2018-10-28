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
        string _mp4Path = Path.Combine(Environment.CurrentDirectory, "Test.mp4");

        /// <summary>
        /// MetaTagがファイルに設定されているかをチェックする。
        /// </summary>
        [TestMethod]
        public void CheckSetMetaTag()
        {
            //通常のテスト       
            var task1 = IsEqualGetMetaTag(new string[] { "test1", "テスト２", "test３", "ﾃｽﾄ4", DateTime.Now.ToString("yyyyMMddhhmmss") });
            Assert.IsTrue(task1.Result);

            //環境依存文字
            var task2 = IsEqualGetMetaTag(new string[] { "①②③" });
            Assert.IsTrue(task2.Result);
        }

        /// <summary>
        /// MetaTagを設定した後に、MetaTaguを取得して比べる
        /// </summary>
        /// <param name="setTag">設定するタグ</param>
        /// <returns>ture:一致、false:不一致</returns>
        private async Task<bool> IsEqualGetMetaTag(IEnumerable<string> setTag)
        {
            await TagLibMP4Extentions.SetMetaTagAsync(_mp4Path, setTag);
            var getTag = await TagLibMP4Extentions.GetMetaTagAsync(_mp4Path);

            return setTag.SequenceEqual(getTag);
        }
    }
}
