using Unity.VisualScripting;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.5f;
    [SerializeField] private float yOffset = 0.005f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    // Logic
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;

    private void Awake()
    {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }
    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover")))
        {
            // Get the indexes of the tile I've hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            // If we're hovering a tile after not hovering any tiles
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we were already hovering a tile, change the previous one
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we press on the mouse
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    // Is it your turn?
                    if (true)
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                    }
                }
            }

            // If we are releasing the mouse button
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    currentlyDragging = null;
                }
                else
                {
                    currentlyDragging = null;
                }
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
            }
        }
    }

    // Generate the board
    private void GenerateAllTiles (float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountY / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0, blackTeam = 1;

        // White team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);

        // Black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return cp;
    }

    // Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    PositionSinglePiece(x, y, true);
    }
    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    // Operations
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        // Is there another piece on the target position?
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];

            if (cp.team == ocp.team)
                return false;
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        return true;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x,y] == hitInfo)
                    return new Vector2Int(x,y);

        return -Vector2Int.one; // Invalid
    }
}
