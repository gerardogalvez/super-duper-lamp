using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Collections.Generic;
using System;
using TMPro;

public class StageMaker : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject verticalWall;
    [SerializeField]
    private GameObject horizontalWall;
    [SerializeField]
    private GameObject block;
    [SerializeField]
    private GameObject torch;
    [SerializeField]
    private GameObject brickVase;
    [SerializeField]
    private GameObject lineRenderer;

    [Header("Parent GameObjects")]
    [SerializeField]
    private Transform Walls;
    [SerializeField]
    private Transform BlockWalls;
    [SerializeField]
    private Transform BlockPath;
    [SerializeField]
    private Transform TorchesAndStuff;
    [SerializeField]
    private Transform LineRenderers;
    [SerializeField]
    private List<List<Vector3>> lineRendererPoints;

    [Header("Materials")]
    [SerializeField]
    private Material highligtEdgesShader;
    [SerializeField]
    private Material coverShader;
    [SerializeField]
    private Material defaultMaterial;

    [Header("Other")]
    [SerializeField]
    private GameObject startText;
    [SerializeField]
    private GameObject goalText;
    [SerializeField]
    private GameObject cameraLookAtObject;
    [SerializeField]
    private GameObject plane;

    private Tuple<int, int> mapSize;
    private Tuple<int, int> currentMapSize;

    private int layerDefault;
    private int layerPath;

    private GameObject[,] stage;
    private List<Vector3> currentPath;

    const int MAX_SIZE = 20;

    private void ClearLog()
    {
#if UNITY_EDITOR
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
#endif
    }

    private void DeletePath()
    {
        GameObject[] toDestroy = GameObject.FindGameObjectsWithTag("Destroyable");
        GameObject[] lineRenderersToDestroy = GameObject.FindGameObjectsWithTag("LineRenderer");
        foreach (var item in toDestroy)
        {
            if (item.GetComponent<Block>() != null)
            {
                item.transform.position = new Vector3(item.transform.position.x, -0.35f, item.transform.position.z);
                item.transform.GetChild(2).GetComponent<Renderer>().material.SetColor("Color_4322BE18", Color.clear);
                if (item.GetComponent<Block>().IsInsideMaze)
                {
                    item.GetComponent<Block>().topWall.GetComponent<Renderer>().enabled = true;
                    item.GetComponent<Block>().rightWall.GetComponent<Renderer>().enabled = true;
                }
            }
        }

        this.lineRendererPoints.Clear();
        foreach (var item in lineRenderersToDestroy)
        {
            Destroy(item);
        }
    }

    private void DestroyProps()
    {
        GameObject[] propsToDestroy = GameObject.FindGameObjectsWithTag("Prop");

        foreach (var item in propsToDestroy)
        {
            Destroy(item);
        }
    }

    private void CreateRandomPath(Tuple<int, int> size)
    {
        this.currentMapSize = Tuple.Create(size.Item1, size.Item2);
        this.currentPath.Clear();
        System.Random r = new System.Random();
        Tuple<int, int> start = Tuple.Create(r.Next(0, size.Item1), r.Next(0, size.Item2));
        Tuple<int, int> goal = Tuple.Create(r.Next(0, size.Item1), r.Next(0, size.Item2));

        while (start.Equals(goal))
        {
            start = Tuple.Create(r.Next(0, size.Item1), r.Next(0, size.Item2));
            goal = Tuple.Create(r.Next(0, size.Item1), r.Next(0, size.Item2));
        }

        // Debug.Log($"Creating maze from {start} to {goal}");
        var result = PathGenerator.GenerateMaze(size, start, goal, UnityEngine.Random.Range(0.2f, 0.5f));
        var maze = result.Item1;
        maze[start.Item1, start.Item2].IsWall = false;
        maze[goal.Item1, goal.Item2].IsWall = false;
        var path = result.Item2;
        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                var tmp = Tuple.Create(i, j);
                GameObject cube = this.stage[i, j];
                if (maze[i, j].IsWall)
                {
                    cube.layer = this.layerDefault;
                    cube.GetComponent<Block>().IsWall = true;
                    cube.transform.GetChild(0).gameObject.SetActive(true);
                    cube.transform.GetChild(1).gameObject.SetActive(false);
                    cube.transform.position += (Vector3.up * 0.8f);
                    cube.transform.GetChild(2).GetComponent<MeshRenderer>().material = this.highligtEdgesShader;
                    cube.transform.SetParent(this.BlockWalls);
                }
                else
                {
                    cube.layer = this.layerPath;
                    cube.GetComponent<Block>().IsWall = false;
                    cube.transform.GetChild(0).gameObject.SetActive(false);
                    cube.transform.GetChild(1).gameObject.SetActive(true);
                    cube.transform.GetChild(2).GetComponent<MeshRenderer>().material = this.coverShader;
                    cube.transform.SetParent(this.BlockPath);
                }

                this.RemoveBoundingWalls(tmp, cube.GetComponent<Block>(), maze);
            }
        }

        GameObject startCube = this.stage[start.Item1, start.Item2];
        GameObject goalCube = this.stage[goal.Item1, goal.Item2];

        startCube.transform.position = new Vector3(startCube.transform.position.x, -0.35f, startCube.transform.position.z);
        startCube.GetComponent<Block>().IsWall = false;
        startCube.transform.GetChild(0).gameObject.SetActive(false);
        startCube.transform.GetChild(1).gameObject.SetActive(true);

        goalCube.transform.position = new Vector3(goalCube.transform.position.x, -0.35f, goalCube.transform.position.z);
        goalCube.GetComponent<Block>().IsWall = false;
        goalCube.transform.GetChild(0).gameObject.SetActive(false);
        goalCube.transform.GetChild(1).gameObject.SetActive(true);

        this.startText.transform.position = new Vector3(start.Item1, 5.0f, start.Item2);
        this.goalText.transform.position = new Vector3(goal.Item1, 3.0f, goal.Item2);

        Stack<Tuple<int, int>> q = new Stack<Tuple<int, int>>();
        Stack<Tuple<int, int>> pathStack = new Stack<Tuple<int, int>>();

        Tuple<int, int> first = null;
        Tuple<int, int> second = null;
        Tuple<int, int> third = null;
        while (path.Count > 0)
        {
            var p = path.Pop();
            q.Push(p);
            pathStack.Push(p);
        }

        while(q.Count > 1)
        {
            var current = q.Pop();
            var next = q.Peek();
            Block b;
            if (current.Item1 < next.Item1)
            {
                b = this.stage[current.Item1, current.Item2].GetComponent<Block>();
                b.rightWall.GetComponent<Renderer>().enabled = false;
            }
            else if (current.Item1 > next.Item1)
            {
                b = this.stage[current.Item1 - 1, current.Item2].GetComponent<Block>();
                b.rightWall.GetComponent<Renderer>().enabled = false;
            }
            else if (current.Item2 < next.Item2)
            {
                b = this.stage[current.Item1, current.Item2].GetComponent<Block>();
                b.topWall.GetComponent<Renderer>().enabled = false;
            }
            else if (current.Item2 > next.Item2)
            {
                b = this.stage[current.Item1, current.Item2 - 1].GetComponent<Block>();
                b.topWall.GetComponent<Renderer>().enabled = false;
            }

            Debug.Log(current);
            this.currentPath.Add(new Vector3(current.Item1, 0.5f, current.Item2));
        }

        var last = q.Pop();
        this.currentPath.Add(new Vector3(last.Item1, 0.5f, last.Item2));

        int pathIndex = 0;
        this.lineRendererPoints.Add(new List<Vector3>());
        while (pathStack.Count > 2)
        {
            if (second == null)
            {
                first = pathStack.Pop();
            }
            else
            {
                first = second;
            }

            second = pathStack.Pop();
            third = pathStack.Peek();

            if (PathUtilities.Turns(first, second, third))
            {
                this.lineRendererPoints[pathIndex].Add(new Vector3(first.Item1, 0.5f, first.Item2));
                this.lineRendererPoints[pathIndex].Add(new Vector3(second.Item1, 0.5f, second.Item2));
                this.lineRendererPoints.Add(new List<Vector3>());
                pathIndex++;
            }
            else
            {
                this.lineRendererPoints[pathIndex].Add(new Vector3(first.Item1, 0.5f, first.Item2));
            }
        }

        first = second;
        second = pathStack.Pop();
        third = pathStack.Pop();
        if (PathUtilities.Turns(first, second, third))
        {
            if (pathIndex >= this.lineRendererPoints.Count)
            {
                this.lineRendererPoints.Add(new List<Vector3>());
                this.lineRendererPoints[pathIndex].Add(new Vector3(first.Item1, 0.5f, first.Item2));
                this.lineRendererPoints[pathIndex++].Add(new Vector3(second.Item1, 0.5f, second.Item2));
                this.lineRendererPoints.Add(new List<Vector3>());
                this.lineRendererPoints[pathIndex].Add(new Vector3(second.Item1, 0.5f, second.Item2));
                this.lineRendererPoints[pathIndex++].Add(new Vector3(third.Item1, 0.5f, third.Item2));
            }
            else
            {
                this.lineRendererPoints[pathIndex].Add(new Vector3(first.Item1, 0.5f, first.Item2));
                this.lineRendererPoints[pathIndex++].Add(new Vector3(second.Item1, 0.5f, second.Item2));
                this.lineRendererPoints.Add(new List<Vector3>());
                this.lineRendererPoints[pathIndex].Add(new Vector3(second.Item1, 0.5f, second.Item2));
                this.lineRendererPoints[pathIndex].Add(new Vector3(third.Item1, 0.5f, third.Item2));
            }
        }
        else
        {
            this.lineRendererPoints[pathIndex].Add(new Vector3(first.Item1, 0.5f, first.Item2));
            this.lineRendererPoints[pathIndex].Add(new Vector3(second.Item1, 0.5f, second.Item2));
            this.lineRendererPoints[pathIndex].Add(new Vector3(third.Item1, 0.5f, third.Item2));
        }

        this.lineRendererPoints.Add(
            new List<Vector3>{
                new Vector3(start.Item1, 0.5f, start.Item2),
                new Vector3(this.startText.transform.position.x, this.startText.transform.position.y - this.startText.GetComponent<TextMeshPro>().bounds.size.y, this.startText.transform.position.z),
            });

        this.lineRendererPoints.Add(
            new List<Vector3>{
                new Vector3(goal.Item1, 0.5f, goal.Item2),
                new Vector3(this.goalText.transform.position.x, this.goalText.transform.position.y - this.goalText.GetComponent<TextMeshPro>().bounds.size.y, this.goalText.transform.position.z),
            });

        foreach (var pointPath in this.lineRendererPoints)
        {
            var newLineRenderer = Instantiate(this.lineRenderer, this.LineRenderers);
            newLineRenderer.tag = "LineRenderer";
            newLineRenderer.GetComponent<LineRenderer>().positionCount = pointPath.Count;
            newLineRenderer.GetComponent<LineRenderer>().SetPositions(pointPath.ToArray());
        }
    }

    private void RemoveBoundingWalls(Tuple<int, int> current, Block b, Cell[,] maze)
    {
        if (b.IsWall)
        {
            b.topWall.GetComponent<Renderer>().enabled = false;
            b.rightWall.GetComponent<Renderer>().enabled = false;
        }
        else
        {
            if (current.Item2 >= (this.mapSize.Item2 - 1))
            {
                b.topWall.GetComponent<Renderer>().enabled = false;
            }
            else
            {
                //var topNeighbor = GameObject.Find($"Cube {current.Item1} {current.Item2 + 1}");
                var topNeighbor = this.stage[current.Item1, current.Item2 + 1];
                if (maze[current.Item1, current.Item2 + 1].IsWall)
                {
                    b.topWall.GetComponent<Renderer>().enabled = false;
                }
            }

            if (current.Item1 >= (this.mapSize.Item1 - 1))
            {
                b.rightWall.GetComponent<Renderer>().enabled = false;
            }
            else
            {
                //var rightNeighbor = GameObject.Find($"Cube {current.Item1 + 1} {current.Item2}");
                var rightNeighbor = this.stage[current.Item1 + 1, current.Item2];
                if (maze[current.Item1 + 1, current.Item2].IsWall)
                {
                    b.rightWall.GetComponent<Renderer>().enabled = false;
                }
            }
        }
    }

    private void Setup()
    {
        Debug.Log("Setting up!");
        for (int i = 0; i < mapSize.Item1; i++)
        {
            for (int j = 0; j < mapSize.Item2; j++)
            {
                //GameObject newBlock = GameObject.Find($"Cube {i} {j}");
                GameObject newBlock = this.stage[i, j];
                if (newBlock == null)
                {
                    newBlock = Instantiate(this.block, new Vector3(i, 1.35f, j), Quaternion.identity);
                    this.stage[i, j] = newBlock;
                }

                for (int k = 0; k < newBlock.transform.childCount; k++)
                {
                    newBlock.transform.GetChild(k).gameObject.SetActive(true);
                }
                newBlock.SetActive(true);
                newBlock.transform.SetParent(null);
                newBlock.GetComponent<Block>().IsInsideMaze = true;

                newBlock.tag = "Destroyable";
                newBlock.name = $"Cube {i} {j}";

                if (newBlock.GetComponent<Block>().topWall == null)
                {
                    GameObject topWall = Instantiate(this.horizontalWall, new Vector3(i, 0.44f, j + 0.5f), Quaternion.identity, this.Walls);
                    newBlock.GetComponent<Block>().topWall = topWall;
                }

                if (newBlock.GetComponent<Block>().rightWall == null)
                {
                    GameObject rightWall = Instantiate(this.verticalWall, new Vector3(i + 0.5f, 0.44f, j), Quaternion.identity, this.Walls);
                    newBlock.GetComponent<Block>().rightWall = rightWall;
                }

                if (i == 0)
                {
                    GameObject newTorch = Instantiate(this.torch, new Vector3(i - 0.9f, 1.35f, j), Quaternion.Euler(0, 90, 0), this.TorchesAndStuff);
                    GameObject tmpBlock = Instantiate(this.block, new Vector3(i - 0.9f, 0f, j), Quaternion.identity, this.TorchesAndStuff);
                    tmpBlock.transform.GetChild(2).GetComponent<Renderer>().material = this.defaultMaterial;

                    newTorch.tag = "Prop";
                    tmpBlock.tag = "Prop";
                }

                if (j == 0)
                {
                    GameObject newTorch = Instantiate(this.torch, new Vector3(i, 1.35f, j - 0.9f), Quaternion.identity, this.TorchesAndStuff);
                    GameObject tmpBlock = Instantiate(this.block, new Vector3(i, 0f, j - 0.9f), Quaternion.identity, this.TorchesAndStuff);
                    tmpBlock.transform.GetChild(2).GetComponent<Renderer>().material = this.defaultMaterial;

                    newTorch.tag = "Prop";
                    tmpBlock.tag = "Prop";
                }

                if (i == (mapSize.Item1 - 1))
                {
                    GameObject newTorch = Instantiate(this.torch, new Vector3(i + 0.9f, 1.35f, j), Quaternion.Euler(0, -90, 0), this.TorchesAndStuff);
                    GameObject tmpBlock = Instantiate(this.block, new Vector3(i + 0.9f, 0f, j), Quaternion.identity, this.TorchesAndStuff);
                    tmpBlock.transform.GetChild(2).GetComponent<Renderer>().material = this.defaultMaterial;

                    newTorch.tag = "Prop";
                    tmpBlock.tag = "Prop";
                }

                if (j == (mapSize.Item2 - 1))
                {
                    GameObject newTorch = Instantiate(this.torch, new Vector3(i, 1.35f, j + 0.9f), Quaternion.Euler(0, 180, 0), this.TorchesAndStuff);
                    GameObject tmpBlock = Instantiate(this.block, new Vector3(i, 0f, j + 0.9f), Quaternion.identity, this.TorchesAndStuff);
                    tmpBlock.transform.GetChild(2).GetComponent<Renderer>().material = this.defaultMaterial;

                    newTorch.tag = "Prop";
                    tmpBlock.tag = "Prop";
                }
            }
        }

        GameObject torchBase1 = Instantiate(this.brickVase, new Vector3(-1, 2.23f, -1), Quaternion.identity, this.TorchesAndStuff);
        GameObject torchBase2 = Instantiate(this.brickVase, new Vector3(-1, 2.23f, this.mapSize.Item2), Quaternion.identity, this.TorchesAndStuff);
        GameObject torchBase3 = Instantiate(this.brickVase, new Vector3(this.mapSize.Item1, 2.23f, this.mapSize.Item2), Quaternion.identity, this.TorchesAndStuff);
        GameObject torchBase4 = Instantiate(this.brickVase, new Vector3(this.mapSize.Item1, 2.23f, -1), Quaternion.identity, this.TorchesAndStuff);

        torchBase1.tag = "Prop";
        torchBase2.tag = "Prop";
        torchBase3.tag = "Prop";
        torchBase4.tag = "Prop";
    }

    public void OnSliderDimensionsChanged(Slider slider)
    {
        this.mapSize = Tuple.Create((int)slider.value, (int)slider.value);
    }

    private void Awake()
    {
        this.lineRendererPoints = new List<List<Vector3>>();
        this.currentPath = new List<Vector3>();
        this.layerDefault = LayerMask.NameToLayer("Default");
        this.layerPath = LayerMask.NameToLayer("Path");
    }

    // Update is called once per frame
    void Start()
    {
        this.mapSize = Tuple.Create(10, 10);
        this.stage = new GameObject[MAX_SIZE, MAX_SIZE];
    }

    private void DeactivateRemainingStuff()
    {
        if (this.mapSize.Item1 < this.currentMapSize.Item1)
        {
            for (int i = this.mapSize.Item1; i < this.currentMapSize.Item1; i++)
            {
                for (int j = 0; j < this.currentMapSize.Item2; j++)
                {
                    //GameObject tmp = GameObject.Find($"Cube {i} {j}");
                    GameObject tmp = this.stage[i, j];
                    tmp.GetComponent<Block>().IsInsideMaze = false;
                    for (int k = 0; k < tmp.transform.childCount; k++)
                    {
                        tmp.transform.GetChild(k).gameObject.SetActive(false);
                    }

                    tmp.GetComponent<Block>().topWall.GetComponent<Renderer>().enabled = false;
                    tmp.GetComponent<Block>().rightWall.GetComponent<Renderer>().enabled = false;

                    //tmp = GameObject.Find($"Cube {j} {i}");
                    tmp = this.stage[j, i];
                    tmp.GetComponent<Block>().IsInsideMaze = false;
                    for (int k = 0; k < tmp.transform.childCount; k++)
                    {
                        tmp.transform.GetChild(k).gameObject.SetActive(false);
                    }

                    tmp.GetComponent<Block>().topWall.GetComponent<Renderer>().enabled = false;
                    tmp.GetComponent<Block>().rightWall.GetComponent<Renderer>().enabled = false;
                }
            }
        }
    }

    private void AdjustCamera()
    {
        float xyPosition = (this.mapSize.Item1 - 1) / 2.0f;
        float planeSize = this.mapSize.Item1 / 10.0f;
        this.cameraLookAtObject.transform.position = new Vector3(xyPosition, 0, xyPosition);
        //this.plane.transform.position = new Vector3(xyPosition, 0.14f, xyPosition);
        //this.plane.transform.localScale = new Vector3(planeSize, planeSize, planeSize);
    }

    //private void BuildNavigationMesh()
    //{
    //    float xyPosition = (this.mapSize.Item1 - 1) / 2.0f;
    //    float planeSize = this.mapSize.Item1 / 10.0f;
    //    this.plane.transform.position = new Vector3(xyPosition, 0.16f, xyPosition);
    //    this.plane.transform.localScale = new Vector3(planeSize, planeSize, planeSize);
    //    this.plane.GetComponent<NavMeshSurface>().BuildNavMesh();
    //}

    public void SelectStage()
    {
        if (this.currentMapSize != null)
        {
            //this.BuildNavigationMesh();
            DontDestroyOnLoad(GameObject.Find("ObjectToNextScene"));

            for (int i = 0; i < this.BlockWalls.childCount; i++)
            {
                this.BlockWalls.GetChild(i).gameObject.AddComponent(typeof(Buildable));
            }

            for (int i = 0; i < this.BlockPath.childCount; i++)
            {
                this.BlockPath.GetChild(i).gameObject.AddComponent(typeof(Path));
            }

            SceneManager.LoadScene("Main");
        }
    }

    public List<Vector3> CurrentPath => this.currentPath;

    public void CreateNewRandomPath()
    {
        this.ClearLog();
        this.startText.SetActive(true);
        this.goalText.SetActive(true);
        if (this.currentMapSize == null)
        {
            this.plane.SetActive(true);
            this.AdjustCamera();
            this.Setup();
        }
        else
        {
            if (this.mapSize.Item1 != this.currentMapSize.Item1)
            {
                this.AdjustCamera();
                this.DestroyProps();
                this.DeactivateRemainingStuff();
                this.Setup();
            }
        }

        this.DeletePath();
        this.CreateRandomPath(this.mapSize);
    }
}
