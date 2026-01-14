using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveGame(string fname)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + fname + ".sav";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(); // Assume SaveData is a serializable class that holds player info
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SaveData LoadGame(string fname)
    {
        string path = Application.persistentDataPath + "/" + fname + ".sav";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

    public static List<SaveData> GetAllSaves()
    {
        List<SaveData> saves = new List<SaveData>();
        string directoryPath = Application.persistentDataPath;
        if (Directory.Exists(directoryPath))
        {
            string[] files = Directory.GetFiles(directoryPath, "*.sav");
            BinaryFormatter formatter = new BinaryFormatter();

            foreach (string file in files)
            {
                FileStream stream = new FileStream(file, FileMode.Open);
                SaveData data = formatter.Deserialize(stream) as SaveData;
                stream.Close();
                saves.Add(data);
            }
        }
        return saves;
    }
}
