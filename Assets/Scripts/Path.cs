using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    private Renderer m_hoverableRenderer;

    public void Awake()
    {
        this.m_hoverableRenderer = this.transform.GetChild(2).GetComponent<Renderer>();
    }

    private void OnMouseOver()
    {
        this.m_hoverableRenderer.material.SetColor("Color_4322BE18", Color.red);
    }

    private void OnMouseExit()
    {
        this.m_hoverableRenderer.material.SetColor("Color_4322BE18", Color.clear);
    }
}
