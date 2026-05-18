using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class RebindableAction : MonoBehaviour
{
    [SerializeField]
    Button rebindButton;
    [SerializeField]
    TMP_Text actionText;
    [SerializeField]
    InputActionReference actionReference;
    [SerializeField]
    Button defaultButton;
    [SerializeField]
    TMP_Text defaultText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        actionText.text = actionReference.action.GetBindingDisplayString();
        defaultText.text = actionReference.action.GetBindingDisplayString(
            options: InputBinding.DisplayStringOptions.IgnoreBindingOverrides
        );
    }

    // Update is called once per frame
    void Update()
    {

    }

    void RebindAction(InputAction.CallbackContext context)
    {
        if (actionReference == null || actionReference.action == null) return;
        string newBindingPath = context.control.path;
        actionReference.action.ApplyBindingOverride(newBindingPath);
        actionText.text = actionReference.action.GetBindingDisplayString();
    }

    void ResetToDefault()
    {
        if (actionReference == null || actionReference.action == null) return;
        actionReference.action.RemoveAllBindingOverrides();
        actionText.text = actionReference.action.GetBindingDisplayString();
    }
}
