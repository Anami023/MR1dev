using UnityEngine;

public class VisibilityToggle : MonoBehaviour
{
    public GameObject target;
    [Header("初期表示設定")]
    public bool startActive = false;   // ✅ ここで初期表示をON/OFF切り替え

    void Start()
    {
        if (target) target.SetActive(startActive);
    }

    public void Toggle()
    {
        if (target) target.SetActive(!target.activeSelf);
    }

    public void SetActive(bool value)
    {
        if (target) target.SetActive(value);
    }
}
