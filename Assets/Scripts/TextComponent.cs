using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetText(string NewText)
    {
        GetComponent<UnityEngine.UI.Text>().text = NewText;
    }
}
