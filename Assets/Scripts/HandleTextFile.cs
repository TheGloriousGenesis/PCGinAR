using UnityEngine;
using UnityEditor;
using System.IO;

public static class HandleTextFile
{
    private static string path = "Assets/results/";

    public static void WriteToFile(string fileName, string data)
    {
        StreamWriter writer = new StreamWriter(path + fileName + ".txt", true);
        writer.WriteLine(data);
        writer.Close();
    }
    
    // [MenuItem("Tools/Write file")]
    // static void WriteString()
    // {
        //Write some text to the test.txt file
        // StreamWriter writer = new StreamWriter(path, true);
        // writer.WriteLine("Test");
        // writer.Close();
        //
        // //Re-import the file to update the reference in the editor
        // AssetDatabase.ImportAsset(path); 
        // TextAsset asset = Resources.Load<TextAsset>(path);
        //
        // //Print the text from the file
        // Debug.Log(asset.text);
    // }

    [MenuItem("Tools/Read file")]
    static void ReadString()
    {
        string path = "Assets/GenerationResults/test.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path); 
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }

}