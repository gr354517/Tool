using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections;
using System.Security.Cryptography;
using System.Reflection.Emit;

namespace CompareDirectories
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //string folderPath1 = @"E:\Sihyong\SINOPAC\Release\02.RTM_SINOPAC";
            //string folderPath2 = @"E:\Sihyong\SINOPAC\Release\02.SINOPAC_2";
            bool m_isClear = true;
            //string folderPath1 = @"E:\OneDrive\王思詠\SOURCES\C#\FOR_WORK\比較資料夾中的檔案\CompareDirectories\Z_TEST1";
            //string folderPath2 = @"E:\OneDrive\王思詠\SOURCES\C#\FOR_WORK\比較資料夾中的檔案\CompareDirectories\Z_TEST2";

            AppSettingsReader Reader = new AppSettingsReader();
            string m_szMODE = Reader.GetValue("MODE", typeof(string)).ToString();
            string m_szPATH = Reader.GetValue("PATH", typeof(string)).ToString();
            if (!m_szPATH.EndsWith(@"\"))
            {
                m_szPATH.Append('\\');
            }
            string m_szPATH_TARGET = Reader.GetValue("PATH_TARGET", typeof(string)).ToString();
            //if (!m_szPATH_TARGET.EndsWith(@"\"))
            //{
            //    m_szPATH_TARGET.Append('\\');
            //}
            string m_szPATH_SOURCE = Reader.GetValue("PATH_SOURCE", typeof(string)).ToString();
            //if (!m_szPATH_SOURCE.EndsWith(@"\"))
            //{
            //    m_szPATH_SOURCE.Append('\\');
            //}
            string m_sz1Except2_Open = Reader.GetValue("m_is1Except2_Open", typeof(string)).ToString();
            string m_sz2Except1_Open = Reader.GetValue("m_is2Except1_Open", typeof(string)).ToString();

            string m_szLog = m_szPATH + @"\Log.log";

            string m_sz1Except2 = "1Except2";
            string m_sz2Except1 = "2Except1";

            string m_szFile = "差異_";

            bool m_is1Except2_Open = (m_sz1Except2_Open == "1");
            bool m_is2Except1_Open = (m_sz2Except1_Open == "1");

            #region 初始化
            if (File.Exists(m_szPATH + m_szFile))
            {
                File.Delete(m_szPATH + m_szFile);
            }

            if (!(Directory.Exists(m_szPATH + m_sz1Except2)))
            {
                Directory.CreateDirectory(m_szPATH + m_sz1Except2);
            }
            if (!(Directory.Exists(m_szPATH + m_sz2Except1)))
            {
                Directory.CreateDirectory(m_szPATH + m_sz2Except1);
            }

            if (m_isClear)//想了想刪除檔案就OK了
            {
                foreach (string m_szFile_C in Directory.EnumerateFiles(m_szPATH + m_sz1Except2))
                {
                    File.Delete(m_szFile_C);
                }
                foreach (string m_szFile_C in Directory.EnumerateFiles(m_szPATH + m_sz2Except1))
                {
                    File.Delete(m_szFile_C);
                }
            }
            #endregion 初始化

            List<string> differences_1Except2 = new List<string>();
            List<string> differences_2Except1 = new List<string>();

            switch (m_szMODE)
            {
                case "HASH":
                    differences_1Except2 = CompareDirectories_HashCode(m_szPATH_TARGET, m_szPATH_SOURCE);
                    differences_2Except1 = CompareDirectories_HashCode(m_szPATH_SOURCE, m_szPATH_TARGET);
                    break;
                case "LENGTH":
                default:
                    differences_1Except2 = CompareDirectories(m_szPATH_TARGET, m_szPATH_SOURCE);
                    differences_2Except1 = CompareDirectories(m_szPATH_SOURCE, m_szPATH_TARGET);
                    break;
            }

            try
            {
                #region 1Except2

                if (!m_is1Except2_Open)
                {
                    File.WriteAllText(m_szPATH + m_szFile + m_sz1Except2 + ".txt", "不比較");
                }
                else if (m_is1Except2_Open && differences_1Except2.Count > 0)
                {
                    differences_1Except2.Sort();

                    File.WriteAllLines(m_szPATH + m_szFile + m_sz1Except2 + ".txt", differences_1Except2);

                    foreach (var diff in differences_1Except2)
                    {
                        string sourcePath = Path.Combine(m_szPATH_TARGET, diff);
                        string destinationPath = Path.Combine(m_szPATH + m_sz1Except2, diff);

                        if (!(Directory.Exists(Path.GetDirectoryName(destinationPath))))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                        }

                        File.Copy(sourcePath, destinationPath, true);
                    }
                }
                else if (differences_1Except2.Count == 0)
                {
                    File.WriteAllText(m_szPATH + m_szFile + m_sz1Except2 + ".txt", "沒有差異");
                }

                #endregion 1Except2

                #region 2Except1
                if (!m_is2Except1_Open)
                {
                    File.WriteAllText(m_szPATH + m_szFile + m_sz2Except1 + ".txt", "不比較");
                }
                else if (m_is2Except1_Open && differences_2Except1.Count > 0)
                {
                    differences_2Except1.Sort();

                    File.WriteAllLines(m_szPATH + m_szFile + m_sz2Except1 + ".txt", differences_2Except1);

                    foreach (var diff in differences_2Except1)
                    {
                        string sourcePath = Path.Combine(m_szPATH_SOURCE, diff);
                        string destinationPath = Path.Combine(m_szPATH + m_sz2Except1, diff);

                        if (!(Directory.Exists(Path.GetDirectoryName(destinationPath))))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                        }

                        File.Copy(sourcePath, destinationPath, true);
                    }
                }
                else if (differences_2Except1.Count == 0)
                {
                    File.WriteAllText(m_szPATH + m_szFile + m_sz2Except1 + ".txt", "沒有差異");
                }

                #endregion 2Except1
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(m_szLog))
                {
                    sw.WriteLine("[ERROR]:" + ex.Message);
                }
                throw;
            }
        }

        static List<string> CompareDirectories(string folderPath1, string folderPath2)
        {
            Directory.GetFiles(folderPath1, "*", SearchOption.AllDirectories);
            var files1 = Directory.GetFiles(folderPath1, "*", SearchOption.AllDirectories)
                         .Select(i => new MY_FileInfo(i.Substring(folderPath1.Count() + 1), new FileInfo(i).Length)).ToList();
            var files2 = Directory.GetFiles(folderPath2, "*", SearchOption.AllDirectories)
                         .Select(i => new MY_FileInfo(i.Substring(folderPath2.Length + 1), new FileInfo(i).Length)).ToList();
            var differences = files1.AsEnumerable().Except(files2.AsEnumerable(), new FileEqualityComparer()).ToList();
            var List = differences.AsEnumerable().Select(i => i.path).ToList();

            return List;
        }

        static List<string> CompareDirectories_HashCode(string folderPath1, string folderPath2)
        {
            SHA256 sha256 = SHA256.Create();

            Directory.GetFiles(folderPath1, "*", SearchOption.AllDirectories);

            var files1 = Directory.GetFiles(folderPath1, "*", SearchOption.AllDirectories)
                         .Select(i => new MY_FileInfo_Hash(i.ToString().Substring(folderPath1.Length + 1), Trun(sha256.ComputeHash(File.OpenRead(i))))).ToList();
            //var files1 = Directory.GetFiles(folderPath1, "*", SearchOption.AllDirectories)
            //             .Select(i => new MY_FileInfo_Hash(i.ToString().Substring(folderPath1.Length + 1), File.OpenRead(i).GetHashCode())).ToList();

            var files2 = Directory.GetFiles(folderPath2, "*", SearchOption.AllDirectories)
                         .Select(i => new MY_FileInfo_Hash(i.ToString().Substring(folderPath2.Length + 1), Trun(sha256.ComputeHash(File.OpenRead(i))))).ToList();
            var differences = files1.AsEnumerable().Except(files2.AsEnumerable(), new FileEqualityComparer_Hash()).ToList();
            var List = differences.AsEnumerable().Select(i => i.path).ToList();

            return List;
        }
        static string Trun(byte[] p_lib)
        {
            StringBuilder m_szhash = new StringBuilder();
            foreach (byte b in p_lib)
            {
                m_szhash.Append(b.ToString("x2"));
            }
            return m_szhash.ToString();
        }
    }
    public class MY_FileInfo
    {
        public string path
        {
            get; set;
        }
        public long length
        {
            get; set;
        }

        public MY_FileInfo()
        {
        }
        public MY_FileInfo(string p_path, long p_length)
        {
            this.path = p_path;
            this.length = p_length;
        }
    }
    public class FileEqualityComparer : EqualityComparer<MY_FileInfo>
    {
        public override bool Equals(MY_FileInfo MFI1, MY_FileInfo MFI2)
        {
            if (MFI2 is null && MFI1 is null)
                return true;
            else if (MFI2 is null || MFI1 is null)
                return false;

            return ((MFI1.path == MFI2.path) && (MFI1.length == MFI2.length));
        }

        public override int GetHashCode(MY_FileInfo p_MY_FileInfo)
        {
            string m_sz = p_MY_FileInfo.path + p_MY_FileInfo.length.ToString();

            return m_sz.GetHashCode();
        }
    }

    public class MY_FileInfo_Hash
    {
        public string path
        {
            get; set;
        }
        public string hash_code
        {
            get; set;
        }

        public MY_FileInfo_Hash()
        {
        }
        public MY_FileInfo_Hash(string p_path, string p_hash_code)
        {
            this.path = p_path;
            this.hash_code = p_hash_code;
        }
    }
    public class FileEqualityComparer_Hash : EqualityComparer<MY_FileInfo_Hash>
    {
        public override bool Equals(MY_FileInfo_Hash MFI1, MY_FileInfo_Hash MFI2)
        {
            if (MFI2 is null && MFI1 is null)
                return true;
            else if (MFI2 is null || MFI1 is null)
                return false;

            return ((MFI1.path == MFI2.path) && (MFI1.hash_code == MFI2.hash_code));
        }

        public override int GetHashCode(MY_FileInfo_Hash p_MY_FileInfo)
        {
            string m_sz = p_MY_FileInfo.path + p_MY_FileInfo.hash_code.ToString();

            return m_sz.GetHashCode();
        }
    }
}
