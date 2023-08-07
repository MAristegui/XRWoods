using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Woods.Tools.Texture
{
    [CreateAssetMenu(fileName = "TextureData.asset", menuName = "Woods/Tools/Texture Data", order = 1)]

    public class TextureData : ScriptableObject
    {
        [SerializeField] Texture2D RGBChannel;
        [SerializeField] Texture2D AlphaChannel;

        [SerializeField] public Texture2D Result;

        

#if UNITY_EDITOR
        public void ProcessTexture(PNGConverter converter)
        {
            if (RGBChannel.width != AlphaChannel.width) 
            {
                Debug.LogError("Diferent RGB and Alpha texture width");
                return; 
            }
            if (RGBChannel.height != AlphaChannel.height)
            {
                Debug.LogError("Diferent RGB and Alpha texture width");
                return;
            }
            Result = new Texture2D(RGBChannel.width, RGBChannel.height, TextureFormat.RGBA32, false);
            for(int i=0; i<RGBChannel.width; i++)
            {
                for(int j=0; j< RGBChannel.height; j++)
                {
                    Color pixel = RGBChannel.GetPixel(i, j);
                    pixel.a = AlphaChannel.GetPixel(i, j).r;
                    Result.SetPixel(i, j, pixel);
                }
            }
            string path = AssetDatabase.GetAssetPath(RGBChannel);
            path = path.Replace("Assets/", "");

            converter.ExportToPNG(Result, path);
        }
#endif

    }
}