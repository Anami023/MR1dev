using Oculus.Interaction;
using UnityEngine;

public class GrabEvents : MonoBehaviour
{
    [SerializeField] private Grabbable grabbable;

    void Awake()
    {
        // Grabbableが同じオブジェクトにない場合、親から探す
        if (!grabbable)
            grabbable = GetComponent<Grabbable>() ?? GetComponentInParent<Grabbable>();
    }

    void OnEnable()
    {
        if (grabbable != null)
            grabbable.WhenPointerEventRaised += OnPointerEvent;
    }

    void OnDisable()
    {
        if (grabbable != null)
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
    }

    private void OnPointerEvent(PointerEvent e)
    {
        switch (e.Type)
        {
            case PointerEventType.Select:      // 掴んだ瞬間
                OnGrab();
                break;
            case PointerEventType.Unselect:    // 離した瞬間
                OnRelease();
                break;
        }
    }

    public void OnGrab()
    {
        Debug.Log("Grabbed (Meta SDK)");
        // ここに掴んだ時の処理を書く
    }

    public void OnRelease()
    {
        Debug.Log("Released (Meta SDK)");
        // 離した時の処理
    }
}
