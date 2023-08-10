using UnityEngine;

// Allow an object to follow the position and rotation of another object in the scene
public class FollowTransform : MonoBehaviour
{
    public Transform trans;
    [SerializeField]
    private bool rotation;

    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.position = trans.position;
        if (rotation)
            this.transform.rotation = trans.rotation;

    }
}
