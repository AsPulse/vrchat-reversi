
using UdonSharp;
using UnityEngine;
public class ReversiDisk : UdonSharpBehaviour
{
    [SerializeField]
    private float Radius;

    private bool _color;

    public bool Color {
        get => _color;
        set {
            animator.SetBool("Color", value);
            _color = value;
        }
    }

    private GameObject ReversiLogic;
    private Animator animator;

    public void Setup(GameObject logic, bool initialColor)
    {
        animator = GetComponent<Animator>();
        ReversiLogic = logic;
        Color = initialColor;
        transform.parent = logic.transform;
    }
}
