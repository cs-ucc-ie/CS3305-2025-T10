using UnityEngine;
using UnityEngine.SceneManagement;
public class TestLoadScene : MonoBehaviour
{
    Scene scene;

void Update()
{
    if (Input.GetKeyDown(KeyCode.L))
    {
        SceneManager.LoadScene(scene.name);
    }
}

}
