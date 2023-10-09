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
using System.Security.Cryptography;
using System.Text;

#nullable enable
namespace Assets.Src
{
    public static class SaveManager
    {

        /* Save file structure:
         * accounts.dat
         * saveName/
         * -players/
         * --playerId.player
         * -world.dat
         * -serverId.dat
         */

        //Random Note: When using Path.Combine, don't put slashes at the start of the path, it will ignore the previous path!

        static readonly SHA256 SHA256 = SHA256.Create();

        private static string saveName = "";

        private static Guid serverIdInternal = Guid.Empty;
        public static string ServerId
        {
            get
            {
                return Convert.ToBase64String(SHA256.ComputeHash(Encoding.UTF8.GetBytes(serverIdInternal.ToString())));
            }
            private set
            {
                serverIdInternal = new Guid(value);
            }
        }

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
        static bool SaveObj(object obj, string relPath, bool relToSave = true)
        {
            Utils.Log($"Saving object at path: {relPath}... Object Type: {obj.GetType()}");

            try
            {
                //Get path
                string filePath = Path.Combine(relToSave ? SavePath : Application.persistentDataPath, relPath);

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

        static T? LoadObj<T>(string relPath, bool relToSave = true)
        {
            Utils.Log($"Loading object at path: {relPath}... Object Type: {typeof(T)}");

            try
            {
                //Get path
                string filePath = Path.Combine(relToSave ? SavePath : Application.persistentDataPath, relPath);

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
                return default;
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
                //Note: We can only make 1 directory at a time with this
                Directory.CreateDirectory(SavePath);
                Directory.CreateDirectory(Path.Combine(SavePath, "players"));

                //Save the world data
                World.World? world = World.World.instance;
                if (world == null) return;

                //Save world data
                SaveObj(world, "world.dat");

                //Save world id
                if (!File.Exists(Path.Combine(SavePath, "/serverid.dat")))
                {
                    Utils.Log("Writing save id...");
                    serverIdInternal = Guid.NewGuid();
                    SaveObj(serverIdInternal, "serverid.dat");
                }

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
                World.World.instance = LoadObj<World.World>("world.dat");

                //Load world id
                serverIdInternal = LoadObj<Guid>("serverid.dat");

                stopwatch.Stop();

                Utils.Log($"Game loaded in {stopwatch.Elapsed}.");
                return;
            }
            catch (Exception e)
            {
                Utils.Log(e, $"Error loading game {SaveName}");
            }
        }

        /// <summary>
        /// Loads the account file and searchs for the server id. If it doesn't exist, it creates a new one.
        /// </summary>
        /// <param name="serverId">The hash of the server ID</param>
        /// <returns>The hash of the account GUID</returns>
        public static string GetServerAccountHash(string serverId)
        {
            Utils.Log($"Loading account id...");

            Dictionary<string, Guid> accounts;

            //Make sure the binary formatter is initialized
            if (binaryFormatter == null) InitBinaryFormatter();

            if (File.Exists(Path.Combine(Application.persistentDataPath, "accounts.dat")))
            {
                accounts = LoadObj<Dictionary<string, Guid>>("accounts.dat", false)!;
            }
            else
            {
                accounts = new();
            }

            if (!accounts.TryGetValue(serverId, out Guid guid))
            {
                guid = Guid.NewGuid();

                accounts.Add(serverId, guid);
                SaveObj(accounts, "accounts.dat", false);
            }

            //We have to replace slashes, because the file system will interpret them as directories
            byte[] hash = SHA256.ComputeHash(Encoding.UTF8.GetBytes(guid.ToString().Replace("\\","")));
            return Convert.ToBase64String(hash);
        }

        public static Player LoadPlayerData(string accountId)
        {
            Utils.Log("Loading player data...");

            Player player;
            if(File.Exists(Path.Combine(SavePath, $"players/{accountId}.player")))
            {
                player = LoadObj<Player>($"players/{accountId}.player")!;
            }
            else
            {
                Utils.Log("Creating new player data...");

                player = new(accountId);

                SaveObj(player, $"players/{accountId}.player");
            }

            return player;
        }
    }
}
