using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using TagLib.Mpeg4;

namespace TagLibExtentions
{
    /// <summary>
    /// Mp4MetaTag処理 TagLib使用
    /// </summary>
    public class TagLibMP4Extentions
    {
        /// <summary>
        /// ファイルプロパティのタグ文字列設定
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="tagString">タグに出力する文字列</param>
        public async static Task SetMetaTagAsync(string path, IEnumerable<string> tagString)
        {
            //0件の場合終了
            if (tagString == null || tagString.Count() == 0) { return; }
            //MP4以外終了
            if (!Path.GetExtension(path).Equals(".mp4")) { return; }

            await Task.Run(() =>
            {
                using (var f = new TaglibMpeg4Wrpper(path))
                {
                    f.AddMetaTag(tagString);
                    f.Save();
                }
            });
        }

        /// <summary>
        /// ファイルプロパティのタグ文字列取得
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public async static Task<IEnumerable<string>> GetMetaTagAsync(string path)
        {
            //MP4以外終了
            if (!Path.GetExtension(path).Equals(".mp4"))
            {
                return null;
            }

            return await Task.Run(() =>
            {
                using (var f = new TaglibMpeg4Wrpper(path))
                {
                    return f.GetMetaTag();
                }
            });
        }

        /// <summary>
        /// TagLib.Mpeg4.Fileのラッパー 
        /// TagLibでmp4のUdtaがprotectedのため継承して更新する
        /// </summary>
        private sealed class TaglibMpeg4Wrpper : TagLib.Mpeg4.File
        {
            /// <summary>
            /// ファイルプロパティのタグ文字列
            /// </summary>
            private IEnumerable<string> _tagString;

            /// <summary>
            /// ファイルプロパティの属性名
            /// https://docs.microsoft.com/en-us/windows/desktop/wmp/wm-category-attribute
            /// </summary>
            private const string PKEY_Keywords = "WM/Category";

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="path">パス</param>
            public TaglibMpeg4Wrpper(string path) : base(path)
            {
            }

            /// <summary>
            /// ファイルプロパティのタグ情報を追加する。
            /// </summary>
            /// <param name="tagString">タグ文字列</param>
            public void AddMetaTag(IEnumerable<string> tagString)
            {
                this._tagString = tagString;

                ByteVector type = "Xtra";

                var hed = new BoxHeader(type);
                var box = new UnknownBox(hed, this, null);
                box.Data = CreateXtraByte();
                this.UdtaBoxes.FirstOrDefault().RemoveChild(type);
                this.UdtaBoxes.FirstOrDefault().AddChild(box);
            }

            /// <summary>
            /// タグ文字列のバイト配列を作成する。
            /// </summary>
            /// <returns>バイト配列</returns>
            private ByteVector CreateXtraByte()
            {
                //タグ文字列を判別させるめのbyte
                //1～4byte:全体のbyte配列数となるので最後に設定 
                //例:)byte配列数：31　byte:00,00,00,1F 、byte配列数：255　byte:00,00,00,ff
                ByteVector data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

                //5～8byte 00,00,00,0B固定
                data += new byte[] { 0x00, 0x00, 0x00, 0x0B };

                //5～19byte WM/Category文字列
                data += Encoding.UTF8.GetBytes(PKEY_Keywords);

                //20～23byte タグ文字列の配列数 
                data += CreateByteCategoryNum();

                //24byte～は プロパティ詳細に表示するタグ文字列
                data += CreateByteTagStaring();

                //1～4byte:タブ文字列数設定
                SetByteZeroFromFour(data);

                return data;
            }

            /// <summary>
            /// 20～23byte タグ文字列数 
            /// </summary>
            /// <returns>バイト配列</returns>
            private byte[] CreateByteCategoryNum()
            {
                var bytes = BitConverter.GetBytes(_tagString.Count());

                return new byte[] {
                    bytes[3],
                    bytes[2],
                    bytes[1],
                    bytes[0]
                };
            }

            /// <summary>
            /// 24byte～は プロパティ詳細に表示するタグ文字列
            /// </summary>
            /// <returns>バイト配列</returns>
            private ByteVector CreateByteTagStaring()
            {
                ByteVector bytes = new byte[] { };

                foreach (var tag in _tagString)
                {
                    bytes += CreateByteTagLength(tag);
                    bytes += new byte[] { 0x00, 0x08 };
                    bytes += Encoding.Unicode.GetBytes(tag);
                    bytes += new byte[] { 0x00, 0x00 };
                }

                return bytes;
            }

            /// <summary>
            /// タグ文字列数のbyteを作成する
            /// 例:) 1文字の場合 0A, 2文字の場合 0C, 3文字の場合 0E
            /// </summary>
            /// <param name="tag">タグ文字列</param>
            /// <returns>バイト配列</returns>
            private byte[] CreateByteTagLength(string tag)
            {
                var length = 10 + ((tag.Length - 1) * 2);
                var bytes = BitConverter.GetBytes(length);
                return new byte[] {
                    bytes[3],
                    bytes[2],
                    bytes[1],
                    bytes[0]
                };
            }


            /// <summary>
            /// 動的に変更となる0～4Byteの設定
            /// タグ文字列Byte全体のByte数 例:)byte数：255　byte:00,00,00,ffとなる
            /// </summary>
            /// <param name="data">バイト配列</param>
            private void SetByteZeroFromFour(ByteVector data)
            {
                var bytes = BitConverter.GetBytes(data.Count);

                data[3] = bytes[0];
                data[2] = bytes[1];
                data[1] = bytes[2];
                data[0] = bytes[3];
            }

            /// <summary>
            /// ファイルプロパティのタグ文字列を返す
            /// </summary>
            /// <returns>タグ文字列</returns>
            public IEnumerable<string> GetMetaTag()
            {
                var xtraBox = this.UdtaBoxes.FirstOrDefault().Children.
                    FirstOrDefault(x => x.BoxType.ToString() == "Xtra");

                var tags = new List<string>();

                //タグ数取得 例) test1;test2;なら2
                int tagCnt = GetTagCount(xtraBox.Data);

                //タグ文字変換 30byte目から開始
                //29byte～から以下となる
                //1～4yte目：文字数のByte, 5～6byte目：00 08, 7byte目～ タグ文字列(unicode 2byte) * 文字数 , 終端byte 00 00
                var bytes = xtraBox.Data.Data;
                int index = 30 - 1;
                for (int i = 0; i < tagCnt; i++)
                {
                    tags.Add(GetTagString(bytes, ref index));

                    if (i < tagCnt - 1)
                    {
                        ShiftSplitStart(bytes, ref index);
                    }
                }

                return tags;
            }

            /// <summary>
            /// タグ文字列を取得する
            /// </summary>
            /// <param name="bytes">タグ情報のbyte</param>
            /// <param name="index">byteの添え字</param>
            /// <returns>タグ文字列</returns>
            private string GetTagString(byte[] bytes, ref int index)
            {
                int tagStringlen = GetByteTagLength(bytes, index);
                var unicodeByte = new List<byte>();

                for (int i = 0; i < tagStringlen * 2; i++)
                {
                    unicodeByte.Add(bytes[index++]);
                }
                return Encoding.Unicode.GetString(unicodeByte.ToArray());
            }

            /// <summary>
            /// タグ文字列の文字数取得
            /// /例:) 1文字の場合 0A, 2文字の場合 0C, 3文字の場合 0E
            /// </summary>
            /// <param name="bytes">タグ情報のbyte</param>
            /// <param name="index">byteの添え字</param>
            /// <returns>タグ文字列の文字数</returns>
            private int GetByteTagLength(byte[] bytes, int index)
            {
                var lenByte = new byte[] {
                    bytes[index - 3],
                    bytes[index - 4],
                    bytes[index - 5],
                    bytes[index - 6],
                };

                return ((BitConverter.ToInt32(lenByte, 0) - 10) / 2) + 1;
            }

            /// <summary>
            /// 次の文字列開始までindexを進める
            /// </summary>
            /// <param name="bytes">タグ情報のbyte</param>
            /// <param name="index">byteの添え字</param>
            private void ShiftSplitStart(byte[] bytes, ref int index)
            {
                while (!bytes[index].Equals(0x08))
                {
                    index++;
                }
                index++;
            }

            /// <summary>
            /// 20～23byte目のtag数を取得する。
            /// </summary>
            /// <param name="data">タグ情報のbyte</param>
            /// <returns>tag数</returns>
            private int GetTagCount(ByteVector data)
            {
                var bytes = new byte[] {
                    data.Data[22],
                    data.Data[21],
                    data.Data[20],
                    data.Data[19]
                };

                return BitConverter.ToInt32(bytes, 0);
            }
        }
    }
}
