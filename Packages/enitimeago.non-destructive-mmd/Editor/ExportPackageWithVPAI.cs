using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using tar_cs;
using UnityEditor;

namespace enitimeago.ExportPackgeWithVpai
{
    public class VpaiPackageExporter
    {
        private string _vpaiDllPath;
        private List<string> _vpmRepositories = new List<string>();
        private Dictionary<string, string> _vpmDependencies = new Dictionary<string, string>();
        private bool _silentIfInstalled = false;

        public static VpaiPackageExporter WithConfig(string vpaiDllPath, IEnumerable<string> vpmRepositories, ICollection<KeyValuePair<string, string>> vpmDependencies, bool silentIfInstalled = false)
        {
            var packageExporter = new VpaiPackageExporter { _vpaiDllPath = vpaiDllPath };
            packageExporter._vpmRepositories.AddRange(vpmRepositories);
            foreach (var vpmDependency in vpmDependencies)
            {
                packageExporter._vpmDependencies.Add(vpmDependency.Key, vpmDependency.Value);
            }
            packageExporter._silentIfInstalled = silentIfInstalled;
            return packageExporter;
        }

        // TODO:
        // public void ExportPackage(string assetPathName, string fileName, ExportPackageOptions flags);
        // public void ExportPackage(string[] assetPathNames, string fileName, ExportPackageOptions flags = ExportPackageOptions.Default);

        public void ExportPackage(string assetPathName, string fileName)
        {
            using (var outFile = File.Create(fileName))
            {
                using (var outStream = new GZipStream(outFile, CompressionMode.Compress))
                {
                    using (var writer = new TarWriter(outStream))
                    {
                        // Include the asset
                        string assetGuid = AssetDatabase.AssetPathToGUID(assetPathName);
                        using (var prefabStream = File.OpenRead(assetPathName))
                        {
                            writer.Write(prefabStream, prefabStream.Length, $"{assetGuid}/asset");
                        }
                        using (var prefabMetaStream = File.OpenRead(AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPathName)))
                        {
                            writer.Write(prefabMetaStream, prefabMetaStream.Length, $"{assetGuid}/asset.meta");
                        }
                        using (var assetPathNameStream = new MemoryStream(Encoding.ASCII.GetBytes(assetPathName)))
                        {
                            writer.Write(assetPathNameStream, assetPathNameStream.Length, $"{assetGuid}/pathname");
                        }

                        // And then include VPAI
                        string vpaiGuid = "93e23fe9bbc86463a9790ebfd1fef5eb";
                        using (var vpaiStream = File.OpenRead(_vpaiDllPath))
                        {
                            writer.Write(vpaiStream, vpaiStream.Length, $"{vpaiGuid}/asset");
                        }
                        using (var vpaiMetaStream = new MemoryStream(Encoding.ASCII.GetBytes(VPAI_DLL_META)))
                        {
                            writer.Write(vpaiMetaStream, vpaiMetaStream.Length, $"{vpaiGuid}/asset.meta");
                        }
                        using (var vpaiPathStream = new MemoryStream(Encoding.ASCII.GetBytes("Assets/com.anatawa12.vpm-package-auto-installer/com.anatawa12.vpm-package-auto-installer.dll")))
                        {
                            writer.Write(vpaiPathStream, vpaiPathStream.Length, $"{vpaiGuid}/pathname");
                        }
                        string configGuid = "9028b92d14f444e2b8c389be130d573f";
                        // TODO: use a real json library? is unity inbuilt json usable? maybe not because Dictionary and string[,] are both unsupported(?)
                        string configFile = @"{
  ""vpmRepositories"": [
    " + string.Join(", ", _vpmRepositories.Select(x => $"\"{x}\"")) + @"
  ],
  ""vpmDependencies"": {
    " + string.Join(", ", _vpmDependencies.Select(kv => $"\"{kv.Key}\": \"{kv.Value}\"")) + @"
  },
  ""silentIfInstalled"": " + (_silentIfInstalled ? "true" : "false") + @"
}";
                        string configMeta = @"fileFormatVersion: 2
guid: 9028b92d14f444e2b8c389be130d573f
TextScriptImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";
                        using (var configStream = new MemoryStream(Encoding.ASCII.GetBytes(configFile)))
                        {
                            writer.Write(configStream, configStream.Length, $"{configGuid}/asset");
                        }
                        using (var configMetaStream = new MemoryStream(Encoding.ASCII.GetBytes(configMeta)))
                        {
                            writer.Write(configMetaStream, configMetaStream.Length, $"{configGuid}/asset.meta");
                        }
                        using (var configPathStream = new MemoryStream(Encoding.ASCII.GetBytes("Assets/com.anatawa12.vpm-package-auto-installer/config.json")))
                        {
                            writer.Write(configPathStream, configPathStream.Length, $"{configGuid}/pathname");
                        }
                        string folderGuid = "4b344df74d4849e3b2c978b959abd31b";
                        string folderMeta = @"fileFormatVersion: 2
guid: 4b344df74d4849e3b2c978b959abd31b
timeCreated: 1652316538
";
                        using (var folderMetaStream = new MemoryStream(Encoding.ASCII.GetBytes(folderMeta)))
                        {
                            writer.Write(folderMetaStream, folderMetaStream.Length, $"{folderGuid}/asset.meta");
                        }
                        using (var folderPathStream = new MemoryStream(Encoding.ASCII.GetBytes("Assets/com.anatawa12.vpm-package-auto-installer")))
                        {
                            writer.Write(folderPathStream, folderPathStream.Length, $"{folderGuid}/pathname");
                        }
                    }
                }
            }
        }

        private const string VPAI_DLL_META = @"fileFormatVersion: 2
guid: 93e23fe9bbc86463a9790ebfd1fef5eb
PluginImporter:
  externalObjects: {}
  serializedVersion: 2
  iconMap: {}
  executionOrder: {}
  defineConstraints: []
  isPreloaded: 0
  isOverridable: 0
  isExplicitlyReferenced: 1
  validateReferences: 1
  platformData:
  - first:
      : Any
    second:
      enabled: 0
      settings:
        Exclude Android: 1
        Exclude Editor: 0
        Exclude Linux64: 1
        Exclude OSXUniversal: 1
        Exclude Win: 1
        Exclude Win64: 1
  - first:
      Android: Android
    second:
      enabled: 0
      settings:
        CPU: ARMv7
  - first:
      Any: 
    second:
      enabled: 0
      settings: {}
  - first:
      Editor: Editor
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
        DefaultValueInitialized: true
        OS: AnyOS
  - first:
      Standalone: Linux64
    second:
      enabled: 0
      settings:
        CPU: None
  - first:
      Standalone: OSXUniversal
    second:
      enabled: 0
      settings:
        CPU: None
  - first:
      Standalone: Win
    second:
      enabled: 0
      settings:
        CPU: None
  - first:
      Standalone: Win64
    second:
      enabled: 0
      settings:
        CPU: None
  - first:
      Windows Store Apps: WindowsStoreApps
    second:
      enabled: 0
      settings:
        CPU: AnyCPU
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";
    }
}
