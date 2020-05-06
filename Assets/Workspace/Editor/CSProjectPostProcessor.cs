using System;
using System.IO;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

public class CSProjectPostProcessor : AssetPostprocessor
{
    private static string OnGeneratedCSProject(string path, string content)
    {
        var document = XDocument.Parse(content);
        var folderPath = Application.dataPath
            .Replace("Assets", "")
            .Replace('/', Path.DirectorySeparatorChar);

        foreach (var descendant in document.Root.Descendants())
        {
            if (descendant.Name.LocalName != "Compile")
                continue;

            var attribute = descendant.Attribute("Include");
            if (attribute == null)
                continue;
            if (!attribute.Value.Contains(".cs") || !attribute.Value.Contains(folderPath))
                continue;

            attribute.Value = attribute.Value.Replace(folderPath, "");
        }
        
        return document.Declaration + Environment.NewLine + document.Root;
    }
}