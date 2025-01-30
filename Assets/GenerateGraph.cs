using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class GenerateGraph : MonoBehaviour
{
    public int nodesNumber;
    public bool oriented;

    public GameObject selected;
    public Button generateGraphButton;
    public InputField nodesNumberField;
    public Toggle orientedToggle;
    public Toggle neorientedToggle;
    public GameObject nodePrefab;
    public GameObject edgePrefab;
    public GameObject neorientedEdgePrefab;
    public float spawnDelay = 1f;

    int firstNode, secondNode;
    Vector3 firstNodeLocation, secondNodeLocation;

    public int[,] adiacentMatrix;
    public Dictionary<int, List<int>> adiacentList = new Dictionary<int, List<int>>();
    public Dictionary<int, List<int>> transpusaAdiacentList = new Dictionary<int, List<int>>();
    public Button optionsButtonPrefab;
    public Color showColor;
    public Color normalColor;

    public Sprite showColorArrow;
    public Sprite normalColorArrow;

    public enum buttonNames
    {
        Generica, BFS, DFS, Sortare, Conexe, TareConexe, APM, Prim, Kruscal, DrumMin
    }

    public void preparation()
    {
        string input = nodesNumberField.text;
        if (!int.TryParse(input, out nodesNumber))
        {
            Debug.Log("Introdu numarul de noduri");
            return;
        }

        if (orientedToggle.isOn)
            oriented = true;
        else if (neorientedToggle.isOn)
            oriented = false;
        else
        {
            Debug.Log("Selecteaza tipul grafului");
            return;
        }

        adiacentMatrix = new int[nodesNumber, nodesNumber];

        RectTransform rt = selected.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(10000, 0);
        RectTransform rt2 = generateGraphButton.GetComponent<RectTransform>();
        rt2.anchoredPosition = new Vector2(10000, 0);

        StartCoroutine(ArrangeCirclesInCircleWithAnimation());
        StartCoroutine(GenerateOptionButton());
    }

    IEnumerator ArrangeCirclesInCircleWithAnimation()
    {
        float angleStep = 360f / nodesNumber;
        float radius = 300f;
        Vector2 centerPosition = new Vector2(1000, 500);
        

        for (int i = 0; i < nodesNumber; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);

            float x = Mathf.Cos(angle) * radius + centerPosition.x;
            float y = Mathf.Sin(angle) * radius + centerPosition.y;

            GameObject node = Instantiate(nodePrefab, transform);
            node.name = $"Node{i + 1}";
            RectTransform rt = node.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x, y);
            node.GetComponentInChildren<Text>().text = (i + 1).ToString();

            Button buttonComponent = node.GetComponent<Button>();
            int capturedI = i + 1;
            buttonComponent.onClick.AddListener(() => OnNodeClick(capturedI));

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void DrawArrow(Vector2 start, Vector2 end, int i, int j, GameObject prefab, float distanceValue)
    {
            GameObject arrow = Instantiate(prefab, transform);
            arrow.name = $"Edge{i}{j}";

            Vector2 midPoint = (start + end) / 2f;
            arrow.GetComponent<RectTransform>().anchoredPosition = midPoint;

            Vector2 direction = end - start;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, angle);

            float distance = direction.magnitude - distanceValue;
            RectTransform arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.sizeDelta = new Vector2(distance, arrowRect.sizeDelta.y);

            Button buttonComponent = arrow.GetComponent<Button>();
            int capturedI = i;
            int capturedJ = j;
            buttonComponent.onClick.AddListener(() => OnArrowClick(capturedI, capturedJ));
    }

    private void OnNodeClick(int i)
    {
        Debug.Log($"Node clicked at [{i}]");

        if (firstNode == 0)
        {
            firstNode = i;
            string buttonName = "Node" + i.ToString();
            GameObject obiect = GameObject.Find(buttonName);
            firstNodeLocation = obiect.transform.position;
        }
        else
        {
            secondNode = i;
            if (secondNode == firstNode || adiacentMatrix[firstNode - 1, secondNode - 1] == 1)
                return;
            string buttonName = "Node" + i.ToString();
            GameObject obiect = GameObject.Find(buttonName);
            secondNodeLocation = obiect.transform.position;

            Debug.Log($"Modificam elementul {firstNode}{secondNode} cu 1");
            adiacentMatrix[firstNode - 1, secondNode - 1] = 1;
            
            if (!oriented)
            {
                adiacentMatrix[secondNode - 1, firstNode - 1] = 1;
                DrawArrow(firstNodeLocation, secondNodeLocation, firstNode - 1, secondNode - 1, neorientedEdgePrefab, 100f);
            }
            else
                DrawArrow(firstNodeLocation, secondNodeLocation, firstNode, secondNode, edgePrefab, 50f);

            firstNode = 0;
            secondNode = 0;
        }
    }

    private void OnArrowClick(int i, int j)
    {
        Debug.Log($"Edge clicked at [{i}{j}]");
        string arrowName = "Edge" + i.ToString() + j.ToString();
        GameObject arrow = GameObject.Find(arrowName);
        Destroy(arrow);
        if (oriented)
            adiacentMatrix[i - 1, j - 1] = 0;
        else
        {
            adiacentMatrix[i - 1, j - 1] = 0;
            adiacentMatrix[j - 1, i - 1] = 0;
        }
    }

    IEnumerator GenerateOptionButton()
    {
        int posX = 200, posY = 800;
        for(int i = 0; i < 10; ++i)
        {
            Button newButton = Instantiate(optionsButtonPrefab, transform);
            newButton.name = $"Button{((buttonNames)i)}";
            newButton.GetComponentInChildren<Text>().text = ((buttonNames)i).ToString();
            Button buttonComponent = newButton.GetComponent<Button>();
            int capturedI = i;
            buttonComponent.onClick.AddListener(() => OnButtonClick(capturedI));
            if (i == 5)
            {
                posX = 1720;
                posY = 800 + i * 150;
            }
            RectTransform rt = newButton.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(posX, posY - i * 150);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void PrintMatrixToConsole(int[,] matrix)
    {
        string matrixString = "";

        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            for (int col = 0; col < matrix.GetLength(1); col++)
            {
                matrixString += matrix[row, col] + "\t";
            }
            matrixString += "\n";
        }

        Debug.Log("Matricea este:\n" + matrixString);
    }

    private void OnButtonClick(int i)
    {
        fromMatrixToList();
        Debug.Log($"Button clicked at [{i}]");
        int startNode = 0;
        Debug.Log($"Parcurgerea incepe de la nodul{startNode}");
        switch (i)
        {
            case 0:
                StartCoroutine(generica(startNode));
                break;
            case 1:

                StartCoroutine(BFS(startNode));
                break;
            case 2:
                bool[] visited = new bool[nodesNumber];
                StartCoroutine(DFS(startNode, visited));
                break;
            case 3:
                sortareTopologica();
                break;
            case 4:
                conex();
                break;
            case 5:
                for (int j = 0; j < nodesNumber; ++j)
                    adiacentList[j] = new List<int>();
                tareConex();
                break;
            case 6:
                APM();
                break;
            case 7:
                prim();
                break;
            case 8:
                kruscal();
                break;
            case 9:
                drumMinim();
                break;
        }

    }

    private void fromMatrixToList()
    {
        for (int i = 0; i < nodesNumber; ++i)
            adiacentList[i] = new List<int>();
        for (int i = 0; i < nodesNumber; ++i)
            for (int j = 0; j < nodesNumber; ++j)
                if(adiacentMatrix[i, j] != 0)
                    addEdge(i, j);
    }

    private void addEdge(int nodeA, int nodeB)
    {
        adiacentList[nodeA].Add(nodeB);
        if (!oriented)
            adiacentList[nodeB].Add(nodeA);
        
    }

    private void colorNode(int currentNode)
    {
        string nodeName = $"Node{currentNode}";
        GameObject nodeButton = GameObject.Find(nodeName);
        Image nodeImage = nodeButton.GetComponent<Image>();
        nodeImage.color = showColor;
    }

    private void colorEdge(int currentNode, int neighbor)
    {
        string edgeName = $"Edge{currentNode}{neighbor}";
        GameObject edgeButton = GameObject.Find(edgeName);
        Image edgeImage = edgeButton.GetComponent<Image>();
        edgeImage.sprite = showColorArrow;
    }

    IEnumerator generica(int startNode)
    {
        Debug.Log("functie generica");

        HashSet<int> W = new HashSet<int>();
        List<int> V = new List<int>();
        HashSet<int> U = new HashSet<int>();

        foreach (var node in adiacentList.Keys)
        {
            U.Add(node);
        }

        V.Add(startNode);
        U.Remove(startNode);
        int k = 1;

        Dictionary<int, int> o = new Dictionary<int, int>();
        foreach (var entry in adiacentList)
        {
            o[entry.Key] = 0;
        }
        o[startNode] = k;

        while (W.Count < adiacentList.Count)
        {
            while (V.Count > 0)
            {
                int currentNode = V[V.Count - 1];
                V.RemoveAt(V.Count - 1);
                Debug.Log($"Procesăm nodul {currentNode + 1}");

                colorNode(currentNode + 1);
                yield return new WaitForSeconds(spawnDelay);

                foreach (int neighbor in adiacentList[currentNode])
                {
                    if (U.Contains(neighbor))
                    {
                        V.Add(neighbor);
                        U.Remove(neighbor);

                        colorEdge(currentNode + 1, neighbor + 1);
                        yield return new WaitForSeconds(spawnDelay);

                        o[neighbor] = k;
                        Debug.Log($"Adăugat nodul {neighbor + 1} în procesare");
                        Debug.Log($"Muchie procesată: ({currentNode + 1}, {neighbor + 1})");
                    }
                }
                W.Add(currentNode);
            }
            if (U.Count > 0)
            {
                int newSource = U.First();
                V.Add(newSource);
                U.Remove(newSource);
                o[newSource] = k;
                Debug.Log($"Pas {k}: Nod sursă {newSource + 1}");
                k++;
            }

        }
        Debug.Log("Parcurgere terminata");
    }

    IEnumerator BFS(int startNode)
    {
        bool[] visited = new bool[nodesNumber];
        Queue<int> queue = new Queue<int>();

        queue.Enqueue(startNode);
        visited[startNode] = true;
        Debug.Log($"Nod vizitat: {startNode + 1}");

        colorNode(startNode + 1);
        yield return new WaitForSeconds(spawnDelay);

        while (queue.Count > 0)
        {
            int currentNode = queue.Dequeue();
            Debug.Log($"Procesăm nodul: {currentNode + 1}");

            foreach (int neighbor in adiacentList[currentNode])
            {
                if (!visited[neighbor])
                {
                    Debug.Log($"Muchie vizitată: {currentNode + 1}{neighbor + 1}");
                    colorEdge(currentNode + 1, neighbor + 1);
                    yield return new WaitForSeconds(spawnDelay);

                    queue.Enqueue(neighbor);
                    visited[neighbor] = true;

                    colorNode(neighbor + 1);
                    yield return new WaitForSeconds(spawnDelay);
                }
            }
        }

        Debug.Log("Parcurgere BFS finalizată!");
    }

    IEnumerator DFS(int currentNode, bool[]visited)
    {
        visited[currentNode] = true;
        Debug.Log($"Nod vizitat: {currentNode + 1}");

        colorNode(currentNode + 1);
        yield return new WaitForSeconds(spawnDelay);

        foreach (int neighbor in adiacentList[currentNode])
        {
            if (!visited[neighbor])
            {
                Debug.Log($"Muchie vizitata: {currentNode + 1}{neighbor + 1}");

                colorEdge(currentNode + 1, neighbor + 1);
                yield return new WaitForSeconds(spawnDelay);

                yield return StartCoroutine(DFS(neighbor, visited));
            }
        }

    }

    private void sortareTopologica()
    {
        List<int> topOrder = new List<int>();
        Queue<int> q = new Queue<int>();

        int[] inGrade = new int[nodesNumber];
        foreach (var node in adiacentList)
            foreach (var neighboor in node.Value)
                inGrade[neighboor]++;

        for (int i = 0; i < nodesNumber; ++i)
            if (inGrade[i] == 0)
                q.Enqueue(i);

        while (q.Count > 0)
        {
            int u = q.Dequeue();
            topOrder.Add(u);

   
            foreach (int v in adiacentList[u])
            {
                inGrade[v]--;
                if (inGrade[v] == 0)
                    q.Enqueue(v);
            }
        }

        if (topOrder.Count != nodesNumber)
        {
            Debug.Log("Graful conține un ciclu.");
            return;
        }
            
        string topologicalSort = "Sortarea este: ";
        foreach (var node in topOrder)
            topologicalSort += node + " ";
        Debug.Log(topologicalSort);

    }

    private void conexDFS(int currentNode, bool[] visited, List<int> component)
    {
        visited[currentNode] = true;
        component.Add(currentNode);
        Debug.Log($"Nod vizitat: {currentNode + 1}");
        foreach (int neighbor in adiacentList[currentNode])
        {
            if (!visited[neighbor])
            {
                Debug.Log($"Muchie vizitată: {currentNode + 1} - {neighbor + 1}");
                conexDFS(neighbor, visited, component);
            }
        }
    }

    private void conex()
    {
        bool[] visited = new bool[nodesNumber];
        List<List<int>> connectedComponents = new List<List<int>>();
        for (int i = 0; i < nodesNumber; i++)
        {
            if (!visited[i])
            {
                List<int> component = new List<int>();
                Debug.Log($"Explorăm componenta conexă începând cu nodul {i + 1}");
                conexDFS(i, visited, component);
                connectedComponents.Add(component);

                Debug.Log($"Componentă conexă găsită: {string.Join(", ", component)}");
            }
        }
        Debug.Log("Componente conexe finale:");
        for (int i = 0; i < connectedComponents.Count; i++)
        {
            Debug.Log($"Componenta {i + 1}: {string.Join(", ", connectedComponents[i])}");
        }
    }

    private void tareConexDFS(int currentNode, bool[] visited, Stack<int> stack)
    {
        visited[currentNode] = true;
        foreach (int neighbor in adiacentList[currentNode])
        {
            if (!visited[neighbor])
            {
                tareConexDFS(neighbor, visited, stack);
            }
        }
        stack.Push(currentNode);
    }

    private void transpuneLista()
    {
        for (int i = 0; i < nodesNumber; i++)
        {
            foreach (int neighbor in adiacentList[i])
            {
                transpusaAdiacentList[neighbor].Add(i);
            }
        }
    }

    private void transpusaDFS(int currentNode, bool[] visited, List<int> component)
    {
        visited[currentNode] = true;
        component.Add(currentNode);
        foreach (int neighbor in transpusaAdiacentList[currentNode])
        {
            if (!visited[neighbor])
            {
                transpusaDFS(neighbor, visited, component);
            }
        }
    }

    private void tareConex()
    {
        Stack<int> stack = new Stack<int>();
        bool[] visited = new bool[nodesNumber];

        for (int i = 0; i < nodesNumber; i++)
        {
            if (!visited[i])
            {
                tareConexDFS(i, visited, stack);
            }
        }

        transpuneLista();

        Array.Fill(visited, false);
        List<List<int>> elementeTareConexe = new List<List<int>>();

        while (stack.Count > 0)
        {
            int node = stack.Pop();
            if (!visited[node])
            {
                List<int> component = new List<int>();
                transpusaDFS(node, visited, component);
                elementeTareConexe.Add(component);

                Debug.Log($"Componentă tare conexă găsită: {string.Join(", ", component)}");
            }
        }

        Debug.Log("Componentele tare conexe finale:");
        for (int i = 0; i < elementeTareConexe.Count; i++)
        {
            Debug.Log($"Componenta {i + 1}: {string.Join(", ", elementeTareConexe[i])}");
        }
    }

    private void APM()
    {

    }

    private void prim()
    {

    }

    private void kruscal()
    {

    }

    private void drumMinim()
    {

    }


}
