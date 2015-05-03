using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace Charge
{
    class FileSystemManager
    {
        public static bool FileExists(String filename)
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

            return store.FileExists(filename);
        }

        public static FileStream GetFileStream(String filename, FileMode fileMode)
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

            return store.OpenFile(filename, fileMode);
        }
    }
}