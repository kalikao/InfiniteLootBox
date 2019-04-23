using UnityEngine;
using System.Collections;
/// <summary>
/// Class to make taking screenshots easier.
/// </summary>
/// <remarks>
/// Attach this to a camera, uncomment the return line in Loot3D-Update,
/// place the Loot3D object in front of the camera, press Play,
/// screenshot will be taken and saved to Resources/InventoryIcons.
/// Probably should be converted to an inspector button.
/// </remarks>
public class ScreenshotCamera : MonoBehaviour
{
    public int resWidth = 512;
    public int resHeight = 512;

    void Awake()
    {
        Time.timeScale = 0f;
    }
    public string FullSSName(string name)
    {
        return string.Format("{0}/Resources/InventoryIcons/{1}.png",
                             Application.dataPath, name);
    }
    bool ssTaken = false;
    void Update()
    {
        if (!ssTaken)
        {
            Camera cam = Camera.main;
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            cam.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            // string filename = ScreenShotName(resWidth, resHeight);
            var loot = GameObject.FindObjectOfType<Loot3D>();
            if(!loot) return;
            string filename = FullSSName(loot.name);
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
        }
        ssTaken = true;
    }
}