using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour
{
    Anim ac;

    public string animation;
    public Vector2 worldPos;



    void Start() 
    {
        ac = gameObject.GetComponent<Anim>();
        ac.PlayAnimationLoop(animation);
        transform.localPosition = worldPos;
    }




}
