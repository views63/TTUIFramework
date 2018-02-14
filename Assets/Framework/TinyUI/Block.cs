using UnityEngine;

namespace TinyUI
{
    public class Block
    {
        public GameObject Go { private set; get; }
        public Transform Tr { private set; get; }

        protected void Init(Block block, Transform tr)
        {
            Tr = tr;
            Go = tr.gameObject;
            InjectorView.AutoInject(block);
        }
    }
}
