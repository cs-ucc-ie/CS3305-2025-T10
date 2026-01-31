using UnityEngine;

public class GameManager : MonoBehaviour
{
    /* 
        This is the main GameManager script that persists across scenes. 
        Other managers under GameManager will also persist due to this.
        */
void Awake(){
    DontDestroyOnLoad(this.gameObject);
}
}
