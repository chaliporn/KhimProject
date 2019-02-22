using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SheetMusicView : MonoBehaviour
{
    public NoteChart SongViewNote ;
    public int ActivePage ;
    public Image NoteSong ;
    


    // Start is called before the first frame update
    public void pressL () {
        ActivePage = ActivePage -1;
        if (ActivePage<0) {
           ActivePage = 0;
        } 
    }

    public void pressR () {
        ActivePage = ActivePage +1;
         if (ActivePage>= SongViewNote.sheetMusics.Length) {
           ActivePage = SongViewNote.sheetMusics.Length -1;
        }
    }

    

    public void ActiveImage (){
       Sprite trueActivePage = SongViewNote.sheetMusics[ActivePage];
       NoteSong.sprite = trueActivePage;
    }


  

}
