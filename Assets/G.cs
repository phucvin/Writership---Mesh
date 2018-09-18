using UnityEngine;
using Writership;

public class G : MonoBehaviour
{
    public static readonly IEngine Engine = new MultithreadEngine();
    public static readonly Op<float> Tick = Engine.Op<float>(reducer: (a, b) => a + b);

    private void LateUpdate()
    {
        Tick.Fire(Time.deltaTime);
        Engine.Update();
    }
}
