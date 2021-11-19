using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSimulation : MonoBehaviour
{

    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private GameObject linePrefab;

    [SerializeField] private MeshRenderer quad;

    [SerializeField] private int numOfPoints;

    [SerializeField] private GameObject ant;

    [SerializeField] private Material visitedMat;

    private Vector3[] points;

    private float[,] distanceMatrix;

    private float[] desirability;

    private bool[] visitedIndexes;

    private GameObject linesParent;
    private GameObject pointsParent;

    private int antPositionIndex;

    private bool isMoving;

    private void Start()
    {
        isMoving = false;

        desirability = new float[numOfPoints];

        visitedIndexes = new bool[numOfPoints];

        points = new Vector3[numOfPoints];

        float xMin = quad.bounds.min.x + .5f;
        float xMax = quad.bounds.max.x - .5f;
        float yMin = quad.bounds.min.y + .5f;
        float yMax = quad.bounds.max.y - .5f;

        pointsParent = new GameObject("Points");

        distanceMatrix = new float[numOfPoints,numOfPoints];

        for (int i = 0; i < numOfPoints; i++)
        {
            float xPos = Random.Range(xMin, xMax);
            float yPos = Random.Range(yMin, yMax);

            Vector3 pos = new Vector3(xPos, yPos, -0.3f);

            Instantiate(pointPrefab, pos, Quaternion.identity, pointsParent.transform).name = "Point " + i;

            points[i] = pos;
        }

        for(int i = 0; i < numOfPoints; i++)
        {
            for(int j = 0; j < numOfPoints; j++)
            {

                float dst = Vector3.Distance(points[i], points[j]);

                distanceMatrix[i, j] = dst;

            }

        }

        linesParent = new GameObject("Lines");

        ant.transform.position = points[0];
        ant.transform.position = new Vector3(ant.transform.position.x, ant.transform.position.y, -4.5f);


        CalculateDesirability(0);
        DrawLines(0);
    }



    private void CalculateDesirability(int index)
    {
        antPositionIndex = index;

        visitedIndexes[index] = true;

        for (int i = 0; i < numOfPoints; i++)
        {
            float dst = distanceMatrix[index, i];

            if (visitedIndexes[i])
            {
                dst = 0;
            }

            desirability[i] = dst == 0 ? 0 : Mathf.Pow(1 / dst, 5f);
        }

    }

    private void DrawLines(int index)
    {
        pointsParent.transform.GetChild(index).gameObject.GetComponent<Renderer>().material = visitedMat;


        for (int i = 0; i < linesParent.transform.childCount; i++)
        {
            Destroy(linesParent.transform.GetChild(i).gameObject);

        }

        

        for (int i = 0; i < numOfPoints; i++)
        {
            



            GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity, linesParent.transform);
            line.name = "Line " + i;

            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

            lineRenderer.SetPosition(0, points[index]);
            lineRenderer.SetPosition(1, points[i]);


            


            lineRenderer.endWidth = .05f;
            lineRenderer.startWidth = .05f;

            float alphaValue = Mathf.Lerp(0, 1, desirability[i] / Mathf.Max(desirability));//desirability[i] * .08f;

            lineRenderer.startColor = new Color(1, 1, 1, alphaValue);
            lineRenderer.endColor = new Color(1, 1, 1, alphaValue);

        }

    }



    private void Update()
    {


        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isMoving)
            {
                return;
            }
            isMoving = true;

            float randomRange = 0;

            


            for (int i = 0; i < desirability.Length; i++)
            {
                randomRange += desirability[i];
            }

            float random = Random.Range(0, randomRange);

            for(int i = 0; i < desirability.Length; i++)
            {
                if(random < desirability[i])
                {
                    StartCoroutine(MoveAnt(i));
                    break;
                }

                random -= desirability[i];
            }



        }

    }

    private IEnumerator MoveAnt(int index)
    {

        float elapsedTime = 0;

        while(elapsedTime < .7f)
        {

            ant.transform.position = Vector3.Lerp(points[antPositionIndex], points[index], elapsedTime / .7f);

            ant.transform.position = new Vector3(ant.transform.position.x, ant.transform.position.y, -4.5f);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        ant.transform.position = points[index];
        ant.transform.position = new Vector3(ant.transform.position.x, ant.transform.position.y, -4.5f);


        antPositionIndex = index;

        CalculateDesirability(antPositionIndex);
        DrawLines(antPositionIndex);

        isMoving = false;
    }

}
