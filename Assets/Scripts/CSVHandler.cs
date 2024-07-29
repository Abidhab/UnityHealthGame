using System.IO;
using UnityEngine;

public class CSVHandler : MonoBehaviour
{
    private string filePath;
    private static readonly object fileLock = new object(); // Lock object for synchronization

    private void Start()
    {
        filePath = Application.persistentDataPath + "/RegistrationData.csv";
        if (!File.Exists(filePath))
        {
            // Create a new file and add header if it doesn't exist
            File.WriteAllText(filePath, "Name,Email,PhoneNumber,Country\n");
        }

        Debug.Log("CSV file path: " + filePath);
    }

    public void SaveToCSV(string name, string email, string phoneNumber, string country)
    {
        string newEntry = $"{name},{email},{phoneNumber},{country}\n";
        bool saved = false;
        int retries = 3;

        while (!saved && retries > 0)
        {
            try
            {
                lock (fileLock)
                {
                    File.AppendAllText(filePath, newEntry);
                }

                Debug.Log($"Data saved to CSV file: {newEntry}");
                saved = true;
            }
            catch (IOException ex)
            {
                retries--;
                if (retries == 0)
                {
                    Debug.LogError($"Error writing to CSV file: {ex.Message}");
                }
                else
                {
                    Debug.LogWarning($"Retrying to write to CSV file: {ex.Message}");
                    System.Threading.Thread.Sleep(100); // Wait before retrying
                }
            }
        }
    }
}
