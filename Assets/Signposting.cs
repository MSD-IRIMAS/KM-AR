using UnityEngine;

/// <summary>
/// Script class that locks the attached GameObject in front of the main camera at a certain distance 
/// </summary>
public class Signposting : MonoBehaviour
{
    private Vector3 Gaze;
    private Vector3 Position;
    private Quaternion Rotation;
    public float distance = 2;
    // Start is called before the first frame update
    private void Start()
    {
        Gaze = Camera.main.transform.forward;
        Position = Camera.main.transform.position;
        Rotation = Camera.main.transform.rotation;
    }

    // Update is called once per frame
    private void Update()
    {
        Gaze = Camera.main.transform.forward;
        Position = Camera.main.transform.position;
        Rotation = Camera.main.transform.rotation;
        Vector3 targetLocal = Gaze * distance;
        Vector3 targetWorld = Position + targetLocal;
        gameObject.transform.position = targetWorld;
        gameObject.transform.rotation = Rotation;
    }
}
