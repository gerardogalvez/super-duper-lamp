using System;
using System.Runtime.Remoting;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buildable : MonoBehaviour
{ 
    public bool IsBuilt { get; set; }

    private Renderer m_hoverableRenderer;

    private GameObject m_towerToBuild;

    public void Awake()
    {
        this.m_hoverableRenderer = this.transform.GetChild(2).GetComponent<Renderer>();
    }

    private void OnMouseOver()
    {
        if (!this.IsBuilt && this.m_towerToBuild == null)
        {
            this.m_hoverableRenderer.material.SetColor("Color_4322BE18", Color.red);
            this.ShowTowerPlaceholder();
        }
        else
        {
            this.m_towerToBuild.GetComponent<BaseTower>().HighlightBlocksInRange(Color.red);
        }
    }

    private void ShowTowerPlaceholder()
    {
        Type towerType = BuildMenu.instance.SelectedTowerType;
        BaseTower tower = (BaseTower)Activator.CreateInstance(towerType);
        this.m_towerToBuild = tower.BuildTower(this.gameObject.transform.position + Vector3.up);
        this.m_towerToBuild.GetComponent<Collider>().enabled = false;
        Color color = this.m_towerToBuild.GetComponent<MeshRenderer>().material.GetColor("_Color");
        color.a = 0.35f;
        this.m_towerToBuild.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        this.m_towerToBuild.GetComponent<BaseTower>().SetBlocksInRange(this.gameObject.transform.position);
        this.m_towerToBuild.GetComponent<BaseTower>().HighlightBlocksInRange(Color.red);
    }

    private void OnMouseExit()
    {
        this.m_hoverableRenderer.material.SetColor("Color_4322BE18", Color.clear);
        this.m_towerToBuild.GetComponent<BaseTower>().HighlightBlocksInRange(Color.clear);
        if (!this.IsBuilt)
        {
            Destroy(this.m_towerToBuild);
            this.m_towerToBuild = null;
        }
    }

    private void BuildTowerPlaceholder()
    {
        this.m_towerToBuild.GetComponent<Collider>().enabled = true;
        Color color = this.m_towerToBuild.GetComponent<MeshRenderer>().material.GetColor("_Color");
        color.a = 1.0f;
        this.m_towerToBuild.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
    }

    private void OnMouseDown()
    {
        // Handle build tower
        if (!this.IsBuilt)
        {
            this.BuildTowerPlaceholder();
            this.IsBuilt = true;
            Debug.Log($"Building {BuildMenu.instance.SelectedTowerType} at {this.gameObject.name}");
        }
    }
}
