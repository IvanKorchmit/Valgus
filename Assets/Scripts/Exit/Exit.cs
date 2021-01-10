using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Exit : MonoBehaviour
{
    public int nextLevel;
    // Update is called once per frame
    public void onExitSuccessfull()
    {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex+1, LoadSceneMode.Single);

    }
}
