// MetaGrabUnityEvents.cs
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class MetaGrabUnityEvents : MonoBehaviour
{
    [Header("Target (自動で探します)")]
    [SerializeField] private Grabbable grabbable;

    [Header("Unity Events (XRIのSelect相当)")]
    public UnityEvent OnGrabbed;     // 掴んだ瞬間に発火
    public UnityEvent OnReleased;    // 離した瞬間に発火

    // 必要なら引数付き（GameObject）も用意できます
    [System.Serializable] public class GOEvent : UnityEvent<GameObject> { }
    [Header("Optional: 引数付きイベント")]
    public GOEvent OnGrabbedWithGO;
    public GOEvent OnReleasedWithGO;

    private void Reset()
    {
        grabbable = GetComponent<Grabbable>();
    }

    private void Awake()
    {
        if (!grabbable)
            grabbable = GetComponent<Grabbable>() ?? GetComponentInParent<Grabbable>();
    }

    private void OnEnable()
    {
        if (grabbable != null)
            grabbable.WhenPointerEventRaised += HandlePointerEvent;
    }

    private void OnDisable()
    {
        if (grabbable != null)
            grabbable.WhenPointerEventRaised -= HandlePointerEvent;
    }

    private void HandlePointerEvent(PointerEvent e)
    {
        switch (e.Type)
        {
            case PointerEventType.Select:      // 掴んだ
                OnGrabbed?.Invoke();
                OnGrabbedWithGO?.Invoke(gameObject);
                break;

            case PointerEventType.Unselect:    // 離した
                OnReleased?.Invoke();
                OnReleasedWithGO?.Invoke(gameObject);
                break;
        }
    }
}
