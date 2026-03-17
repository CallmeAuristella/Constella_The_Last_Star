using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DevConsoleManager : MonoBehaviour
{
    public static DevConsoleManager Instance;

    private Dictionary<string, System.Action<string[]>> commands;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitCommands();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitCommands()
    {
        commands = new Dictionary<string, System.Action<string[]>>();

        commands.Add("load", CmdLoadScene);
        commands.Add("complete", CmdCompleteLevel);
        commands.Add("reset", CmdReset);
        commands.Add("give_score", CmdGiveScore);
    }

    // =========================
    // 🎮 COMMAND EXECUTOR
    // =========================

    public void Execute(string input)
    {
        if (string.IsNullOrEmpty(input)) return;

        string[] split = input.Split(' ');
        string command = split[0].ToLower();

        if (commands.ContainsKey(command))
        {
            commands[command].Invoke(split);
        }
        else
        {
            Debug.LogWarning("[DEV] Unknown command: " + command);
        }
    }

    // =========================
    // 🔥 COMMANDS
    // =========================

    void CmdLoadScene(string[] args)
    {
        if (args.Length < 2)
        {
            Debug.Log("Usage: load SceneName");
            return;
        }

        SceneManager.LoadScene(args[1]);
        Debug.Log("[DEV] Load scene: " + args[1]);
    }

    void CmdCompleteLevel(string[] args)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[DEV] GameManager not found!");
            return;
        }

        GameManager.Instance.grandTotalScore += 999;
        SceneManager.LoadScene("StageSummary");

        Debug.Log("[DEV] Level completed");
    }

    void CmdReset(string[] args)
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("[DEV] Reset save");
    }

    void CmdGiveScore(string[] args)
    {
        if (args.Length < 2) return;

        int value;
        if (int.TryParse(args[1], out value))
        {
            GameManager.Instance.grandTotalScore += value;
            Debug.Log("[DEV] Add score: " + value);
        }
    }
}