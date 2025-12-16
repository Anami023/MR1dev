using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMenuFan : MonoBehaviour
{
    [Header("Targets")]
    public RectTransform origin;                 // 左端ボタン（items には含めない）
    public List<RectTransform> items;            // 左→右順

    public enum Unit { Meters, Pixels }

    [Header("Circle / Arc Shape")]
    public Unit unit = Unit.Meters;
    public float radius = 0.22f;                 // 基準半径（m or px）
    [Range(0f, 1.5f)] public float arcFlatten = 0.6f; // 1=円, <1＝浅い（y半径を縮める）
    public float verticalOffset = 0f;            // 全体の上下シフト

    [Header("Equal Spacing (by arc length)")]
    [Range(64, 4096)] public int samples = 512;  // アーク長の近似精度

    [Header("Animation")]
    public float duration = 0.28f;
    public float stagger = 0.04f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    bool open, busy;
    Canvas _canvas;
    float PxPerM => 1f / (_canvas ? _canvas.transform.lossyScale.x : 0.001f);
    float Rpx => unit == Unit.Meters ? radius * PxPerM : radius;
    float RyPx => Rpx * Mathf.Max(0f, arcFlatten);
    float yShift => (unit == Unit.Meters ? verticalOffset * PxPerM : verticalOffset);

    void Awake() { _canvas = GetComponentInParent<Canvas>(); SnapClosed(); }

    public void Toggle() { if (!busy) StartCoroutine(Animate(!open)); }

    IEnumerator Animate(bool toOpen)
    {
        busy = true;

        var playOrder = new List<RectTransform>(items);
        if (!toOpen) playOrder.Reverse();

        // 一時的に操作不可
        var cgMap = new Dictionary<RectTransform, CanvasGroup>(items.Count);
        foreach (var rt in items)
        {
            var g = rt.GetComponent<CanvasGroup>() ?? rt.gameObject.AddComponent<CanvasGroup>();
            g.interactable = false; g.blocksRaycasts = false;
            cgMap[rt] = g;
        }

        Vector2 O = origin.anchoredPosition;     // 畳み位置（左端）
        Vector2 C = O + Vector2.right * Rpx;     // 円の中心（左端から +R）

        int n = items.Count;
        if (n == 0) { busy = false; yield break; }

        // ★ アーク長を等分した角度θ群を求める
        var thetas = ComputeEqualArcThetas(n, Rpx, RyPx);

        // 目標座標を先に作る（左→右順）
        var targetPos = new Dictionary<RectTransform, Vector2>(n);
        for (int i = 0; i < n; i++)
        {
            float th = thetas[i];  // 180°→0°の中の等弧長点（ラジアン）
            Vector2 pos = new Vector2(
                C.x + Rpx * Mathf.Cos(th),
                C.y + RyPx * Mathf.Sin(th) + yShift
            );
            targetPos[items[i]] = pos;
        }

        // アニメーション（展開/収納で順番だけ変える）
        for (int i = 0; i < playOrder.Count; i++)
        {
            var rt = playOrder[i];

            Vector2 from = toOpen ? O : rt.anchoredPosition;
            Vector2 to = toOpen ? targetPos[rt] : O;
            float fromA = toOpen ? 0f : 1f;
            float toA = toOpen ? 1f : 0f;

            yield return new WaitForSeconds(stagger);

            // 少し持ち上げるベジェ
            Vector2 mid = (from + to) * 0.5f + Vector2.up * (RyPx * 0.2f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float e = ease.Evaluate(Mathf.Clamp01(t));
                Vector2 p = (1 - e) * (1 - e) * from + 2 * (1 - e) * e * mid + e * e * to;
                rt.anchoredPosition = p;

                var g = cgMap[rt];
                g.alpha = Mathf.Lerp(fromA, toA, e);
                yield return null;
            }
            rt.anchoredPosition = to;
        }

        foreach (var g in cgMap.Values) { g.interactable = toOpen; g.blocksRaycasts = toOpen; }
        open = toOpen; busy = false;
    }

    // —— 等弧長のθを求める（180°→0° を n分割の“間”に n個配置）——
    List<float> ComputeEqualArcThetas(int n, float Rx, float Ry)
    {
        var result = new List<float>(n);

        // 特殊：完全に平坦（Ry≈0）なら直線の等間隔
        if (Ry < 1e-5f)
        {
            for (int i = 0; i < n; i++)
            {
                // xを左端(-R)→右端(+R)に n+1 等分
                float x = -Rx + (i + 1) * (2f * Rx / (n + 1));
                // cosθ = x/R → θ = arccos(x/R)
                float th = Mathf.Acos(Mathf.Clamp(x / Rx, -1f, 1f));
                result.Add(th); // 0〜π（右→左）※ここでは 180→0 と逆だが arccos は [0,π]
            }
            // arccos の都合で右→左並びになる可能性があるのでソートして 0..π の降順に
            result.Sort((a, b) => b.CompareTo(a)); // 180°(π)→0° の順に
            return result;
        }

        // サンプリングして累積長を作る（θ: π→0）
        int S = Mathf.Clamp(samples, 64, 4096);
        float dth = Mathf.PI / S;

        var theta = new float[S + 1];
        var cum = new float[S + 1];
        theta[0] = Mathf.PI; // 180°
        Vector2 p0 = new Vector2(Rx * Mathf.Cos(theta[0]), Ry * Mathf.Sin(theta[0]));
        cum[0] = 0f;

        for (int k = 1; k <= S; k++)
        {
            theta[k] = Mathf.PI - dth * k; // …→0°
            Vector2 pk = new Vector2(Rx * Mathf.Cos(theta[k]), Ry * Mathf.Sin(theta[k]));
            cum[k] = cum[k - 1] + (pk - p0).magnitude;
            p0 = pk;
        }
        float total = cum[S];

        // 等弧長ターゲット： (total / (n+1)) * (i+1)
        for (int i = 0; i < n; i++)
        {
            float target = total * (i + 1) / (n + 1);

            // 2分探索で cum に target を挿入する位置を探す
            int lo = 0, hi = S;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (cum[mid] < target) lo = mid + 1;
                else hi = mid;
            }

            // 線形補間で θ を滑らかに
            int k1 = Mathf.Clamp(lo, 1, S);
            int k0 = k1 - 1;
            float seg = cum[k1] - cum[k0];
            float r = (seg > 1e-6f) ? (target - cum[k0]) / seg : 0f;
            float th = Mathf.Lerp(theta[k0], theta[k1], r); // π→0 の範囲

            result.Add(th);
        }

        return result;
    }

    [ContextMenu("Snap Closed")]
    public void SnapClosed()
    {
        Vector2 O = origin ? origin.anchoredPosition : Vector2.zero;
        foreach (var rt in items)
        {
            rt.anchoredPosition = O;
            var g = rt.GetComponent<CanvasGroup>() ?? rt.gameObject.AddComponent<CanvasGroup>();
            g.alpha = 0f; g.interactable = false; g.blocksRaycasts = false;
        }
        open = false;
    }
}
