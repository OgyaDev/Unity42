using UnityEngine;
using Fusion.Menu;

public abstract class MenuConnectionPlugin : MonoBehaviour
{
	public abstract IFusionMenuConnection Create(MenuConnectionBehaviour connectionBehaviour);
}

