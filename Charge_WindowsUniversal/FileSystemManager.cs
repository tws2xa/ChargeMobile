using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.IO.IsolatedStorage;

namespace Charge
{
    class FileSystemManager
    {
        public static bool FileExists(String filename)
        {
			IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();

			return file.FileExists(filename);
		}

        public static Stream GetFileStream(String filename, FileMode fileMode)
        {
            IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();

            return file.OpenFile(filename, fileMode);
        }
    }
}
