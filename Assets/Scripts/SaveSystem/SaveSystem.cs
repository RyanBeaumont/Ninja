using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveGame(string fname)
    {
        try{
        SaveData data = SaveDataBuilder.Build(fname); // Assume SaveData is a serializable class that holds player info
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + fname + ".sav";
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();
         AudioManager.Instance.PlaySoundEffect("Save");
        }catch (System.Exception e)
        {
            Debug.LogError("SAVE FAILED: " + e);
            GameManager.Instance.ShowMessage("Save failed");
        }
    }

    public static SaveData LoadGame(string fname)
{
    string path = Application.persistentDataPath + "/" + fname + ".sav";

    if (!File.Exists(path))
    {
        Debug.LogError("Save file not found: " + path);
        GameManager.Instance.ShowMessage("Save file not found");
        return null;
    }

    try
    {
        FileInfo info = new FileInfo(path);
        if (info.Length == 0)
        {
            Debug.LogError("Save file is empty: " + path);
            GameManager.Instance.ShowMessage("Save file is corrupt");
            return null;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(path, FileMode.Open))
        {
            SaveData data = formatter.Deserialize(stream) as SaveData;
            return data;
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError($"LOAD FAILED for {path}: {e}");
        GameManager.Instance.ShowMessage("Save file is corrupt");

        // Optional: rename or delete corrupt save
        string corruptPath = path + ".corrupt";
        if (File.Exists(path))
            File.Move(path, corruptPath);

        return null;
    }
}

    public static List<SaveData> GetAllSaves()
{
    List<SaveData> saves = new List<SaveData>();
    string directoryPath = Application.persistentDataPath;

    if (!Directory.Exists(directoryPath))
        return saves;

    string[] files = Directory.GetFiles(directoryPath, "*.sav");
    BinaryFormatter formatter = new BinaryFormatter();

    foreach (string file in files)
    {
        try
        {
            FileInfo info = new FileInfo(file);
            if (info.Length == 0)
            {
                Debug.LogError("Save file is empty: " + file);
                continue;
            }

            using (FileStream stream = new FileStream(file, FileMode.Open))
            {
                SaveData data = formatter.Deserialize(stream) as SaveData;
                saves.Add(data);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Corrupt save skipped: {file}\n{e}");

            // Optional: quarantine corrupt file
            string corruptPath = file + ".corrupt";
            if (File.Exists(file))
                File.Move(file, corruptPath);
        }
    }

    return saves;
}
}
