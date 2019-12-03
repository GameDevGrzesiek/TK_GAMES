using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DependencyInjectionTest : MonoBehaviour
{
    [Inject(InjectFrom.Anywhere)]
    public Button TestBtn;

    void Start()
    {
        TestBtn.onClick.AddListener(delegate { OnButtonPress(); });
    }

    void OnButtonPress()
    {
        Debug.Log("I'm pressed");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
