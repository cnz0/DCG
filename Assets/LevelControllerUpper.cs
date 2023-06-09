using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelControllerUpper : MonoBehaviour
{   
    //[SerializeField] GameObject pl;
    void OnTriggerEnter2D(Collider2D player) {
        string level_char;
        Scene scene = SceneManager.GetActiveScene();
        string scene_name = scene.name;
        if (scene_name.Length == 6) {
            level_char = scene_name[scene_name.Length-1].ToString();
        }
        else {
            level_char = scene_name[scene_name.Length-2].ToString() + scene_name[scene_name.Length-1].ToString();
        }
        int level = int.Parse(level_char);

        if (level < 15) {
            level_char = (level+1).ToString();
            string new_level_name = "Level" + level_char;
            SceneManager.LoadScene(new_level_name);
            //player.transform.position -= new Vector3(player.transform.position.x, player.transform.position.y, 0);
            //Debug.Log("controller" + pl.transform.position.x + " " + pl.transform.position.y);
        }
    }
}
