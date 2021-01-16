using UnityEngine;

public class SimpleHighlight : MonoBehaviour
{
	private Renderer _renderer;
	public  Material highlightMaterial;
	private Material defaultMaterial;

	public Technique technique;

	// Use this for initialization
	void Start()
	{
		_renderer = GetComponent<Renderer>();
		defaultMaterial = _renderer.material;
		technique.onHover.AddListener(Highlight);
		technique.onUnhover.AddListener(Unhighlight);
		technique.onSelectObject.AddListener(PlaySelectSound);
	}

	private void Highlight()
	{
		if (technique.highlightedObject == gameObject)
		{
			print("highlight");
			_renderer.material = highlightMaterial;
		}
	}

	private void Unhighlight()
	{
		if (technique.highlightedObject == gameObject)
		{
			print("unhighlight");
			_renderer.material = defaultMaterial;
		}
	}

	private void PlaySelectSound()
	{
		if (technique.selectedObject == gameObject)
		{
			GetComponent<AudioSource>().Play();
		}
	}
}
