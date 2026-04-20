using ModLoader;
using SFS;
using SFS.IO;
using SFS.Parsers.Json;

namespace MorePartsMod.Utils
{
    public class FileUtils
    {
        public static void SaveWorldPersistent(string filename, object data)
        {
            IFile file =Base.worldBase.paths.worldPersistentPath.GetFile(filename);
            JsonWrapper.SaveAsJson(file, data, true);
        }

        public static bool LoadWorldPersistent<T>(string filename, out T result)
        {
            result = default;
            IFile file =Base.worldBase.paths.worldPersistentPath.GetFile(filename);
            if (!file.Exists())
            {
                return false;
            }
            JsonWrapper.TryLoadJson(file, out result);
            return true;
        }


    }
}