using UnityEngine;

public class ChinkoLockedAttach : MonoBehaviour
{
    [Header("’Ç]Œ³")]
    public Transform posSource;        // š Head1: ˆÊ’u‚ÌŽQÆ(Sphere / CalendarRoot‚È‚Ç)
    public Transform lookSource;       // š Head2: Œü‚«‚ÌŽQÆ(CenterEyeAnchor)

    [Header("ƒpƒ‰ƒ[ƒ^")]
    public Vector3 offset = new Vector3(0f, -0.10f, 0f); // ’Ç]Œ³‚©‚ç‚ÌƒIƒtƒZƒbƒg
    public float posLerp = 8f;          // ˆÊ’u’Ç]ƒXƒs[ƒhi¬‚³‚¢‚Ù‚Ç‚Ó‚í‚Á‚Æj
    public float rotLerp = 10f;         // ‰ñ“]’Ç]ƒXƒs[ƒh
    public float deadZoneDeg = 25f;     // Ž‹ŠE’†S‚©‚ç‚±‚ÌŠp“x’´‚¦‚½‚çgŒü‚«h‚ðC³

    void Awake()
    {
        // •ÛŒ¯FlookSource–¢Ý’è‚È‚çƒJƒƒ‰‚ð’T‚·
        if (!lookSource && Camera.main)
            lookSource = Camera.main.transform;
    }

    void Start()
    {
        // ‰ŠúˆÊ’u‚Æ‰ñ“]‚ðˆê“x‡‚í‚¹‚Ä‚¨‚­
        if (posSource)
        {
            transform.position = posSource.position + offset;
        }

        if (lookSource)
        {
            var toMe = (transform.position - lookSource.position).normalized;
            var r = Quaternion.LookRotation(toMe, Vector3.up);
            transform.rotation = r;
        }
    }

    void LateUpdate()
    {
        if (!posSource || !lookSource) return;

        // --- ˆÊ’u‚Í Sphere(posSource) ‚É’Ç] ---
        var desiredPos = posSource.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            Time.deltaTime * posLerp
        );

        // --- ‰ñ“]‚Í ƒJƒƒ‰(lookSource) ‚Ì•û‚ðŒü‚­ ---
        var toMe = (transform.position - lookSource.position).normalized;
        var center = lookSource.forward;
        var angle = Vector3.Angle(center, toMe);   // Ž‹ŠE’†S‚Æ‚ÌŠp“x

        if (angle > deadZoneDeg)
        {
            var desiredRot = Quaternion.LookRotation(toMe, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRot,
                Time.deltaTime * rotLerp
            );
        }
        // deadZone “à‚Å‚Í‰ñ“]‚ðŒÅ’è‚µ‚ÄƒKƒ^‚Â‚«‚ð—}‚¦‚é
    }
}