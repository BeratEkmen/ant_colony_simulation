using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSimulation : MonoBehaviour
{

    private class Ant
    {

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

            for(int i = 0; i < visitedIndexes.Length; i++)
            {
                visitedIndexes[i] = -1;
            }

        }


    }



    [SerializeField] private GameObject _pointPrefab;
    [SerializeField] private GameObject _linePrefab;

    [SerializeField] private MeshRenderer _quad;

    [SerializeField] private int _numOfPoints;

    [SerializeField] private GameObject _ant;

    [SerializeField] private Material _visitedMat;


    private Vector3[] _points;

    private float[,] _distanceMatrix;
    private float[,] _pheromoneTrails;
    private float _minDist;


    private GameObject _linesParent;
    private GameObject _pointsParent;


    private bool _isMoving;


    private List<Ant> antList;

    private void Start()
    {
        antList = new List<Ant>();

        

        _pheromoneTrails = new float[_numOfPoints, _numOfPoints];


        for (int i = 0; i < _numOfPoints; i++)
        {
            for (int j = 0; j < _numOfPoints; j++)
            {

                _pheromoneTrails[i, j] = 1;


            }

        }

        _isMoving = false;

        _minDist = -1;
        

        SpawnPoints();
        CalculateDistanceMatrix();
        

        _linesParent = new GameObject("Lines");

        /*for(int i = 0; i < antList.Count; i++)
        {
            GameObject newAnt = Instantiate(_ant, Vector3.zero, Quaternion.identity);

            newAnt.transform.position = _points[antList[i].antPositionIndex];

            newAnt.transform.position = new Vector3(newAnt.transform.position.x, newAnt.transform.position.y, -4.5f);


            //DrawLines(antList[i], antList[i].antPositionIndex);
        }*/

        


        
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
            pheromoneStrength = _pheromoneTrails[index, i];

            if (Array.Exists(ant.visitedIndexes, element => element == i))
            {
                dst = 0;
            }




            ant.desirabilityArr[i] = dst == 0 ? 0 : Mathf.Pow(1 / dst, 4f) * Mathf.Pow(pheromoneStrength, 1f);

        }


    }



    private bool _isCalculating;

    private void Update()
    {

        if(Input.GetKeyDown(KeyCode.Space) && !_isCalculating)
        {
            _isCalculating = true;

            antList.Clear();

            for(int i = 0; i < _linesParent.transform.childCount; i++)
            {
                Destroy(_linesParent.transform.GetChild(i).gameObject);
            }


            for (int i = 0; i < 5; i++)
            {
                antList.Add(new Ant(_numOfPoints));
                CalculateDesirability(antList[i], antList[i].antPositionIndex);

            }


            for (int i = 0; i < antList.Count; i++)
            {
                CalculateAntPathDistance(antList[i]);

            }

            /*for(int i = 0; i < antList.Count; i++)
            {
                DrawFinalRoad(antList[i]);
            }*/


            for (int i = 0; i < _numOfPoints; i++)
            {
                for (int j = 0; j < _numOfPoints; j++)
                {

                    _pheromoneTrails[i, j] = _pheromoneTrails[i, j] * 0.3f;


                }

            }

            _isCalculating = false;
        }



        

    }

    private void CalculateAntPathDistance(Ant ant)
    {

        if (_isMoving)
        {
            return;
        }

        while(Mathf.Min(ant.visitedIndexes) == -1)
        {
            


            //_isMoving = true;

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


                    ant.antPositionIndex = i;

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

    private void DrawFinalRoad(Ant ant)
    {
        
        




        

        /*for (int i = 0; i < _linesParent.transform.childCount; i++)
        {
            Destroy(_linesParent.transform.GetChild(i).gameObject);

        }*/

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


    }

    private IEnumerator MoveAnt(Ant ant, int index)
    {

        /*float elapsedTime = 0;

        float moveTime = .2f;

        while(elapsedTime < moveTime)
        {

            _ant.transform.position = Vector3.Lerp(_points[ant1.antPositionIndex], _points[index], elapsedTime / moveTime);

            _ant.transform.position = new Vector3(_ant.transform.position.x, _ant.transform.position.y, -4.5f);

            elapsedTime += Time.deltaTime;

            yield return null;
        }*/

        _ant.transform.position = _points[index];
        _ant.transform.position = new Vector3(_ant.transform.position.x, _ant.transform.position.y, -4.5f);


        ant.antPositionIndex = index;

        CalculateDesirability(ant, ant.antPositionIndex);
        //DrawLines(ant, ant.antPositionIndex);

        _isMoving = false;

        yield return null;

    }
    private void DrawLines(Ant ant, int index)
    {
        _pointsParent.transform.GetChild(index).gameObject.GetComponent<Renderer>().material = _visitedMat;


        /*for (int i = 0; i < _linesParent.transform.childCount; i++)
        {
            Destroy(_linesParent.transform.GetChild(i).gameObject);

        }*/



        for (int i = 0; i < _numOfPoints; i++)
        {




            GameObject line = Instantiate(_linePrefab, Vector3.zero, Quaternion.identity, _linesParent.transform);
            line.name = "Line " + i;

            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

            lineRenderer.SetPosition(0, _points[index]);
            lineRenderer.SetPosition(1, _points[i]);





            lineRenderer.endWidth = .05f;
            lineRenderer.startWidth = .05f;

            float alphaValue = Mathf.Lerp(0, 1, ant.desirabilityArr[i] / Mathf.Max(ant.desirabilityArr));//desirability[i] * .08f;

            lineRenderer.startColor = new Color(1, 1, 1, alphaValue);
            lineRenderer.endColor = new Color(1, 1, 1, alphaValue);

        }

    }

}
