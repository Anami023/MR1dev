using UnityEngine;

public class MoveToFront : MonoBehaviour
{
    [Header("移動させたいオブジェクト")]
    public Transform target;

    [Header("参照する頭（CenterEyeAnchor）")]
    public Transform head;

    [Header("目の前の位置設定")]
    public float distance = 0.9f;
    public float heightOffset = -0.05f;

    public void ShowAndTeleport()
    {
        if (!target || !head) return;
        target.gameObject.SetActive(true);
        TeleportToFront();
    }

    public void TeleportToFront()
    {
        if (!target || !head) return;

        // 水平前方（上下ブレ防止）
        Vector3 flatForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude < 1e-6f) flatForward = head.forward;

        // 目の前の位置
        Vector3 goalPos = head.position + flatForward * distance + Vector3.up * heightOffset;

        // ユーザーの方を向く（UIがこちら向きになる）
        Quaternion goalRot = Quaternion.LookRotation(goalPos - head.position, Vector3.up);

        // ★一回だけ反映（追従しない）
        target.SetPositionAndRotation(goalPos, goalRot);
    }
}
