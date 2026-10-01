using UnityEngine;

public class Shadow : MonoBehaviour
{
    [SerializeField] Transform theSun;
    [SerializeField] Transform visualParent;
    [SerializeField] Transform shadowVisual;
    float size = 1;
    [SerializeField] float minYSize = 0.2f;   // 0.2 for ball
    [SerializeField] float minXSize = 0.2f;   // 0.2 for ball
    [SerializeField] float maxYSize = 2f;     // 2 for ball
    [SerializeField] float maxXSize = 2f;     // 2 for ball

    // Update is called once per frame
    void Update()
    {
        // rotate shadow to be opposite thesun
        transform.up = (transform.position - theSun.position).normalized;

        // measure distance from sun, and proposed distortions accordingly
        float rawSize = (transform.position - theSun.position).magnitude / 5;
        float ySize = Mathf.Clamp(rawSize, minYSize, maxYSize);
        float xSize = Mathf.Clamp(rawSize / 2.5f, 1, maxXSize);

        // apply distortions
        shadowVisual.localScale = new Vector3(xSize, ySize, 1);
        shadowVisual.localPosition = new Vector3(0, Mathf.Clamp(rawSize / 2, 0, maxYSize / 2), 0);;
        
        // optional for rotating the shape, as well as the basic shadow rotation
        if (visualParent != null) shadowVisual.up = visualParent.up;
    }
}
