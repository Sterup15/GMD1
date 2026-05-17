using UnityEngine;

namespace GameObjects.Common.Utils
{
    public static class CurveUtils
    {
        public static AnimationCurve EaseIn(float t0, float v0, float t1, float v1)
        {
            float slope = (v1 - v0) / (t1 - t0);
            return new AnimationCurve(
                new Keyframe(t0, v0, 0f,         0f),
                new Keyframe(t1, v1, slope * 2f, 0f)
            );
        }
    }
}
