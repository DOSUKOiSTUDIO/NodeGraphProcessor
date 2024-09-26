using GraphProcessor;
using TNRD;
using UnityEngine;

[System.Serializable, NodeMenuItem("Dosukoi")]
public class DosukoiNode : BaseNode
{
    [SerializeField] public SerializableInterface<IA> _a;

    public override string		name => nameof(DosukoiNode);
}