using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] private float cameraZ;

    public Transform playerTransform;

    void Update()
    {
        if(playerTransform)
        {
            Vector3 cameraPos = new Vector3(playerTransform.position.x, playerTransform.position.y, cameraZ);
            transform.position = cameraPos;
        }
        else
        {
            Debug.LogWarning("Player transform not set in CameraFollowPlayer.cs");
        }
    }

}
