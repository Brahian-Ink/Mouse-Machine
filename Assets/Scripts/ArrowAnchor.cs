using UnityEngine;

public class ArrowAnchor : MonoBehaviour
{
    [SerializeField] private Transform arrow;
    [SerializeField] private SpriteRenderer suitSprite;   // si usás flipX
    [SerializeField] private bool invertFlipLogic = false;

    // Posición base cuando mira a la izquierda
    [SerializeField] private Vector2 baseLocalPos = new Vector2(-0.0f, 0.9f);

    void Awake()
    {
        if (arrow == null) arrow = transform;
    }

    void LateUpdate()
    {
        bool facingLeft = suitSprite != null && suitSprite.flipX;
        if (invertFlipLogic) facingLeft = !facingLeft;

        Vector2 pos = baseLocalPos;
        if (!facingLeft) pos.x = -baseLocalPos.x; // si mira derecha, reflejar X

        arrow.localPosition = pos;
    }
}
