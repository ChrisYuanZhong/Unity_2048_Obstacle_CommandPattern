using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public TileState state { get; private set; }
    public TileCell cell { get; private set; }
    public int number { get; private set; }
    public bool locked { get; set; }

    [HideInInspector]
    public float animationDuration = 0.1f;

    private Image background;
    private TextMeshProUGUI numberText;

    private void Awake()
    {
        background = GetComponent<Image>();
        numberText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetState(TileState state, int number)
    {
        this.state = state;
        this.number = number;

        background.color = state.backgroundColor;
        numberText.color = state.textColor;
        numberText.text = number.ToString();
    }

    public void Spawn(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        transform.position = cell.transform.position;
    }

    public void MoveTo(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        StartCoroutine(Animate(cell.transform.position, false));
    }

    public void MergeWith(TileCell other)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = null;
        other.tile.locked = true;

        StartCoroutine(Animate(other.transform.position, true));
    }

    private IEnumerator Animate(Vector3 to, bool merging)
    {
        float elapsedTime = 0f;

        Vector3 from = transform.position;

        while (elapsedTime < animationDuration)
        {
            transform.position = Vector3.Lerp(from, to, elapsedTime / animationDuration);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = to;

        if (merging)
        {
            Destroy(gameObject);
        }
    }
}
