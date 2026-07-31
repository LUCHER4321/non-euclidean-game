using UnityEngine;
using UnityEngine.UI;

public class HandSigner : MonoBehaviour
{
    [SerializeField] RawImage[] hands;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateHands(int index)
    {
        for (int i = 0; i < Player.Instance.hands.Length; i++) hands[i].gameObject.SetActive(i == index);
    }
}
