
using UdonSharp;
using UnityEngine;
public class ReversiDisk : UdonSharpBehaviour
{
    private bool _color = true;

    public bool Color {
        get => _color;
        set {
            if (value == _color) return;
            animator.SetBool("Color", value);
            _color = value;
        }
    }

    private GameObject ReversiLogic;
    private Animator animator;

    public void Setup(GameObject logic, Vector3 position, bool initialColor)
    {
        animator = GetComponentInChildren<Animator>();
        ReversiLogic = logic;
        Color = initialColor;
        transform.parent = logic.transform;
        transform.Translate(logic.transform.position + position, Space.World);
    }

    public override void Interact()
    {
        Color = !Color;
    }

}
