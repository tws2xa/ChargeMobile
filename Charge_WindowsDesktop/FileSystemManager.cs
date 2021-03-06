﻿using System;
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
            return File.Exists(filename);
        }

        public static Stream GetFileStream(String filename, FileMode fileMode)
        {
            return new FileStream(filename, fileMode);
        }
    }
}
