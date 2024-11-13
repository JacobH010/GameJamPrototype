using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinityCube : ScannerItems
{
    private SpriteRenderer imageComponent;
    private int stateSelector;

    public Sprite[] sprites;
    // Start is called before the first frame update
    void Start()
    {
        imageComponent = this.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void ArtifactAction()
    {
        base.ArtifactAction();
        stateSelector = Random.Range(0, sprites.Length);
        imageComponent.sprite = sprites[stateSelector];
    }
}
