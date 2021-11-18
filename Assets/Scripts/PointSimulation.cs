using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSimulation : MonoBehaviour
{

    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private GameObject linePrefab;

    [SerializeField] private MeshRenderer quad;

    [SerializeField] private int numOfPoints;

    private Vector3[] points;

    private float[,] distanceMatrix;

    private void Start()
    {

        points = new Vector3[numOfPoints];

        float xMin = quad.bounds.min.x + .5f;
        float xMax = quad.bounds.max.x - .5f;
        float yMin = quad.bounds.min.y + .5f;
        float yMax = quad.bounds.max.y - .5f;

        GameObject pointsParent = new GameObject("Points");

        distanceMatrix = new float[numOfPoints,numOfPoints];

        for (int i = 0; i < numOfPoints; i++)
        {
            float xPos = Random.Range(xMin, xMax);
            float yPos = Random.Range(yMin, yMax);

            Vector3 pos = new Vector3(xPos, yPos, -0.3f);

            Instantiate(pointPrefab, pos, Quaternion.identity, pointsParent.transform);

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

        GameObject lineParent = new GameObject("Lines");
        for(int i = 1; i < numOfPoints; i++)
        {
            GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity, lineParent.transform);            

            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
           
            lineRenderer.SetPosition(0, points[0]);
            lineRenderer.SetPosition(1, points[i]);


            float dst = distanceMatrix[0, i];
            float desirability = Mathf.Pow(1 / dst, 1.5f);

            float lineWidth = desirability * .08f;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startWidth = lineWidth;
        }

    }

}
