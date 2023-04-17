using UnityEngine;

public class RayVisualizer : MonoBehaviour
{
    public GameObject pointObject;

    void LateUpdate()
    {
        var rayTest = GetComponent<RayTest>();

        if (rayTest.isHit)
        {
            if (!pointObject.activeSelf)
                pointObject.SetActive(true);
            pointObject.transform.position = rayTest.hitInfo.point;
            pointObject.transform.rotation = Quaternion.LookRotation(rayTest.hitInfo.normal);
        }
        else
        {
            if (pointObject.activeSelf)
                pointObject.SetActive(false);
        }
    }
}
