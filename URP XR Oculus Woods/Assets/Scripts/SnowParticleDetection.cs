using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.ParticleSystem;

public class SnowParticleDetection : MonoBehaviour
{
    public static bool EnableDetection = false;
    public static int Count = 0;

    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] int MaxParticleCollision = 60000;

    List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    private void Start()
    {
        Count = 0;
        EnableDetection = false;
    }
    /*
    private void OnParticleCollision(GameObject other)
    {
        if (other != particleSystem.gameObject) return;

        if (!EnableDetection) return;

        collisionEvents.Clear();
        ParticlePhysicsExtensions.GetCollisionEvents(particleSystem, this.gameObject, collisionEvents);

        Count += collisionEvents.Count;

    }
    /**/

    float timer = 0;

    private void FixedUpdate()
    {
        if (particleSystem.main.maxParticles > 0)
        {
            timer += Time.fixedDeltaTime;
            if (timer > 20)
            {
                Count += (int)(particleSystem.emission.rateOverTime.constant * Time.fixedDeltaTime);
                if (Count > MaxParticleCollision)
                {
                    Count = MaxParticleCollision;
                }
            }
        }
        else timer = 0;
    }


    /*
    // Use this for initialization
    public void SaveTexture()
    {
        texture = toTexture2D2(renderTexture);
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath+"/SavedScreen2.png", bytes);
    }
    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(2048 , 2048, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    Texture2D toTexture2D2(RenderTexture rTex)
    {
        int width = renderTexture.width;
        //var depth = Camera.depthTextureMode;
        Texture2D outputTex = new Texture2D(width, width, TextureFormat.RFloat, false);
        
        RenderTexture.active = rTex;           // If not using a scene camera
        outputTex.ReadPixels(
                  new Rect(0, 0, width, width), // Capture the whole texture
                  0, 0,                          // Write starting at the top-left texel
                  false                          // No mipmaps
        );

        outputTex.Apply();
        RenderTexture.active = null;
        return outputTex;

    }
    /**/
}
