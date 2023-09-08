using Assets.Src.World;
using Assets.Src.SerializationSurrogates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using System.Diagnostics;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;

#nullable enable
namespace Assets.Src
{
    public static class SaveManager
    {

        /* Save file structure
         * saveName/
         * -world
         */

        public static string saveName = "";
        public static string SaveName
        {
            set
            {
                saveName = value;
            }
            private get
            {
                if (saveName == "")
                    throw new Exception("Save name not set");
                return saveName;
            }
        }

        private static string basePath = "";
        static string SavePath => Path.Combine(basePath, SaveName);

        //Static readonly uses Pascal-case (upper camel case)
        static BinaryFormatter? binaryFormatter;

        public static bool SaveExists(string name)
        {
            return Directory.Exists(Path.Combine(Application.persistentDataPath, name));
        }

        /// <summary>
        /// Gets the names of all the save files, in order of most recently accessed (most recent first)
        /// </summary>
        /// <returns>An array of the save file names</returns>
        public static string[] GetSaveFileNames()
        {
            DirectoryInfo directoryInfo = new(Application.persistentDataPath);
            DirectoryInfo[] directories = directoryInfo.GetDirectories();

            directories = directories.OrderByDescending(directories => directories.LastAccessTime).ToArray();

            string[] names = new string[directories.Length];
            for (int i = 0; i < directories.Length; i++)
            {
                names[i] = directories[i].Name;
            }

            return names;
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

                //Serialize
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

        static T? LoadObj<T>(string relPath)
        {
            Utils.Log($"Loading object at path: {relPath}... Object Type: {typeof(T)}");

            try
            {
                //Get path
                string filePath = Path.Combine(SavePath, relPath);

                //Open file
                FileStream file = File.Open(filePath, FileMode.Open);

                //Deserialize
                T obj = (T)binaryFormatter!.Deserialize(file);

                //Close file
                file.Close();

                return obj;
            }
            catch (IOException e)
            {
                Utils.Log(e, $"Error loading object at path: {relPath}, Object Type: {typeof(T)}");
                return default(T);
            }
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
            Utils.Log($"Saving world {SaveName}...");

            Stopwatch stopwatch = new();
            stopwatch.Start();

            try
            {
                //Make sure the binary formatter is initialized
                if (binaryFormatter == null) InitBinaryFormatter();


                //Make sure the save directory exists
                Directory.CreateDirectory(SavePath);

                //Save the world data
                World.World? world = World.World.instance;
                if (world == null) return;

                //Save world data
                SaveObj(world, "world");

                stopwatch.Stop();

                Utils.Log($"World saved in {stopwatch.Elapsed}");
                return;
            }
            catch (Exception e)
            {
                Utils.Log(e, $"Error saving world {SaveName}");
            }
        }

        /// <summary>
        /// Loads the game from the save file. The save name must be set before calling this method.
        /// </summary>
        public static async Task LoadGameAsync()
        {
            Utils.Log($"Loading game: {SaveName}...");
            //Set base filepath
            basePath = Application.persistentDataPath; //We can only do this on the main thread
            await Task.Run(LoadGame);
        }

        static void LoadGame()
        {
            Utils.Log($"Loading game: {SaveName}...");

            Stopwatch stopwatch = new();
            stopwatch.Start();

            try
            {
                //Make sure the binary formatter is initialized
                if (binaryFormatter == null) InitBinaryFormatter();

                //Load world data
                World.World.instance = LoadObj<World.World>("world");

                stopwatch.Stop();

                Utils.Log($"Game loaded in {stopwatch.Elapsed}");
                return;
            }
            catch (Exception e)
            {
                Utils.Log(e, $"Error loading game {SaveName}");
            }
        }
    }
}
