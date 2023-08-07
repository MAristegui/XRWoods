using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Woods.Tools.Texture
{
    public class PNGConverter
    {
        public void ExportToPNG(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            var dirPath = Application.dataPath + "/" + path;
            //if (!Directory.Exists(dirPath))
            {
            //    Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + ".png", bytes);

        }
    }
}