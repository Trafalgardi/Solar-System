using UnityEngine;

public static class Normalizer
{
    public static void Normalize(System.Func<int, float> getValue, System.Action<int, float> setValue, int length)
    {
        var min = float.PositiveInfinity;
        var max = float.NegativeInfinity;

        for (var i = 0; i < length; i++)
        {
            var value = getValue(i);
            min = System.Math.Min(min, value);
            max = System.Math.Max(max, value);
        }

        for (var i = 0; i < length; i++)
        {
            setValue(i, Mathf.InverseLerp(min, max, getValue(i)));
        }
    }
}