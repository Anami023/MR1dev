using UnityEngine;

public class HeadLockedAttach : MonoBehaviour
{
    public Transform head;              // OVRCameraRig: .../CenterEyeAnchor, XRI: .../Main Camera
    public float distance = 0.9f;       // 前方距離(0.6–1.0m目安)
    public Vector3 offset = new Vector3(0f, -0.10f, 0f);
    public float posLerp = 8f;          // 追従スピード（小さいほどふわっと）
    public float rotLerp = 10f;
    public float deadZoneDeg = 25f;     // 視界中心のデッドゾーン（この角度超えたら寄せる）

    void Awake()
    {
        if (!head) head = Camera.main ? Camera.main.transform : null;
    }

    void Start()
    {
        // 初期位置だけ合わせておく
        if (head)
        {
            var flatFwd = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            var p = head.position + (flatFwd.sqrMagnitude > 1e-3 ? flatFwd : head.forward) * distance + offset;
            var r = Quaternion.LookRotation((p - head.position).normalized, Vector3.up);
            transform.SetPositionAndRotation(p, r);
        }
    }

    void LateUpdate()
    {
        if (!head) return;

        // 今のパネル方向が視界中心からどれくらい外れてるか
        var toMe = (transform.position - head.position).normalized;
        var angle = Vector3.Angle(head.forward, toMe);

        if (angle > deadZoneDeg)
        {
            // 水平面に投影して“上下の揺れ”を抑える
            var flatFwd = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            if (flatFwd.sqrMagnitude < 1e-3f) flatFwd = head.forward;

            var desiredPos = head.position + flatFwd * distance + offset;
            var desiredRot = Quaternion.LookRotation((desiredPos - head.position).normalized, Vector3.up);

            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * posLerp);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotLerp);
        }
        // デッドゾーン内では動かさず、ガタつきを防止
    }
}
