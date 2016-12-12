using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LightController2D : MonoBehaviour {

    public List<GameObject> Targets;

    private float m_Rnd;
    private RawImage _image;

    public float BaseIntensity = 0.5f;
    public float RandomIntensity = 0.5f;
    public float Speed = 1f;
    
    

    void Awake()
    {
        m_Rnd = Random.value * 100;
    }


    private void Update()
    {
        for (int i = 0; i < Targets.Count; i++)
        {
            if (Targets[i].GetComponent<Image>() != null)
            {
                Color c = Targets[i].GetComponent<Image>().color;
                c.r = c.g = c.b = BaseIntensity + (RandomIntensity * Mathf.PerlinNoise(m_Rnd + Time.time * Speed, m_Rnd + 1 + Time.time * Speed));
                Targets[i].GetComponent<Image>().color = c;
            }
            else if (Targets[i].GetComponent<RawImage>() != null)
            {
                Color c = Targets[i].GetComponent<RawImage>().color;
                c.r = c.g = c.b = BaseIntensity + (RandomIntensity * Mathf.PerlinNoise(m_Rnd + Time.time * Speed, m_Rnd + 1 + Time.time * Speed));
                Targets[i].GetComponent<RawImage>().color = c;
            }
            else if (Targets[i].GetComponent<Renderer>() != null)
            {
                Material[] materials = Targets[i].GetComponent<Renderer>().materials;

                for (int j = 0; j < materials.Length; j++)
                {
                    Color c = materials[j].GetColor("_Color");
                    c.r = c.g = c.b = BaseIntensity + (RandomIntensity * Mathf.PerlinNoise(m_Rnd + Time.time * Speed, m_Rnd + 1 + Time.time * Speed));
                    materials[j].SetColor("_Color", c);
                }

            }
        }
    }
}
