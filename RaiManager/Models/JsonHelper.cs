using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RaiManager.Models;

public static class JsonHelper
{
    public static async Task<TObject?> Read<TObject>(string path) where TObject: class
    {
        if (!File.Exists(path))
        {
            return null;
        }

        return await Task.Run(() => JsonConvert.DeserializeObject<TObject>(File.ReadAllText(path)));
    }
}