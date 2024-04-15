using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


[Serializable]
public class Beacon
{
    public int Id { get; set; }
    public string Message { get; set; }

    public Beacon(int id, string message)
    {
        Id = id;
        Message = message;
    }

    public override string ToString()
    {
        return $"Beacon ID: {Id}, Message: {Message}";
    }
}

public class BeaconFileManager
{
    public static Beacon LoadBeacon(string filePath)
    {
        try
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return (Beacon)binaryFormatter.Deserialize(fileStream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading beacon: {ex.Message}");
            return null;
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter the file path to load the beacon: ");
        string loadFilePath = Console.ReadLine();
        Beacon loadedBeacon = BeaconFileManager.LoadBeacon(loadFilePath);
        if (loadedBeacon != null)
        {
            Console.WriteLine("Loaded Beacon:");
            Console.WriteLine(loadedBeacon); 
        }

        Console.ReadLine();
    }
}