using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Death : MonoBehaviour
{
    public GameObject particle;
    public SceneLoader sceneManager;
    public Collider2D[] collidersToDestroy;
    public SoundManager sfxManager;
    public void Die()
    {
        sfxManager.PlaySound(SoundEffect.SoundEvent.Death);
        Instantiate(particle, transform.position,Quaternion.identity);
        int activeScene = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(sceneManager.loadScene(activeScene, 3));
        foreach (var coll in collidersToDestroy)
        {
            Destroy(coll);
        }
        GetComponentInChildren<SpriteRenderer>().enabled = false;
    }
}
