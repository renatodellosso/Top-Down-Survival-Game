using Assets.Src.Components.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

#nullable enable
namespace Assets.Src
{
    public static class Utils
    {

        private static readonly System.Random Random = new(Seed: DateTimeOffset.Now.Millisecond);

        public static void Log(string msg)
        {
            UnityEngine.Debug.Log(msg);
        }

        public static void Log(Exception e, string note = "")
        {
            UnityEngine.Debug.LogError($"Caught error: {(note != "" ? note + " - " : "")}{e.Message} (@ {e.Source})\n{e.StackTrace}");
        }

        /// <summary>
        /// Generates a random float between min and max
        /// This casts the result of Random.NextDouble() to a float, so it is less efficient than RandDouble()
        /// </summary>
        public static float RandFloat(float min, float max)
        {
            return ((float)Random.NextDouble()) * (max - min) + min;
        }

        public static double RandDouble(double min, double max)
        {
            return Random.NextDouble() * (max - min) + min;
        }

        public static double RandDouble()
        {
            return Random.NextDouble();
        }

        public static int RandInt(int min, int max)
        {
            return Random.Next(min, max);
        }

        public static int RandInt(int max)
        {
            return Random.Next(max);
        }

        static readonly List<CancellationTokenSource> activeTasks = new();

        /// <summary>
        /// Calls Task.Run() with a CancellationToken that will be cancelled when the game is closed
        /// </summary>
        /// <param name="action">The action to execute asynchronously</param>
        /// <returns>The task that was created</returns>
        public static Task Async(Action action)
        {
            CancellationTokenSource source = new();
            activeTasks.Add(source);
            Task task = Task.Run(action, source.Token);

            return task;
        }

        public static string FormatText(string text, string? color = null)
        {
            if(color != null)
                text = $"<color={color}>{text}</color>";

            return text;
        }

        /// <summary>
        /// Draws an X at the given position. Must have Gizmos enabled to see it
        /// </summary>
        public static void MarkPos(UnityEngine.Vector2 pos, float size = .25f, float duration = 5f, UnityEngine.Color? color = null)
        {
#if UNITY_EDITOR
            if(color == null)
                color = UnityEngine.Color.red;

            UnityEngine.Debug.DrawLine(pos + new UnityEngine.Vector2(-size, -size), pos + new UnityEngine.Vector2(size, size), color.Value, duration);
            UnityEngine.Debug.DrawLine(pos + new UnityEngine.Vector2(-size, size), pos + new UnityEngine.Vector2(size, -size), color.Value, duration);
#endif
        }

        public static ClientRpcParams OnlySendRpcTo(params ulong[] targetIds)
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new List<ulong>(targetIds)
                }
            };
        }

        public static GameObject GetMainCanvas()
        {
            return GameObject.FindGameObjectWithTag("MainCanvas");
        }

        private static Fadeable GetFader()
        {
            IEnumerable<Fadeable> fadeables = GameObject.FindObjectsByType<Fadeable>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            return fadeables.Where(f => f.name == "Fader").First();
        }

        private const float FADE_TIME = 1f;

        /// <summary>
        /// Fade the screen in from black
        /// </summary>
        public static void FadeScreenIn()
        {
            Fadeable fader = GetFader();
            fader.gameObject.SetActive(true);
            fader.FadeOut(FADE_TIME);
        }

        /// <summary>
        /// Fades the screen out to black
        /// </summary>
        public static void FadeScreenOut()
        {
            GetFader().FadeIn(FADE_TIME);
        }

        public static void MakeScreenBlack()
        {
            GetFader().gameObject.SetActive(true);
        }
    }
}