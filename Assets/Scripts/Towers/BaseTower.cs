using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public abstract class BaseTower : MonoBehaviour {

    protected List<GameObject> blocksInRange;

    protected string TowerDirectory = "Towers/Sprites/";

    protected float m_FireRate;

    protected float m_Range;

    protected abstract string TowerPrefabName { get; }

    public abstract Sprite Sprite { get; }

    public abstract Sprite PressedSprite { get; }

    public abstract Sprite HighlightedSprite { get; }

    public abstract Sprite DisabledSprite { get; }

    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract int Index { get; }

    public float FireRate { get { return this.m_FireRate; } set { this.m_FireRate = value; } }

    protected abstract float BaseFireRate { get; }

    protected abstract float BaseRange { get; }

    public abstract GameObject BuildTower(Vector3 position);

    protected virtual void Awake()
    {
        this.blocksInRange = new List<GameObject>();
        this.m_FireRate = this.BaseFireRate;
        this.m_Range = this.BaseRange;
        Debug.Log("BaseTower::Start called by " + this.gameObject.name);
    }

    public void HighlightBlocksInRange(Color color)
    {
        foreach (var block in this.blocksInRange)
        {
            block.transform.GetChild(2).GetComponent<MeshRenderer>().material.SetColor("Color_4322BE18", color);
        }
    }

    public void SetBlocksInRange(Vector3 sphereOrigin)
    {
        // Esto está regresando más basura
        Collider[] hitColliders = Physics.OverlapSphere(sphereOrigin, this.m_Range, 1 << LayerMask.NameToLayer("Path"));
        this.blocksInRange.Clear();
        foreach (var coll in hitColliders)
        {
            this.blocksInRange.Add(coll.gameObject);
        }
    }

    private void OnMouseOver()
    {
        this.HighlightBlocksInRange(Color.red);
    }

    private void OnMouseExit()
    {
        this.HighlightBlocksInRange(Color.clear);
    }

}
