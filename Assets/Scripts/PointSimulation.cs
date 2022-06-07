using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSimulation : MonoBehaviour
{

    private class Ant
    {
        public bool antCalculating;

        public bool isMoving;
        public int[] visitedIndexes;
        public float[] desirabilityArr;
        public int antPositionIndex;
        public float pathDistance;

        public Ant(int numOfPoints)
        {
            visitedIndexes = new int[numOfPoints];
            desirabilityArr = new float[numOfPoints];
            antPositionIndex = UnityEngine.Random.Range(0, numOfPoints);
            pathDistance = 0;
            isMoving = false;
            for(int i = 0; i < visitedIndexes.Length; i++)
            {
                visitedIndexes[i] = -1;
            }

        }


    }


    [SerializeField] private GameObject _antPrefab;
    [SerializeField] private GameObject _pointPrefab;
    [SerializeField] private GameObject _linePrefab;

    [SerializeField] private MeshRenderer _quad;

    [SerializeField] private int _numOfPoints;
    [SerializeField] private int _numOfAntsPerIteration;

    [SerializeField] private float _moveTime;

    //[SerializeField] private GameObject _ant;

    //[SerializeField] private Material _visitedMat;


    private Vector3[] _points;

    private float[,] _distanceMatrix;
    private float[,] _pheromoneTrails;
    private float _minDist;

    private bool _isCalculating;


    private GameObject _pointsParent;
    private GameObject _antParent;

    private GameObject _pheromoneLinesParent;

    private List<Ant> antList;

    private void Start()
    {
        antList = new List<Ant>();

        _antParent = new GameObject("AntsParent");

        for(int i = 0; i < _numOfAntsPerIteration; i++)
        {

            Instantiate(_antPrefab, _antParent.transform).SetActive(false);

        }



        SpawnPoints();
        CalculateDistanceMatrix();







        _pheromoneTrails = new float[_numOfPoints, _numOfPoints];


        for (int i = 0; i < _numOfPoints; i++)
        {
            for (int j = 0; j < _numOfPoints; j++)
            {

                _pheromoneTrails[i, j] = 1;


            }

        }

        _pheromoneLinesParent = new GameObject("PheromoneLinesParent");
        InitializePheromoneLines();

        _minDist = -1;
        

        

        /*for(int i = 0; i < antList.Count; i++)
        {
            GameObject newAnt = Instantiate(_ant, Vector3.zero, Quaternion.identity);

            newAnt.transform.position = _points[antList[i].antPositionIndex];

            newAnt.transform.position = new Vector3(newAnt.transform.position.x, newAnt.transform.position.y, -4.5f);


            //DrawLines(antList[i], antList[i].antPositionIndex);
        }*/

        


        
    }

    private void InitializePheromoneLines()
    {

        for(int i = 0; i < _numOfPoints; i++)
        {
            for (int j = i + 1; j < _numOfPoints; j++)
            {

                int fromIndex = i;
                int toIndex = j;




                GameObject line = Instantiate(_linePrefab, Vector3.zero, Quaternion.identity, _pheromoneLinesParent.transform);
                line.name = "Line " + i + "," + j;

                LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

                lineRenderer.SetPosition(0, _points[fromIndex]);
                lineRenderer.SetPosition(1, _points[toIndex]);





                lineRenderer.endWidth = .05f;
                lineRenderer.startWidth = .05f;

                float alphaValue = _pheromoneTrails[i, j];



                lineRenderer.startColor = new Color(1, 1, 0, alphaValue);
                lineRenderer.endColor = new Color(1, 1, 0, alphaValue);

            }
        }
        
    }

    private void SpawnPoints()
    {
        _points = new Vector3[_numOfPoints];

        float xMin = _quad.bounds.min.x + .5f;
        float xMax = _quad.bounds.max.x - .5f;
        float yMin = _quad.bounds.min.y + .5f;
        float yMax = _quad.bounds.max.y - .5f;

        _pointsParent = new GameObject("Points");


        for (int i = 0; i < _numOfPoints; i++)
        {
            float xPos = UnityEngine.Random.Range(xMin, xMax);
            float yPos = UnityEngine.Random.Range(yMin, yMax);

            Vector3 pos = new Vector3(xPos, yPos, -0.3f);

            Instantiate(_pointPrefab, pos, Quaternion.identity, _pointsParent.transform).name = "Point " + i;

            _points[i] = pos;
        }
    }

    private void CalculateDistanceMatrix()
    {
        _distanceMatrix = new float[_numOfPoints, _numOfPoints];



        for (int i = 0; i < _numOfPoints; i++)
        {
            for (int j = 0; j < _numOfPoints; j++)
            {

                float dst = Vector3.Distance(_points[i], _points[j]);

                _distanceMatrix[i, j] = dst;


            }

        }
    }


    private void CalculateDesirability(Ant ant, int index)
    {

        int nextEmptyIndex = Array.IndexOf(ant.visitedIndexes, -1);



        ant.visitedIndexes[nextEmptyIndex] = index;

        /*float minPheromone = 1;

        for(int i = 0; i < _numOfPoints; i++)
        {
            if(_pheromoneTrails[index, i] != 0)
            {
                if(_pheromoneTrails[index, i] < minPheromone)
                {
                    minPheromone = _pheromoneTrails[index, i];
                }

            }

        }*/


        for (int i = 0; i < _numOfPoints; i++)
        {
            float dst = _distanceMatrix[index, i];


            float pheromoneStrength = _pheromoneTrails[index, i];

            if (Array.Exists(ant.visitedIndexes, element => element == i))
            {
                dst = 0;
            }




            ant.desirabilityArr[i] = dst == 0 ? 0 : Mathf.Pow(1 / dst, 4f) * Mathf.Pow(pheromoneStrength, 1f) * 100f;

        }


    }



    private void Update()
    {




        if(!_isCalculating && Input.GetKeyDown(KeyCode.Space))
        {
            _isCalculating = true;


            for(int i = 0; i < _antParent.transform.childCount; i++)
            {
                for(int j = 0; j < _antParent.transform.GetChild(i).GetChild(1).childCount; j++)
                {
                    Destroy(_antParent.transform.GetChild(i).GetChild(1).GetChild(j).gameObject);

                }
            }


            antList.Clear();


            for (int i = 0; i < _numOfAntsPerIteration; i++)
            {
                antList.Add(new Ant(_numOfPoints));

                _antParent.transform.GetChild(i).GetChild(0).position = _points[antList[i].antPositionIndex] + Vector3.back * .1f;
                _antParent.transform.GetChild(i).gameObject.SetActive(true);

                CalculateDesirability(antList[i], antList[i].antPositionIndex);

            }


            for (int i = 0; i < antList.Count; i++)
            {
                StartCoroutine(CalculateAntPathDistance(antList[i], i));

            }

            /*for(int i = 0; i < antList.Count; i++)
            {
                DrawFinalRoad(antList[i]);
            }*/


            
        }

        if (_isCalculating)
        {
            for(int i = 0; i < antList.Count; i++)
            {
                if (antList[i].antCalculating)
                {

                    break;
                }


                if(i == antList.Count - 1)
                {

                    CalculationEnded();
                }


            }



        }

        UpdatePheromoneTrails();


        CheckMouseInput();



    }

    private void CheckMouseInput()
    {
        if (Input.GetMouseButton(0))
        {

            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo = new RaycastHit();

            if(Physics.Raycast(mouseRay, out hitInfo))
            {

                if(hitInfo.transform.tag == "Ant")
                {
                    if (!_isCalculating)
                    {
                        for(int i = 0; i < _antParent.transform.childCount; i++)
                        {
                            if(_antParent.transform.GetChild(i).GetChild(0).gameObject != hitInfo.transform.gameObject)
                            {

                                _antParent.transform.GetChild(i).GetChild(1).gameObject.SetActive(false);
                                _antParent.transform.GetChild(i).GetChild(1).transform.localPosition = Vector3.zero + Vector3.back * 5f;

                            }
                            else
                            {
                                _antParent.transform.GetChild(i).GetChild(1).gameObject.SetActive(true);
                                //_antParent.transform.GetChild(i).transform.position += Vector3.forward * .1f;
                                _antParent.transform.GetChild(i).GetChild(1).transform.localPosition = Vector3.zero + Vector3.back * 10f;

                            }

                        }

                    }

                }


            }

        }


    }

    private void UpdatePheromoneTrails()
    {

        int childIndex = 0;

        for (int i = 0; i < _numOfPoints; i++)
        {
            for (int j = i + 1; j < _numOfPoints; j++)
            {






                GameObject line = _pheromoneLinesParent.transform.GetChild(childIndex).gameObject;

                LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

                






                float alphaValue = _pheromoneTrails[i, j];



                lineRenderer.startColor = new Color(1, 1, 0, alphaValue);
                lineRenderer.endColor = new Color(1, 1, 0, alphaValue);


                childIndex++;
            }
        }


    }

    private void CalculationEnded()
    {






        for (int k = 0; k < _numOfPoints; k++)
        {
            for (int j = 0; j < _numOfPoints; j++)
            {

                _pheromoneTrails[k, j] = _pheromoneTrails[k, j] * 0.3f;


            }

        }

        //update pheromones

        float maxPheromoneStrength = 0f;


        for(int i = 0; i < antList.Count; i++)
        {
            Ant currAnt = antList[i];

            float incAmount = Mathf.Lerp(.5f, 1f, _minDist / currAnt.pathDistance);

            for(int j = 0; j < currAnt.visitedIndexes.Length; j++)
            {

                int fromIndex = currAnt.visitedIndexes[j];
                int toIndex = currAnt.visitedIndexes[(j + 1) % _numOfPoints];



                _pheromoneTrails[fromIndex, toIndex] += incAmount;
                _pheromoneTrails[toIndex, fromIndex] = _pheromoneTrails[fromIndex, toIndex];

                if(_pheromoneTrails[fromIndex, toIndex] > maxPheromoneStrength)
                {
                    maxPheromoneStrength = _pheromoneTrails[fromIndex, toIndex];
                }
            }

        }



        for (int k = 0; k < _numOfPoints; k++)
        {
            for (int j = 0; j < _numOfPoints; j++)
            {

                _pheromoneTrails[k, j] = _pheromoneTrails[k, j] / maxPheromoneStrength;


            }

        }

        _isCalculating = false;
    }


    private IEnumerator CalculateAntPathDistance(Ant ant, int antIndex)
    {

        if (ant.isMoving)
        {
            yield return null;
        }


        ant.antCalculating = true;


        while(Mathf.Min(ant.visitedIndexes) == -1)
        {

            if (ant.isMoving)
            {
                yield return null;
                continue;
            }

            ant.isMoving = true;

            float randomRange = 0;




            for (int i = 0; i < ant.desirabilityArr.Length; i++)
            {
                randomRange += ant.desirabilityArr[i];
            }



            float random = UnityEngine.Random.Range(0, randomRange);

            for (int i = 0; i < ant.desirabilityArr.Length; i++)
            {
                if (random < ant.desirabilityArr[i])
                {
                    //StartCoroutine(MoveAnt(i));


                    int prevPointIndex = ant.antPositionIndex;

                    ant.antPositionIndex = i;

                    StartCoroutine(MoveAntCoroutine(ant, antIndex, prevPointIndex));



                    CalculateDesirability(ant, ant.antPositionIndex);

                    break;
                }

                random -= ant.desirabilityArr[i];
            }
        }

        //Calculate ants paths distance
        for(int i = 0; i < ant.visitedIndexes.Length; i++)
        {
            int fromIndex = ant.visitedIndexes[i];
            int toIndex = ant.visitedIndexes[(i + 1) % ant.visitedIndexes.Length];

            ant.pathDistance += _distanceMatrix[fromIndex, toIndex];



        }


        if (_minDist == -1)
        {
            _minDist = ant.pathDistance;


            DrawMinDistance(ant);


        }
        else
        {
            if (_minDist > ant.pathDistance)
            {


                _minDist = ant.pathDistance;
                DrawMinDistance(ant);




            }
        }


        ant.antCalculating = false;



    }
    
    
       

    private void DrawMinDistance(Ant ant)
    {


        if (GameObject.Find("MinDistLines") != null)
        {
            DestroyImmediate(GameObject.Find("MinDistLines"));


        }


        GameObject lineParent = new GameObject("MinDistLines");


        

        for (int i = 0; i < _numOfPoints; i++)
        {

            int fromIndex = ant.visitedIndexes[i];
            int toIndex = ant.visitedIndexes[(i + 1) % _numOfPoints];

            


            GameObject line = Instantiate(_linePrefab, Vector3.zero, Quaternion.identity, lineParent.transform);
            line.name = "Line " + i;

            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

            lineRenderer.SetPosition(0, _points[fromIndex]);
            lineRenderer.SetPosition(1, _points[toIndex]);





            lineRenderer.endWidth = .05f;
            lineRenderer.startWidth = .05f;

            float alphaValue = 1;



            lineRenderer.startColor = new Color(1, 1, 1, alphaValue);
            lineRenderer.endColor = new Color(1, 1, 1, alphaValue);

        }


    }

    /*private void DrawFinalRoad(Ant ant)
    {
        
        




        

        for (int i = 0; i < _linesParent.transform.childCount; i++)
        {
            Destroy(_linesParent.transform.GetChild(i).gameObject);

        }

        GameObject lineParent = new GameObject("FinalLine");


        lineParent.transform.parent = _linesParent.transform;

        for (int i = 0; i < _numOfPoints; i++)
        {

            int fromIndex = ant.visitedIndexes[i];
            int toIndex = ant.visitedIndexes[(i + 1) % _numOfPoints];

            //_pheromoneTrails[fromIndex, toIndex] = Mathf.Max(_pheromoneTrails[fromIndex, toIndex], Mathf.Lerp(0, 1, _minDist / ant.pathDistance));
            _pheromoneTrails[fromIndex, toIndex] += 100f / ant.pathDistance;
            _pheromoneTrails[toIndex, fromIndex] = _pheromoneTrails[fromIndex, toIndex];


            GameObject line = Instantiate(_linePrefab, Vector3.zero, Quaternion.identity, lineParent.transform);
            line.name = "Line " + i;

            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

            lineRenderer.SetPosition(0, _points[fromIndex]);
            lineRenderer.SetPosition(1, _points[toIndex]);





            lineRenderer.endWidth = .05f;
            lineRenderer.startWidth = .05f;

            float alphaValue = Mathf.Lerp(0, 1, _minDist / ant.pathDistance);

            alphaValue = alphaValue == 1 ? 1 : alphaValue * .1f;


            lineRenderer.startColor = new Color(1, 1, 1, alphaValue);
            lineRenderer.endColor = new Color(1, 1, 1, alphaValue);

        }


    }*/

    private IEnumerator MoveAntCoroutine(Ant ant, int index, int prevPointIndex)
    {

        float elapsedTime = 0;


        GameObject antGO = _antParent.transform.GetChild(index).GetChild(0).gameObject;

        Vector3 initPos = antGO.transform.position;

        while(elapsedTime < _moveTime)
        {

            antGO.transform.position = Vector3.Lerp(initPos, _points[ant.antPositionIndex], elapsedTime / _moveTime);

            antGO.transform.position += Vector3.back * .1f;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        antGO.transform.position = _points[ant.antPositionIndex];
        antGO.transform.position += Vector3.back * .1f;




        //CalculateDesirability(ant, ant.antPositionIndex);
        //DrawLines(ant, ant.antPositionIndex);

        yield return new WaitForSeconds(.1f);

        ant.isMoving = false;

        DrawAntPath(ant, index, prevPointIndex);

        //yield return null;

    }
    private void DrawAntPath(Ant ant, int index, int prevPointIndex)
    {


        /*for (int i = 0; i < _linesParent.transform.childCount; i++)
        {
            Destroy(_linesParent.transform.GetChild(i).gameObject);

        }*/


        GameObject line = Instantiate(_linePrefab, Vector3.zero, Quaternion.identity, _antParent.transform.GetChild(index).GetChild(1).transform);
        line.name = "Line ";
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, _points[prevPointIndex]);
        lineRenderer.SetPosition(1, _points[ant.antPositionIndex]);


        lineRenderer.endWidth = .05f;
        lineRenderer.startWidth = .05f;


        lineRenderer.startColor = new Color(1, 0, 0, 1);
        lineRenderer.endColor = new Color(1, 0, 0, 1);


        if (_antParent.transform.GetChild(index).GetChild(1).childCount == _numOfPoints - 1)
        {
            GameObject finalLine = Instantiate(_linePrefab, Vector3.zero, Quaternion.identity, _antParent.transform.GetChild(index).GetChild(1).transform);
            line.name = "Line ";
            LineRenderer finalLineRenderer = finalLine.GetComponent<LineRenderer>();

            finalLineRenderer.SetPosition(0, _points[ant.antPositionIndex]);
            finalLineRenderer.SetPosition(1, _points[ant.visitedIndexes[0]]);


            finalLineRenderer.endWidth = .05f;
            finalLineRenderer.startWidth = .05f;


            finalLineRenderer.startColor = new Color(1, 0, 0, 1);
            finalLineRenderer.endColor = new Color(1, 0, 0, 1);
        }

    }

}
