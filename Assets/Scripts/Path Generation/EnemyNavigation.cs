using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour {

    private List<Vector3> pathToFollow;
    private int currentPositionIndex;

    [SerializeField]
    private float movementSpeed;

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(this.pathToFollow[this.currentPositionIndex], 1.0f);
    //}

    // Use this for initialization
    void Start () {
        this.pathToFollow = GameObject.Find("Manager").GetComponent<StageMaker>().CurrentPath;
        var startPosition = this.pathToFollow[0];
        this.pathToFollow.Insert(0, new Vector3(startPosition.x, 10.0f, startPosition.z));
        this.currentPositionIndex = 0;
        Vector3 currentPosition = this.pathToFollow[currentPositionIndex];
        this.transform.position = this.pathToFollow[0];
        this.transform.LookAt(new Vector3(currentPosition.x, this.transform.position.y, currentPosition.z));
    }
	
	// Update is called once per frame
	void Update () {
        var newPosition = Vector3.MoveTowards(this.transform.position, this.pathToFollow[currentPositionIndex], this.movementSpeed * Time.deltaTime);
        this.transform.position = newPosition;
        if (Vector3.Distance(this.transform.position, this.pathToFollow[this.currentPositionIndex]) < 0.01f)
        {
            this.currentPositionIndex++;

            if (this.currentPositionIndex >= this.pathToFollow.Count)
            {
                Destroy(this.gameObject);
                return;
            }

            Vector3 currentPosition = this.pathToFollow[currentPositionIndex];
            this.transform.LookAt(new Vector3(currentPosition.x, this.transform.position.y, currentPosition.z));
        }
    }
}
