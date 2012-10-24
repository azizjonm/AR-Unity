using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

public class ALVARBridge {

    [DllImport("ALVARBridge", EntryPoint = "alvar_init", CallingConvention = CallingConvention.Cdecl)]
    public static extern void alvar_init(
        IntPtr imageData,
        //[MarshalAs(UnmanagedType.LPArray)] char[] imageData,
		//char[] imageData,
		int width, 
        int height);
	
	[DllImport("ALVARBridge", EntryPoint = "alvar_process", CallingConvention = CallingConvention.Cdecl)]
    public static extern void alvar_process(
        IntPtr imageData,
        //[MarshalAs(UnmanagedType.LPArray)] char[] imageData,
		//char[] imageData,
		[Out] [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] double[] transMatrix);
	
	[DllImport("ALVARBridge", EntryPoint = "alvar_close", CallingConvention = CallingConvention.Cdecl)]
    public static extern void alvar_close();
}

public class background : MonoBehaviour {
	
	//The texture that holds the video captured by the webcam
	private WebCamTexture webCamTexture;
	
	// The pixels data in raw format
	private Color32[] data;

	//The selected webcam
	public int selectedCam = 0;
	void Start()
	{
		//An integer that stores the number of connected webcams
	    //int numOfCams = WebCamTexture.devices.Length;
		print (selectedCam);
		print (WebCamTexture.devices[selectedCam].name);
		//renderer.material.color = new Color (255, 255, 255);

		//Initialize the webCamTexture
		webCamTexture = new WebCamTexture();

        renderer.material.mainTexture = webCamTexture;
		webCamTexture.deviceName = WebCamTexture.devices[selectedCam].name;
		
		//Start streaming the images captured by the webcam into the texture
        webCamTexture.Play();
		data = new Color32[webCamTexture.width * webCamTexture.height];
		
		webCamTexture.GetPixels32 (data);
		// Need to convert Color32[] to char*
		char[] imageData = new char[webCamTexture.width * webCamTexture.height * 3];

		for (int i = 0; i < webCamTexture.width * webCamTexture.height; ++i) {
			imageData[i*3] = (char)data[i].r;
			imageData[i*3+1] = (char)data[i].g;
			imageData[i*3+2] = (char)data[i].b;
		}

        /*IntPtr c = Marshal.UnsafeAddrOfPinnedArrayElement(imageData, 0);
        ALVARBridge.alvar_init(c, webCamTexture.width, webCamTexture.height);*/
        IntPtr ptr = new IntPtr();
        Marshal.StructureToPtr(imageData, ptr, true);
        ALVARBridge.alvar_init(ptr, webCamTexture.width, webCamTexture.height);
        //ALVARBridge.alvar_init(imageData, webCamTexture.width, webCamTexture.height);
	}
	
	// Update is called once per frame
	void Update () {
		
		webCamTexture.GetPixels32 (data);
		char[] imageData = new char[webCamTexture.width * webCamTexture.height * 3];
		for (int i = 0; i < webCamTexture.width * webCamTexture.height; ++i) {
			imageData[i*3] = (char)data[i].r;
			imageData[i*3+1] = (char)data[i].g;
			imageData[i*3+2] = (char)data[i].b;
		}
		
		double[] transMat = new double[16];
        /*IntPtr c = Marshal.UnsafeAddrOfPinnedArrayElement(imageData, 0);
        ALVARBridge.alvar_process(c, transMat);*/
        ALVARBridge.alvar_process(imageData, transMat);
		
		Debug.Log (transMat[0] + " " + transMat[1] + " " + transMat[2] + " " + transMat[3] + " "
			+ transMat[4] + " " + transMat[5] + " " + transMat[6] + " " + transMat[7] + " "
			+ transMat[8] + " " + transMat[9] + " " + transMat[10] + " " + transMat[11] + " "
			+ transMat[12] + " " + transMat[13] + " " + transMat[14] + " " + transMat[15]);
	}
	
	void Close() {
		ALVARBridge.alvar_close();	
	}
}
