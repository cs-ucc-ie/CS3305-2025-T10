using UnityEngine;
using UnityEngine.SceneManagement;
public class TestLoadScene : MonoBehaviour
{
void Update()
{
    if (Input.GetKeyDown(KeyCode.L))
    {
        SceneManager.LoadScene("TestLoadScene1");
    }
}

}
