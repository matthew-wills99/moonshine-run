using UnityEngine;

public class HoverOverItem : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color startColour;
    private bool changed = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnMouseEnter()
    {
        if(!InputManager.BuildMode)
        {
            startColour = sr.material.color;
            sr.material.color = Color.yellow;
            changed = true;
        }
        
    }

    void OnMouseExit()
    {
        if(changed)
        {
            sr.material.color = startColour;
            changed = false;
        }
        
    }
}
