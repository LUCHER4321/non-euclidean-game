using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class SceneLoadST : MonoBehaviour
{
    public static SceneLoadST Instance { get; private set; }
    [SerializeField]
    Slider loadBar;
    [SerializeField]
    GameObject loadPanel;
    [SerializeField]
    TMP_Text loadText;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void SceneLoad(int sceneIndex)
    {
        StartCoroutine(LoadAsync(sceneIndex));
    }

    IEnumerator LoadAsync(int sceneIndex)
    {
        loadPanel.SetActive(true);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            loadBar.value = progress;
            string percentText = $"{Mathf.RoundToInt(progress * 100)}%";
            Debug.Log(percentText);
            loadText.text = percentText;
            yield return null;
        }
        loadPanel.SetActive(false);
    }
}
