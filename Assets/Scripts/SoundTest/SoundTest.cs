using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioLib;

public class SoundTest : MonoBehaviour
{


    public void TestBgm()
    {
        // Play BGM, tự cross-fade nếu đang có track khác
        AudioManager.Instance.PlayBgm(BgmID.Gameplay);
    }
}
