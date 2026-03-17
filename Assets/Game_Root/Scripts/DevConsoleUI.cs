using UnityEngine;
using TMPro;

public class DevConsoleUI : MonoBehaviour
{
    public GameObject consoleRoot;
    public TMP_InputField inputField;

    private bool isOpen = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote)) // `
        {
            ToggleConsole();
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Return))
        {
            Submit();
        }
    }

    void ToggleConsole()
    {
        isOpen = !isOpen;
        consoleRoot.SetActive(isOpen);

        if (isOpen)
        {
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    void Submit()
    {
        string input = inputField.text;

        DevConsoleManager.Instance.Execute(input);

        inputField.text = "";
        inputField.ActivateInputField();
    }
}