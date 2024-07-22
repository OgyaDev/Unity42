using UnityEngine;

	public class MenuUI : MenuUIController
	{
		public override void OnGameStarted()
		{
			base.OnGameStarted();

			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(false);
			}
		}

		public override void OnGameStopped()
		{
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(true);
			}

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			base.OnGameStopped();
		}

		protected override void Awake()
		{
			base.Awake();

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
