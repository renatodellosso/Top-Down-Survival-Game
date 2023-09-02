using Assets.Src.WorldGeneration;
using Assets.Src.SerializationSurrogates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using System.Diagnostics;

#nullable enable
namespace Assets.Src
{
    public static class SaveManager
    {

        /* Save file structure
         * saveName/
         * -main.world
         */

        public static string saveName = "", basePath = "";
        static string SavePath => Path.Combine(basePath, saveName);

        //Static readonly uses Pascal-case (upper camel case)
        static BinaryFormatter? binaryFormatter;

        const int CHUNK_SAVE_THREADS = 8;

        public static bool SaveExists(string name)
        {
            return Directory.Exists(Path.Combine(Application.persistentDataPath, name));
        }

        /// <summary>
        /// Serializes an object to a file
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="relPath">The filename, relative to base save path</param>
        static bool SaveObj(object obj, string relPath)
        {
            Utils.Log($"Saving object at path: {relPath}... Object Type: {obj.GetType()}");

            try
            {
                //Get path
                string filePath = Path.Combine(SavePath, relPath);

                //Open file
                FileStream file = File.Open(filePath, FileMode.OpenOrCreate);

                //Serialize chunk
                binaryFormatter!.Serialize(file, obj);

                //Close file
                file.Close();
            }
            catch (IOException e)
            {
                Utils.Log(e, $"Error saving object at path: {relPath}, Object Type: {obj.GetType()}");
                return false;
            }

            Utils.Log($"Saved object at path: {relPath}, Object Type: {obj.GetType()}");
            return true;
        }

        static void InitBinaryFormatter()
        {
            binaryFormatter = new();

            //Init surrogate selector
            SurrogateSelector surrogateSelector = new();
            binaryFormatter.SurrogateSelector = surrogateSelector;

            //Add surrogates
            surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), new Vector2Surrogate());
            surrogateSelector.AddSurrogate(typeof(Color32), new StreamingContext(StreamingContextStates.All), new Color32Surrogate());
        }

        public static CancellableTask SaveWorld()
        {
            //Set base filepath
            basePath = Application.persistentDataPath; //We can only do this on the main thread

            return CancellableTask.Run(SaveWorldAsync);
        }

        static void SaveWorldAsync()
        {
            Utils.Log($"Saving world {saveName}...");

            Stopwatch stopwatch = new();
            stopwatch.Start();

            try
            {
                //Make sure the binary formatter is initialized
                if (binaryFormatter == null) InitBinaryFormatter();


                //Make sure the save directory exists
                Directory.CreateDirectory(SavePath);

                //Save the world data
                World? world = World.instance;
                if (world == null) return;

                //Save world data
                SaveObj(world, "main.world");

                stopwatch.Stop();

                Utils.Log($"World saved in {stopwatch.Elapsed}");
                return;
            }
            catch (Exception e)
            {
                Utils.Log(e, $"Error saving world {saveName}");
            }
        }

    }
}
