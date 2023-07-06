using UnityEngine;

public class Block : MonoBehaviour {

    public bool IsWall;
    public bool IsInsideMaze;
    public GameObject topWall;
    public GameObject rightWall;

    //private void OnMouseOver()
    //{
    //    if (this.IsWall)
    //    {
    //        this.transform.GetChild(2).GetComponent<Renderer>().material.SetColor("Color_4322BE18", Color.red);
    //    }
    //}

    //private void OnMouseExit()
    //{
    //    if (this.IsWall)
    //    {
    //        this.transform.GetChild(2).GetComponent<Renderer>().material.SetColor("Color_4322BE18", Color.clear);
    //    }
    //}
}
