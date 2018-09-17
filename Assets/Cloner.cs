using System.Collections;
using UnityEngine;

public class Cloner : MonoBehaviour
{
    [SerializeField]
    private GameObject src = null;

    private IEnumerator Start()
    {
        for (int i = 0, n = 30; i < n; ++i)
        {
            Instantiate(src, transform.position + (Vector3)(Vector2)Random.insideUnitSphere * 10, transform.rotation);
            yield return new WaitForSeconds(0.05f);
        }
    }
}
