using System.Text;

namespace aspnetMCVBSUser1.Server
{
    namespace helper
    {
        public static class helperIO
        {
            /// <summary>
            /// 写入文本文件
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="Txt"></param>
            /// <param name="appendStr">true追加 false覆盖</param>
            public static void Writefile(string fileName, string Txt, bool appendStr = false)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(fileName, appendStr, Encoding.UTF8);
                    sw.Write(Txt);
                    sw.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }

            /// <summary>
            /// 异步写入文本文件
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="Txt"></param>
            /// <param name="appendStr">true追加 false覆盖</param>
            public static void WritefileAsync(string fileName, string Txt, bool appendStr = false)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(fileName, appendStr, Encoding.UTF8);
                    sw.WriteAsync(Txt);
                    sw.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }

            /// <summary>
            /// 读取文本文件
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static Task<string> ReadfileAsync(string fileName)
            {
                try
                {
                    StreamReader sr = new StreamReader(fileName, Encoding.UTF8);
                    var aa = sr.ReadToEndAsync();
                    sr.Close();
                    return aa;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                    //MessageBox.Show(ex.ToString());
                    //return "";
                }
                
            }

            /// <summary>
            /// 异步读取文本文件
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static string Readfile(string fileName)
            {
                try
                {
                    StreamReader sr = new StreamReader(fileName, Encoding.UTF8);
                    string aa = sr.ReadToEnd();
                    sr.Close();
                    return aa;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                    //MessageBox.Show(ex.ToString());
                    //return "";
                }

            }
        }
    }
    }
