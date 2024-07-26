using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UI : MonoBehaviour
{
    
      void Start()
    {
       LockCursor();
    }

void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
       {
          LockCursor();
       }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

    }
    
   void LockCursor()
   {
       Cursor.lockState = CursorLockMode.Locked;
       Cursor.visible = false;
   }

   void UnlockCursor()
 {
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
 }
}
