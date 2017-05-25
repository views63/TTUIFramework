using UnityEngine;

public class Block
{
    public GameObject Go;
    public Transform Tr;

    protected void Init(Block block, Transform tr)
    {
        Tr = tr;
        Go = tr.gameObject;
        InjectorView.AutoInject(block);
    }
}

