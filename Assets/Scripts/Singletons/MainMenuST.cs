using UnityEngine;

public class MainMenuST : MonoBehaviour
{
    public static MainMenuST Instance { get; private set; }
    [SerializeField]
    GameObject mainMenu;
    [SerializeField]
    GameObject optionsMenu;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Options(bool open)
    {
        mainMenu.SetActive(!open);
        optionsMenu.SetActive(open);
    }

    public void CloseGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void StartGame()
    {
        mainMenu.SetActive(false);
    }
}
