using System;
using GraphProcessor;
using TNRD;
using UnityEngine;

[Serializable,NodeMenuItem(nameof(TestNode_A))]
public class TestNode_A : BaseNode
{
    [SerializeField]
    public Dog _Dog;

    [SerializeField] public float _A;

    [SerializeReference] public IAnimal _a;
    
    [SerializeField]
    public SerializableInterface<IAnimal> _animal = new();
}