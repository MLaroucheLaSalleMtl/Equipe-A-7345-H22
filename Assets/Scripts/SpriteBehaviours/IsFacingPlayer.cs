using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsFacingPlayer : MonoBehaviour
{
    public TransformSO playerTransform;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = playerTransform.Transform.rotation;
    }
}
