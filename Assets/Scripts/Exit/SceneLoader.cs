using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    public void loadScene(int scene)
    {
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
    }
    public IEnumerator loadScene(int scene, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
    }
}
