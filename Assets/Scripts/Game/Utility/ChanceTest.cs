using UnityEngine;

[ExecuteInEditMode]
public class ChanceTest : MonoBehaviour
{
    public Chance chance;
    public int total;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            chance = new Chance(new System.Random());
            total++;

            if (chance.Percent(percent: 10))
            {
                Debug.Log(message: "1");
            }

            if (chance.Percent(percent: 70))
            {
                Debug.Log(message: "7");
            }

            if (chance.Percent(percent: 20))
            {
                Debug.Log(message: "2");
            }
        }
    }
}

public class Chance
{
    private float value;

    public Chance(System.Random prng)
    {
        value = (float)prng.NextDouble();
    }

    public Chance(PRNG prng)
    {
        value = prng.Value();
    }

    public bool Percent(float percent)
    {
        if (value <= 0)
        {
            return false;
        }

        var t = percent / 100f;
        value -= t;

        return value <= 0;
    }
}