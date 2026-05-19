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
    int targetBindingIndex = 0;
    [SerializeField]
    Button defaultButton;
    [SerializeField]
    TMP_Text defaultText;
    private InputActionRebindingExtensions.RebindingOperation rebindOperation;

    private bool IsDuplicateBinding(string newPath, InputAction actionToRebind, int bindingIndexToRebind)
    {
        InputActionAsset asset = actionToRebind.actionMap.asset;
        if (asset == null) return false;
        foreach (InputActionMap map in asset.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    InputBinding binding = action.bindings[i];
                    if (action == actionToRebind && i == bindingIndexToRebind) continue;
                    if (binding.isComposite) continue;
                    if (binding.effectivePath == newPath) return true;
                }
            }
        }
        return false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateActionText();
        defaultText.text = actionReference.action.GetBindingDisplayString(
            targetBindingIndex,
            options: InputBinding.DisplayStringOptions.IgnoreBindingOverrides
        );
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetRebind()
    {
        Player.Instance.currentRebind = this;
        Player.Instance.playerInput.SwitchCurrentActionMap("Rebind Action");
        actionReference.action.Disable();
        rebindOperation = actionReference.action.PerformInteractiveRebinding(targetBindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .OnComplete(operation =>
            {
                string newPath = operation.selectedControl.path;
                if (IsDuplicateBinding(newPath, actionReference.action, targetBindingIndex))
                {
                    actionReference.action.RemoveBindingOverride(targetBindingIndex);
                    Debug.LogWarning("Rebind canceled! The key " + newPath + " is already in use.");
                }
                FinalizeRebind();
            })
            .OnCancel(operation =>
            { FinalizeRebind(); })
            .Start();
    }

    private void FinalizeRebind()
    {
        rebindOperation.Dispose();
        rebindOperation = null;
        actionReference.action.Enable();
        UpdateActionText();
        Player.Instance.playerInput.SwitchCurrentActionMap("Menu");
    }

    public void RebindAction(InputAction.CallbackContext context)
    {
        if (actionReference == null || actionReference.action == null) return;
        string newBindingPath = context.control.path;
        actionReference.action.ApplyBindingOverride(newBindingPath);
        UpdateActionText();
    }

    public void ResetToDefault()
    {
        if (actionReference == null || actionReference.action == null) return;
        actionReference.action.RemoveBindingOverride(targetBindingIndex);
        UpdateActionText();
    }

    void UpdateActionText()
    {
        if (actionReference != null && actionReference.action != null) actionText.text = actionReference.action.GetBindingDisplayString(targetBindingIndex);
    }
}
