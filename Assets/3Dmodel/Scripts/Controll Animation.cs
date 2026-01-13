using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private Coroutine idleLoopCoroutine;
    private bool isPlayingInterruptAnimation = false;

    [Header("アニメーション設定")]
    [SerializeField] private string idleAnimationName = "Armature_Humanoid";
    [SerializeField] private string interruptAnimationName = "Armature_Humanoid_001";
    [SerializeField] private float loopInterval = 20f; // ループする間隔（秒）

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animatorコンポーネントが見つかりません！");
            return;
        }

        StartIdleLoop();
    }

    void Update()
    {
        // Hキーでも手動実行可能
        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            TriggerInterruptAnimation();
        }
    }

    // 割り込みアニメーションをトリガー（外部から呼び出し可能）
    public void TriggerInterruptAnimation()
    {
        if (!isPlayingInterruptAnimation)
        {
            // 既存のコルーチンを確実に停止
            if (idleLoopCoroutine != null)
            {
                StopCoroutine(idleLoopCoroutine);
                idleLoopCoroutine = null;
            }

            StartCoroutine(PlayInterruptAnimation());
        }
        else
        {
            Debug.Log("既に割り込みアニメーション実行中です");
        }
    }

    private void StartIdleLoop()
    {
        if (idleLoopCoroutine == null)
        {
            idleLoopCoroutine = StartCoroutine(IdleLoopCoroutine());
            Debug.Log("待機アニメーションループを開始しました");
        }
    }

    private void StopIdleLoop()
    {
        if (idleLoopCoroutine != null)
        {
            StopCoroutine(idleLoopCoroutine);
            idleLoopCoroutine = null;
            Debug.Log("待機アニメーションループを停止しました");
        }
    }

    private IEnumerator IdleLoopCoroutine()
    {
        while (true)
        {
            // 割り込みアニメーション実行中は待機
            if (isPlayingInterruptAnimation)
            {
                yield return null;
                continue;
            }

            // 待機アニメーションを再生
            animator.Play(idleAnimationName, 0, 0f);
            Debug.Log($"[{Time.time:F2}] 待機アニメーション '{idleAnimationName}' を再生開始");

            // 次のフレームまで待機
            yield return null;
            yield return null;

            // アニメーションの長さを取得
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // ステート名が一致するまで待機（最大1秒）
            float timeout = Time.time + 1f;
            while (!stateInfo.IsName(idleAnimationName) && Time.time < timeout)
            {
                yield return null;
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }

            if (!stateInfo.IsName(idleAnimationName))
            {
                Debug.LogWarning($"アニメーション '{idleAnimationName}' への切り替えに失敗しました");
                yield return new WaitForSeconds(1f);
                continue;
            }

            float animLength = stateInfo.length;
            Debug.Log($"[{Time.time:F2}] アニメーション長: {animLength}秒");

            // アニメーション終了まで待機
            float animStartTime = Time.time;
            while (Time.time - animStartTime < animLength && !isPlayingInterruptAnimation)
            {
                yield return null;
            }

            // 割り込みがあった場合はスキップ
            if (isPlayingInterruptAnimation)
            {
                continue;
            }

            Debug.Log($"[{Time.time:F2}] アニメーション終了、{loopInterval}秒待機します");

            // 次のループまで待機（割り込みチェック付き）
            float waitStartTime = Time.time;
            while (Time.time - waitStartTime < loopInterval && !isPlayingInterruptAnimation)
            {
                yield return null;
            }

            // 割り込みがなければ次のループへ
            if (!isPlayingInterruptAnimation)
            {
                Debug.Log($"[{Time.time:F2}] 待機終了、次のアニメーションを再生します");
            }
        }
    }

    private IEnumerator PlayInterruptAnimation()
    {
        isPlayingInterruptAnimation = true;

        // 待機ループを停止
        StopIdleLoop();

        // 割り込みアニメーションを再生
        animator.Play(interruptAnimationName, 0, 0f);
        Debug.Log($"[{Time.time:F2}] 割り込みアニメーション '{interruptAnimationName}' を再生");

        // 次のフレームまで待機
        yield return null;
        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // ステート名が一致するまで待機（最大1秒）
        float timeout = Time.time + 1f;
        while (!stateInfo.IsName(interruptAnimationName) && Time.time < timeout)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        if (!stateInfo.IsName(interruptAnimationName))
        {
            Debug.LogWarning($"アニメーション '{interruptAnimationName}' への切り替えに失敗しました");
            isPlayingInterruptAnimation = false;
            StartIdleLoop();
            yield break;
        }

        // アニメーションが終了するまで待機
        float startTime = Time.time;
        float maxDuration = 10f; // 最大10秒でタイムアウト

        while (Time.time - startTime < maxDuration)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // アニメーションが終了したか確認
            if (stateInfo.IsName(interruptAnimationName) && stateInfo.normalizedTime >= 0.99f)
            {
                break;
            }

            // 別のアニメーションに切り替わった場合も終了
            if (!stateInfo.IsName(interruptAnimationName))
            {
                break;
            }

            yield return null;
        }

        Debug.Log($"[{Time.time:F2}] 割り込みアニメーション終了");

        isPlayingInterruptAnimation = false;

        // 待機ループを再開
        StartIdleLoop();
    }

    // クリーンアップ
    private void OnDisable()
    {
        StopIdleLoop();
        StopAllCoroutines();
    }
}