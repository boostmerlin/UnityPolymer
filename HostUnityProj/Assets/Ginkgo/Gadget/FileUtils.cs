using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Ginkgo
{
    public class FileUtils
    {
        public static readonly char[] FOLDER_SEPARATOR_CHARS = new char[]
        {
        '/',
        '\\'
        };

        public static string StreamingAssetsPath
        {
            get
            {
                string text = Application.streamingAssetsPath;
                return text;
            }
        }

        public static string TemporaryCachePath
        {
            get
            {
                string text = Application.temporaryCachePath;
                return text;
            }
        }

        public static string PersistentDataPath
        {
            get
            {
                string text = Application.persistentDataPath;
                if (text == null)
                {
#if UNITY_ANDROID
                    //TODO:
#elif UNITY_IOS || UNITY_IPHONE

#endif
                }

                if (!Directory.Exists(text))
                {
                    try
                    {
                        Directory.CreateDirectory(text);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(string.Format("FileUtils.PersistentDataPath - Error creating {0}. Exception={1}", text, ex.Message));
                    }
                }
                return text;
            }
        }

        public static string MakeSourceAssetPath(DirectoryInfo folder)
        {
            return FileUtils.MakeSourceAssetPath(folder.FullName);
        }

        public static string MakeSourceAssetPath(FileInfo fileInfo)
        {
            return FileUtils.MakeSourceAssetPath(fileInfo.FullName);
        }

        public static string MakeSourceAssetPath(string path)
        {
            string text = path.Replace("\\", "/");
            int num = text.IndexOf("/Assets", StringComparison.OrdinalIgnoreCase);
            return text.Remove(0, num + 1);
        }

        public static string MakeMetaPathFromSourcePath(string path)
        {
            return string.Format("{0}.meta", path);
        }

        public static string MakeSourceAssetMetaPath(string path)
        {
            string path2 = FileUtils.MakeSourceAssetPath(path);
            return FileUtils.MakeMetaPathFromSourcePath(path2);
        }

        public static string GameToSourceAssetPath(string path, string dotExtension = ".prefab")
        {
            return string.Format("{0}{1}", path, dotExtension);
        }

        public static string GameToSourceAssetName(string folder, string name, string dotExtension = ".prefab")
        {
            return string.Format("{0}/{1}{2}", folder, name, dotExtension);
        }

        public static string SourceToGameAssetPath(string path)
        {
            int num = path.LastIndexOf('.');
            if (num < 0)
            {
                return path;
            }
            return path.Substring(0, num);
        }

        public static string SourceToGameAssetName(string path)
        {
            int num = path.LastIndexOf('/');
            if (num < 0)
            {
                return path;
            }
            int num2 = path.LastIndexOf('.');
            if (num2 < 0)
            {
                return path;
            }
            return path.Substring(num + 1, num2);
        }

        public static string GameAssetPathToName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            int num = path.LastIndexOf('/');
            if (num < 0)
            {
                return path;
            }
            return path.Substring(num + 1);
        }

        public static string GetOnDiskCapitalizationForFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return FileUtils.GetOnDiskCapitalizationForFile(fileInfo);
        }

        public static string GetOnDiskCapitalizationForDir(string dirPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            return FileUtils.GetOnDiskCapitalizationForDir(dirInfo);
        }

        public static string GetOnDiskCapitalizationForFile(FileInfo fileInfo)
        {
            DirectoryInfo directory = fileInfo.Directory;
            string name = directory.GetFiles(fileInfo.Name)[0].Name;
            string onDiskCapitalizationForDir = FileUtils.GetOnDiskCapitalizationForDir(directory);
            return System.IO.Path.Combine(onDiskCapitalizationForDir, name);
        }

        public static string GetOnDiskCapitalizationForDir(DirectoryInfo dirInfo)
        {
            DirectoryInfo parent = dirInfo.Parent;
            if (parent == null)
            {
                return dirInfo.Name;
            }
            string name = parent.GetDirectories(dirInfo.Name)[0].Name;
            string onDiskCapitalizationForDir = FileUtils.GetOnDiskCapitalizationForDir(parent);
            return System.IO.Path.Combine(onDiskCapitalizationForDir, name);
        }

        public static bool GetLastFolderAndFileFromPath(string path, out string folderName, out string fileName)
        {
            folderName = null;
            fileName = null;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            int num = path.LastIndexOfAny(FileUtils.FOLDER_SEPARATOR_CHARS);
            if (num > 0)
            {
                int num2 = path.LastIndexOfAny(FileUtils.FOLDER_SEPARATOR_CHARS, num - 1);
                int num3 = (num2 >= 0) ? (num2 + 1) : 0;
                int length = num - num3;
                folderName = path.Substring(num3, length);
            }
            if (num < 0)
            {
                fileName = path;
            }
            else if (num < path.Length - 1)
            {
                fileName = path.Substring(num + 1);
            }
            return folderName != null || fileName != null;
        }

        public static bool SetFolderWritableFlag(string dirPath, bool writable)
        {
            string[] files = Directory.GetFiles(dirPath);
            for (int i = 0; i < files.Length; i++)
            {
                string path = files[i];
                FileUtils.SetFileWritableFlag(path, writable);
            }
            string[] directories = Directory.GetDirectories(dirPath);
            for (int j = 0; j < directories.Length; j++)
            {
                string dirPath2 = directories[j];
                FileUtils.SetFolderWritableFlag(dirPath2, writable);
            }
            return true;
        }

        public static bool SetFileWritableFlag(string path, bool setWritable)
        {
            if (!File.Exists(path))
            {
                return false;
            }
            try
            {
                FileAttributes attributes = File.GetAttributes(path);
                FileAttributes fileAttributes = (!setWritable) ? (attributes | FileAttributes.ReadOnly) : (attributes & ~FileAttributes.ReadOnly);
                if (setWritable && Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    fileAttributes |= FileAttributes.Normal;
                }
                bool result;
                if (fileAttributes == attributes)
                {
                    result = true;
                    return result;
                }
                File.SetAttributes(path, fileAttributes);
                FileAttributes attributes2 = File.GetAttributes(path);
                if (attributes2 != fileAttributes)
                {
                    result = false;
                    return result;
                }
                result = true;
                return result;
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static string GetMD5(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return string.Empty;
            }
            string result;
            using (FileStream fileStream = File.OpenRead(fileName))
            {
                MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
                byte[] value = mD5CryptoServiceProvider.ComputeHash(fileStream);
                result = BitConverter.ToString(value).Replace("-", string.Empty);
            }
            return result;
        }

        public static string GetMD5FromString(string buf)
        {
            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] value = mD5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(buf));
            return BitConverter.ToString(value).Replace("-", string.Empty);
        }
    }
}