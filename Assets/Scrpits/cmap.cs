using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class cmap : MonoBehaviour
{

    public int width;
    public int height;

    [Range(0, 1)]
    public float fillness = 0.35f;
    [Range(0, 99999)]
    public float throughChance = 1350;
    [Range(0, 100)]
    public int doublechance = 70;

    public enum kind { old, k, kruskal, prim };
    public kind kindpos;

    public bool costumerSeed;

    public string seed;

    private List<GameObject> mapObjs;
    private System.Random pseudoRandom;
    public Material wallMate, roadMate;

    private int[,] poss;

    int[,] changes = {
        { 0, 1 },
        { 0, -1 },
        { -1, 0 },
        { 1, 0 }
    };

    int[,] map;


    void Start()
    {

        if (!costumerSeed)
            seed = System.DateTime.Now.ToString() + System.DateTime.Now.Millisecond.ToString();
        pseudoRandom = new System.Random(seed.GetHashCode());

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            switch ((int)kindpos)
            {
                case 0:
                    GenerateMap0();
                    break;
                case 1:
                    GenerateMap1();
                    break;
                case 2:
                    kruskal();
                    break;
                case 3:
                    prim();
                    break;
            }
        }
    }


    void GenerateMap0()
    {
        map = new int[width, height];

        DarwMap(pseudoRandom, 0, 0, height / 2);

        Debug.Log(seed);
    }

    //挖洞大法
    void DarwMap(System.Random rd, int depth, int posX, int posY)
    {
        if (!checkG(posX, posY) || map[posX, posY] == 1)
            return;

        map[posX, posY] = 1;
        bool[] checks = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            checks[i] = checkC(posX + changes[i, 0], posY + changes[i, 1]);
        }
        int[] c = new int[4];
        int ccount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (checks[i])
            {
                c[i] = rd.Next(0, 9999);
                ccount++;
            }
            else
                c[i] = -1;
        }

        if (ccount == 0)
            return;

        int maxpos = 0;
        for (int i = 0; i < 4; i++)
        {
            if (c[i] > c[maxpos])
                maxpos = i;
        }
        c[maxpos] = -1;

        DarwMap(rd, depth + 1, posX + changes[maxpos, 0], posY + changes[maxpos, 1]);

        if (ccount > 1)
        {
            int nextpos = 0;
            for (int i = 0; i < 4; i++)
            {
                if (c[i] > c[nextpos])
                    nextpos = i;
            }
            int xx = posX + changes[nextpos, 0];
            int yy = posY + changes[nextpos, 1];
            if (checkC(xx, yy) && rd.Next(0, 100) < doublechance)
                DarwMap(rd, depth + 1, posX + changes[nextpos, 0], posY + changes[nextpos, 1]);
        }

    }


    void GenerateMap1()
    {
        map = new int[width, height];

        splitMap(pseudoRandom, 0, width - 1, 0, height - 1);

    }

    //递归分割
    void splitMap(System.Random rb, int xmi, int xma, int ymi, int yma)
    {
        bool fx, fy;
        fx = xmi < (xma - 1);
        fy = ymi < (yma - 1);

        int x = 0, y = 0;

        if (!fx && !fy || xmi == xma || ymi == yma)
            return;

        if (fx)
        {
            x = rb.Next(xmi + 1, xma);
            for (int i = ymi; i <= yma; i++)
                map[x, i] = 1;

        }

        if (fy)
        {
            y = rb.Next(ymi + 1, yma);
            for (int i = xmi; i <= xma; i++)
                map[i, y] = 1;
        }



        if (fx && fy)
        {
            splitMap(rb, xmi, x - 1, ymi, y - 1);
            splitMap(rb, x + 1, xma, ymi, y - 1);
            splitMap(rb, xmi, x - 1, y + 1, yma);
            splitMap(rb, x + 1, xma, y + 1, yma);



        }
        else if (fx)
        {
            splitMap(rb, xmi, x - 1, ymi, yma);
            splitMap(rb, x + 1, xma, ymi, yma);
            map[x, rb.Next(ymi, yma + 1)] = 0;

        }
        else if (fy)
        {
            splitMap(rb, xmi, xma, ymi, y - 1);
            splitMap(rb, xmi, xma, y + 1, yma);
            map[rb.Next(xmi, xma + 1), y] = 0;
        }

        if (fx && fy)
        {
            int[] digPoint ={
                 rb.Next(xmi, x) ,
                 rb.Next(x + 1, xma + 1) ,
                 rb.Next(ymi, y) ,
                 rb.Next(y + 1, yma + 1)
            };
            int dp = rb.Next(4);

            for (int i = 0; i < 4; i++)
            {
                if (i == dp)
                    continue;
                if (i < 2)
                {
                    if (canDarw(digPoint[i], y) < 3)
                        map[digPoint[i], y] = 0;
                    else if (canDarw(digPoint[i] + 1, y) < 3)
                        map[digPoint[i] + 1, y] = 0;
                    else if (canDarw(digPoint[i] - 1, y) < 3)
                        map[digPoint[i] - 1, y] = 0;
                }
                else
                {
                    if (canDarw(x, digPoint[i]) < 3)
                        map[x, digPoint[i]] = 0;
                    else if (canDarw(x, digPoint[i] + 1) < 3)
                        map[x, digPoint[i] + 1] = 0;
                    else if (canDarw(x, digPoint[i] - 1) < 3)
                        map[x, digPoint[i] - 1] = 0;
                }


            }
        }


        return;
    }


    public class edge
    {

        public int[] a;
        public int[] b;

        public edge(int x1, int y1, int x2, int y2)
        {
            a = new int[] { x1, y1 };
            b = new int[] { x2, y2 };
        }
    }

    //并查集类，一维数组实现
    public class UFset
    {
        int[] parents;

        public UFset(int size)
        {
            parents = new int[size];

            for (int i = 0; i < parents.Length; i++)
                parents[i] = i;
        }

        public bool union(int a, int b)
        {
            if (root(a) == root(b))
                return false;
            else
            {
                parents[root(b)] = root(a);
                return true;
            }

        }

        int root(int a)
        {
            if (parents[a] == a)
                return a;
            else
                return parents[a] = root(parents[a]);
        }
    }

    void kruskal()
    {
        //int[,] node = new int[width, height];
        UFset set = new UFset(width * height);

        List<edge> edges = new List<edge>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x != width - 1)
                    edges.Add(new edge(x, y, x + 1, y));
                if (y != height - 1)
                    edges.Add(new edge(x, y, x, y + 1));
            }
        }

        map = new int[width * 2 - 1, height * 2 - 1];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                map[x * 2, y * 2] = 1;
            }


        while (edges.Count > 0)
        {
            int iter = pseudoRandom.Next(edges.Count);
            edge value = edges[iter];
            int ax = value.a[0],
                ay = value.a[1],
                bx = value.b[0],
                by = value.b[1];
            //a = node[ax, ay],
            //b = node[bx, by];

            if (set.union(ax * height + ay, bx * height + by))
                map[ax + bx, ay + by] = 1;

            edges.RemoveAt(iter);
        }

        width = width * 2 - 1;
        height = height * 2 - 1;

    }

    void prim()
    {
        int[,] node = new int[width, height];
        List<edge> edges = new List<edge>();

        map = new int[width * 2 - 1, height * 2 - 1];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x * 2, y * 2] = 1;


        int count = 1;
        int xx = pseudoRandom.Next(width),
            yy = pseudoRandom.Next(height);
        node[xx, yy] = 1;

        while (count < width * height)
        {
            pushEdge(edges, xx, yy);

            int pos;
            edge e;
            do
            {
                pos = pseudoRandom.Next(edges.Count);
                e = edges[pos];
                edges.RemoveAt(pos);
            } while (node[e.a[0], e.a[1]] == node[e.b[0], e.b[1]]);

            map[e.a[0] + e.b[0], e.a[1] + e.b[1]] = 1;
            if (node[e.a[0], e.a[1]] == 0)
            {
                xx = e.a[0];
                yy = e.a[1];
            }
            else
            {
                xx = e.b[0];
                yy = e.b[1];
            }
            node[xx, yy] = 1;
            count++;
        }


        width = width * 2 - 1;
        height = height * 2 - 1;
    }

    void pushEdge(List<edge> edges, int x, int y)
    {
        if (x > 0 && map[2 * x - 1, 2 * y] == 0)
            edges.Add(new edge(x, y, x - 1, y));
        if (x < width - 1 && map[2 * x + 1, 2 * y] == 0)
            edges.Add(new edge(x, y, x + 1, y));
        if (y > 0 && map[2 * x, 2 * y - 1] == 0)
            edges.Add(new edge(x, y, x, y - 1));
        if (y < height - 1 && map[2 * x, 2 * y + 1] == 0)
            edges.Add(new edge(x, y, x, y + 1));
    }


    bool checkC(int x, int y)
    {
        return checkG(x, y) && canDarw(x, y) < 2;
    }


    ///目标墙可以挖与否
    bool checkG(int x, int y)
    {
        if (x < 0 || x >= width)
            return false;
        if (y < 0 || y >= height)
            return false;
        if (map[x, y] == 1)
            return false;

        return true;
    }


    //目标方块上下左右情况
    int canDarw(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int i = 0; i < 4; i++)
        {
            int x = gridX + changes[i, 0];
            int y = gridY + changes[i, 1];
            if (!(x < 0 || x >= width || y < 0 || y >= height) && map[x, y] == 1)
                wallCount++;
        }

        return wallCount;
    }


    void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] == 0) ? Color.black : Color.white;

                    Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }


}