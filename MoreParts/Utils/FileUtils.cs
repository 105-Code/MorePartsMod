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
            FilePath file = Base.worldBase.paths.worldPersistentPath.ExtendToFile(filename);
            JsonWrapper.SaveAsJson(file, data, true);
        }

        public static bool LoadWorldPersistent<T>(string filename, out T result)
        {
            result = default;
            FilePath file = Base.worldBase.paths.worldPersistentPath.ExtendToFile(filename);
            if (!file.FileExists())
            {
                return false;
            }
            JsonWrapper.TryLoadJson(file, out result);
            return true;
        }


    }
}