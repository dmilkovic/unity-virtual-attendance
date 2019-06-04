using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;

public class MaskedColorTextureRenderer : MonoBehaviour
{
    private Texture2D _texture;
    private const TextureFormat Format = TextureFormat.RGBA32;
    private long _lastFrameIndex = -1;

    private void Start()
    {
        _texture = new Texture2D(320, 240, Format, false);
        GetComponent<Renderer>().material.mainTexture = _texture;
    }

    public void OnNewFrame(Astra.MaskedColorFrame frame)
    {
        if (frame.Width == 0 ||
            frame.Height == 0)
        {
            return;
        }

        if (_lastFrameIndex == frame.FrameIndex)
        {
            return;
        }
      /*  System.IntPtr f = new IntPtr();
        f*/

        _lastFrameIndex = frame.FrameIndex;

        EnsureTexture(frame.Width, frame.Height);


        // _texture.LoadRawTextureData(frame.DataPtr, (int)frame.ByteLength);

        //-----------------------------------

        Color[] colors_test = new Color[3];
        colors_test[0] = Color.red;
        colors_test[1] = Color.green;
        colors_test[2] = Color.blue;
        int mipCount = Mathf.Min(3, _texture.mipmapCount);
        for (int mip = 0; mip < mipCount; ++mip)
        {
            Color[] cols = _texture.GetPixels(mip);
            for (int i = 0; i < cols.Length; ++i)
            {
                cols[i].r = 100;
                cols[i].a = 0;

            }
            _texture.SetPixels(cols, mip);
        }
        //-----------------------------------
        //var* testnaVarijabla = frame*.DataPtr;
        byte[] data = new byte[1228000];
        frame.CopyData(ref data);
        Converter<IntPtr, Astra.MaskedColorFrame> strConverter;

        //string mangedObject = strConverter.ToString();
        //Debug.Log("frame.DataPtr" + frame.DataPtr + "frame.ByteLenght" + (int)frame.ByteLength + " Hash code: " +  frame.DataPtr.ToString() + data.GetValue(0));


        _texture.Apply();
    }

    private void EnsureTexture(int width, int height)
    {
        if (_texture == null)
        {
            _texture = new Texture2D(width, height, Format, false);
            GetComponent<Renderer>().material.mainTexture = _texture;
            return;
        }

        if (_texture.width != width ||
            _texture.height != height)
        {
            _texture.Resize(width, height);
        }
    }


}
