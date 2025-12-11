using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoicedNaelLines;

internal class Utility
{
    public static string GetResourcePath(IDalamudPluginInterface pluginInterface, string fileName)
    {
        var resourcesDir = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "Resources");
        return Path.Combine(resourcesDir, fileName);
    }
}
