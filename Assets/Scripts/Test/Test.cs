using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class Test : MonoBehaviour
{
  private void Start()
  {
    SubmoduleClass submoduleClass = new SubmoduleClass();
    Debug.Log(submoduleClass);
  }
}
