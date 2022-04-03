using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    public static class ModConstants
    {
        public const string MOD_FILE_EX_SPOREMOD = ".sporemod";
        public const string MOD_FILE_EX_DBPF = ".package";
        
        public const string MOD_SUBFILE_EX_DLL = ".dll";

        public const string ID_XML_FILE_NAME = "ModInfo.xml";
        public static readonly LoadOptions ID_XML_LOAD_OPTIONS = LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo;

        public static readonly Version ID_VER_1_0_0_0 = new Version(1, 0, 0, 0);
        public static readonly Version ID_VER_1_0_1_0 = new Version(1, 0, 1, 0);
        public static readonly Version ID_VER_1_0_1_1 = new Version(1, 0, 1, 1);

        public static readonly Version ID_VER_GRANDFATHER = new Version(1, 1, 0, 0);


        public const string AT_UNIQUE = "unique";
        public const string AT_DISP_NAME = "displayName";
    }
}
