using UnityEngine;

public class TestHurtEnemt : MonoBehaviour
{
    public GameObject Object;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Button T Pressed, trying to hurt enemy");
            OutleaderAI ai = Object.GetComponent<OutleaderAI>();
            ai.GetHurt(10);
        }
    }
}
