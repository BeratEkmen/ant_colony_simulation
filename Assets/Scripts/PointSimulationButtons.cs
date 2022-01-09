using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSimulationButtons : MonoBehaviour
{

    private GameObject _minDistGO;
    private GameObject _pheromoneLinesGO;
    private GameObject _antsParentGO;


    private int _lastActiveAnt;

    public void MinDistButton()
    {
        _minDistGO = GameObject.Find("MinDistLines");


        for(int i = 0; i < _minDistGO.transform.childCount; i++)
        {

            _minDistGO.transform.GetChild(i).gameObject.SetActive(!_minDistGO.transform.GetChild(i).gameObject.activeSelf);

        }

    }

    public void AntsButton()
    {
        _antsParentGO = GameObject.Find("AntsParent");

        bool atLeastOneActive = false;


        for (int i = 0; i < _antsParentGO.transform.childCount; i++)
        {
            if (_antsParentGO.transform.GetChild(i).GetChild(1).gameObject.activeSelf)
            {
                _lastActiveAnt = i;
                atLeastOneActive = true;
                break;
            }

        }



        if (atLeastOneActive)
        {

            for (int i = 0; i < _antsParentGO.transform.childCount; i++)
            {

                _antsParentGO.transform.GetChild(i).GetChild(1).gameObject.SetActive(false);
            }



        }
        else
        {

            _antsParentGO.transform.GetChild(_lastActiveAnt).GetChild(1).gameObject.SetActive(true);


        }



    }


    public void PheromonesButton()
    {
        _pheromoneLinesGO = GameObject.Find("PheromoneLinesParent");


        for (int i = 0; i < _pheromoneLinesGO.transform.childCount; i++)
        {

            _pheromoneLinesGO.transform.GetChild(i).gameObject.SetActive(!_pheromoneLinesGO.transform.GetChild(i).gameObject.activeSelf);

        }
    }
   
}
