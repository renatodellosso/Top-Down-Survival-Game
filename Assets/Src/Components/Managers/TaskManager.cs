using Assets.Src;
using System.Threading.Tasks;
using UnityEngine;

public class TaskManager : MonoBehaviour
{

    static TaskManager instance;

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void FixedUpdate()
    {
        for (int i = 0; i < CancellableTask.activeTasks.Count; i++)
        {
            CancellableTask.ActiveTaskEntry activeTaskEntry = CancellableTask.activeTasks[i];
            Task task = activeTaskEntry.task;

            if (task.IsCompleted)
            {
                CancellableTask.activeTasks.Remove(activeTaskEntry);
            }
        }
    }

    void OnApplicationQuit()
    {
        CancellableTask.CancelAllTasks();
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnScriptsReloaded()
    {
        CancellableTask.CancelAllTasks();
    }

}
