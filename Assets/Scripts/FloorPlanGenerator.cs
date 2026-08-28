using System;
using System.Collections.Generic;
using UnityEngine;


public class FloorPlanGenerator : MonoBehaviour
{


    [Header("JSON")]
    public TextAsset jsonFile;


 

    [Header("Scale")]

    [Tooltip("How many source-image pixels equal 1 Unity meter.")]
    public float pixelsPerMeter = 100f;

    [Tooltip(
        "If true, position.x/position.z is interpreted as the " +
        "TOP-LEFT corner of the detection."
    )]
    public bool positionIsTopLeft = false;



    [Header("Walls")]

    [Tooltip("Height of the walls.")]
    public float wallHeight = 3f;

    [Tooltip(
        "Minimum wall thickness used when the detected wall " +
        "thickness is extremely small."
    )]
    public float minimumWallThickness = 0.10f;


  

    [Header("Doors")]

    public float doorHeight = 2.1f;

    public float doorThickness = 0.08f;

    public float doorFrameThickness = 0.08f;

    public float doorFrameDepth = 0.12f;


 
 

    [Header("Windows")]

    public float windowHeight = 1.2f;

    [Tooltip("Height of the bottom of the window from the floor.")]
    public float windowSillHeight = 1.0f;

    public float windowFrameThickness = 0.07f;

    public float windowFrameDepth = 0.12f;

    [Tooltip(
        "Small offset that moves the window toward the visible " +
        "surface of the wall."
    )]
    public float windowSurfaceOffset = 0.015f;


   

    [Header("Wall Openings")]

    [Tooltip(
        "Extra space around a door/window opening."
    )]
    public float openingPadding = 0.02f;

    [Tooltip(
        "Maximum distance in meters for a door/window to be " +
        "considered part of a wall."
    )]
    public float maximumWallAssociationDistance = 0.75f;




    [Header("Floor")]

    public bool generateFloor = true;

    public float floorMargin = 1f;

    public float floorThickness = 0.08f;




    [Header("AI Confidence")]

    [Range(0f, 1f)]
    public float minimumConfidence = 0.5f;

    public float minimumElementSize = 0.05f;



    [Header("Generation")]

    public bool generateOnStart = false;

    public bool clearBeforeGenerating = true;




    [Header("Materials")]

    public Material wallMaterial;

    public Material doorMaterial;

    public Material doorFrameMaterial;

    public Material windowFrameMaterial;

    public Material windowGlassMaterial;

    public Material floorMaterial;



    private Transform root;

    private Transform floorParent;

    private Transform wallsParent;

    private Transform doorsParent;

    private Transform windowsParent;



    private FloorPlanData floorPlan;




    private class Detection
    {
        public Element element;

        public string type;

        public Vector3 center;

        public float width;

        public float length;
    }


    private class WallInfo
    {
        public Detection detection;

        public bool horizontal;

        public float longLength;

        public float thickness;

        public float centerLong;

        public float centerShort;
    }




    private void Start()
    {
        if (generateOnStart)
        {
            GenerateFloorPlan();
        }
    }


  

    [ContextMenu("Generate Floor Plan")]
    public void GenerateFloorPlan()
    {
        if (jsonFile == null)
        {
            Debug.LogError(
                "[FloorPlanGenerator] No JSON file assigned.",
                this
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(jsonFile.text))
        {
            Debug.LogError(
                $"[FloorPlanGenerator] '{jsonFile.name}' is empty.",
                this
            );

            return;
        }

        GenerateFromJsonText(
            jsonFile.text,
            jsonFile.name
        );
    }


 

    public FloorPlanGenerationResult GenerateFromJsonText(
        string jsonText,
        string sourceLabel = "JSON"
    )
    {
       

        if (string.IsNullOrWhiteSpace(jsonText))
        {
            string message =
                $"'{sourceLabel}' is empty.";

            Debug.LogError(
                $"[FloorPlanGenerator] {message}",
                this
            );

            return FloorPlanGenerationResult.Failed(
                message
            );
        }


        if (pixelsPerMeter <= 0f)
        {
            string message =
                "Pixels Per Meter must be greater than 0.";

            Debug.LogError(
                $"[FloorPlanGenerator] {message}",
                this
            );

            return FloorPlanGenerationResult.Failed(
                message
            );
        }


      

        try
        {
            floorPlan =
                JsonUtility.FromJson<FloorPlanData>(
                    jsonText
                );
        }
        catch (Exception e)
        {
            string message =
                $"Failed to parse '{sourceLabel}': {e.Message}";

            Debug.LogError(
                $"[FloorPlanGenerator] {message}",
                this
            );

            return FloorPlanGenerationResult.Failed(
                message
            );
        }



        if (
            floorPlan == null ||
            floorPlan.elements == null ||
            floorPlan.elements.Length == 0
        )
        {
            string message =
                $"'{sourceLabel}' contains no valid elements.";

            Debug.LogError(
                $"[FloorPlanGenerator] {message}",
                this
            );

            return FloorPlanGenerationResult.Failed(
                message
            );
        }



        if (clearBeforeGenerating)
        {
            ClearGenerated();
        }


        EnsureHierarchy();



        List<Detection> walls =
            new List<Detection>();

        List<Detection> doors =
            new List<Detection>();

        List<Detection> windows =
            new List<Detection>();


        float minX = float.MaxValue;
        float maxX = float.MinValue;

        float minZ = float.MaxValue;
        float maxZ = float.MinValue;


        int skippedLowConfidence = 0;

        int skippedTooSmall = 0;

        int skippedUnknown = 0;


        foreach (Element element in floorPlan.elements)
        {
            if (element == null)
                continue;


            if (element.confidence < minimumConfidence)
            {
                skippedLowConfidence++;
                continue;
            }


            if (
                element.position == null ||
                element.size == null
            )
            {
                skippedUnknown++;
                continue;
            }


            float width =
                element.size.width /
                pixelsPerMeter;


            float length =
                element.size.length /
                pixelsPerMeter;


            if (
                width <= 0f ||
                length <= 0f ||
                Mathf.Min(
                    width,
                    length
                ) < minimumElementSize
            )
            {
                skippedTooSmall++;
                continue;
            }


            string type =
                (element.type ?? string.Empty)
                .Trim()
                .ToLowerInvariant();


            Vector3 center =
                ConvertPosition(
                    element.position.x,
                    element.position.z,
                    width,
                    length
                );


            Detection detection =
                new Detection
                {
                    element = element,
                    type = type,
                    center = center,
                    width = width,
                    length = length
                };


            switch (type)
            {
                case "wall":
                    walls.Add(detection);
                    break;

                case "door":
                    doors.Add(detection);
                    break;

                case "window":
                    windows.Add(detection);
                    break;

                default:
                    skippedUnknown++;
                    break;
            }



            float halfWidth =
                width / 2f;

            float halfLength =
                length / 2f;


            minX =
                Mathf.Min(
                    minX,
                    center.x - halfWidth
                );

            maxX =
                Mathf.Max(
                    maxX,
                    center.x + halfWidth
                );

            minZ =
                Mathf.Min(
                    minZ,
                    center.z - halfLength
                );

            maxZ =
                Mathf.Max(
                    maxZ,
                    center.z + halfLength
                );
        }



        int wallCount = 0;

        foreach (Detection wall in walls)
        {
            CreateArchitecturalWall(
                wall,
                doors,
                windows,
                wallCount
            );

            wallCount++;
        }



        int doorCount = 0;

        foreach (Detection door in doors)
        {
            WallInfo nearestWall =
                FindNearestWall(
                    door,
                    walls
                );


            CreateArchitecturalDoor(
                door,
                nearestWall,
                doorCount
            );

            doorCount++;
        }



        int windowCount = 0;

        foreach (Detection window in windows)
        {
            WallInfo nearestWall =
                FindNearestWall(
                    window,
                    walls
                );


            CreateArchitecturalWindow(
                window,
                nearestWall,
                windowCount
            );

            windowCount++;
        }



        if (
            generateFloor &&
            minX != float.MaxValue
        )
        {
            CreateFloor(
                minX,
                maxX,
                minZ,
                maxZ
            );
        }



        string successMessage =
            $"Generated {wallCount} walls, " +
            $"{doorCount} doors, " +
            $"{windowCount} windows.";


        Debug.Log(
            $"[FloorPlanGenerator] '{sourceLabel}': " +
            $"{successMessage} " +
            $"Skipped {skippedLowConfidence} low-confidence, " +
            $"{skippedTooSmall} too-small, " +
            $"{skippedUnknown} unknown/invalid.",
            this
        );


        return FloorPlanGenerationResult.Success(
            successMessage,
            wallCount,
            doorCount,
            windowCount
        );
    }




    private void CreateArchitecturalWall(
        Detection wall,
        List<Detection> doors,
        List<Detection> windows,
        int wallIndex
    )
    {
        bool horizontal =
            wall.width >= wall.length;


        float longLength =
            horizontal
                ? wall.width
                : wall.length;


        float thickness =
            horizontal
                ? wall.length
                : wall.width;


        thickness =
            Mathf.Max(
                thickness,
                minimumWallThickness
            );


        float start =
            -longLength / 2f;


        float end =
            longLength / 2f;


 

        List<float[]> openings =
            new List<float[]>();


        foreach (Detection door in doors)
        {
            if (
                IsDetectionOnWall(
                    door,
                    wall,
                    horizontal
                )
            )
            {
                float openingCenter =
                    GetLongCoordinate(
                        door,
                        wall,
                        horizontal
                    );


                float openingWidth =
                    GetOpeningWidth(
                        door,
                        horizontal
                    );


                openings.Add(
                    new float[]
                    {
                        openingCenter -
                        openingWidth / 2f -
                        openingPadding,

                        openingCenter +
                        openingWidth / 2f +
                        openingPadding
                    }
                );
            }
        }


        foreach (Detection window in windows)
        {
            if (
                IsDetectionOnWall(
                    window,
                    wall,
                    horizontal
                )
            )
            {
                float openingCenter =
                    GetLongCoordinate(
                        window,
                        wall,
                        horizontal
                    );


                float openingWidth =
                    GetOpeningWidth(
                        window,
                        horizontal
                    );


                openings.Add(
                    new float[]
                    {
                        openingCenter -
                        openingWidth / 2f -
                        openingPadding,

                        openingCenter +
                        openingWidth / 2f +
                        openingPadding
                    }
                );
            }
        }



        if (openings.Count == 0)
        {
            CreateWallSegment(
                "Wall_" + wallIndex,
                wall.center,
                horizontal,
                longLength,
                thickness,
                wallHeight
            );

            return;
        }



        openings.Sort(
            (a, b) =>
                a[0].CompareTo(b[0])
        );


    

        List<float[]> merged =
            new List<float[]>();


        foreach (float[] opening in openings)
        {
            if (merged.Count == 0)
            {
                merged.Add(
                    new float[]
                    {
                        opening[0],
                        opening[1]
                    }
                );

                continue;
            }


            float[] previous =
                merged[
                    merged.Count - 1
                ];


            if (opening[0] <= previous[1])
            {
                previous[1] =
                    Mathf.Max(
                        previous[1],
                        opening[1]
                    );
            }
            else
            {
                merged.Add(
                    new float[]
                    {
                        opening[0],
                        opening[1]
                    }
                );
            }
        }



        float current =
            start;


        int segmentIndex = 0;


        foreach (float[] opening in merged)
        {
            float openingStart =
                Mathf.Clamp(
                    opening[0],
                    start,
                    end
                );


            float openingEnd =
                Mathf.Clamp(
                    opening[1],
                    start,
                    end
                );


           
            if (
                openingStart >
                current
            )
            {
                float segmentLength =
                    openingStart -
                    current;


                CreateWallSegment(
                    "Wall_" +
                    wallIndex +
                    "_Segment_" +
                    segmentIndex,

                    GetWallSegmentCenter(
                        wall,
                        horizontal,
                        (current + openingStart) / 2f
                    ),

                    horizontal,

                    segmentLength,

                    thickness,

                    wallHeight
                );


                segmentIndex++;
            }


            current =
                Mathf.Max(
                    current,
                    openingEnd
                );
        }


        if (current < end)
        {
            float segmentLength =
                end - current;


            CreateWallSegment(
                "Wall_" +
                wallIndex +
                "_Segment_" +
                segmentIndex,

                GetWallSegmentCenter(
                    wall,
                    horizontal,
                    (current + end) / 2f
                ),

                horizontal,

                segmentLength,

                thickness,

                wallHeight
            );
        }
    }



    private void CreateWallSegment(
        string objectName,
        Vector3 center,
        bool horizontal,
        float longLength,
        float thickness,
        float height
    )
    {
        GameObject wall =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );


        wall.name =
            objectName;


        wall.transform.SetParent(
            wallsParent,
            true
        );


        wall.transform.position =
            new Vector3(
                center.x,
                height / 2f,
                center.z
            );


        if (horizontal)
        {
            wall.transform.localScale =
                new Vector3(
                    Mathf.Max(
                        longLength,
                        0.01f
                    ),

                    height,

                    Mathf.Max(
                        thickness,
                        0.01f
                    )
                );
        }
        else
        {
            wall.transform.localScale =
                new Vector3(
                    Mathf.Max(
                        thickness,
                        0.01f
                    ),

                    height,

                    Mathf.Max(
                        longLength,
                        0.01f
                    )
                );
        }


        ApplyMaterial(
            wall,
            wallMaterial,
            new Color(
                0.55f,
                0.55f,
                0.55f
            )
        );
    }



    private void CreateArchitecturalDoor(
        Detection door,
        WallInfo wall,
        int index
    )
    {
        Transform parent =
            new GameObject(
                "Door_" + index
            ).transform;


        parent.SetParent(
            doorsParent,
            true
        );


        Vector3 position =
            door.center;


        bool horizontal;


        if (wall != null)
        {
            horizontal =
                wall.horizontal;


            position =
                ProjectElementOntoWall(
                    door,
                    wall
                );
        }
        else
        {
            horizontal =
                door.width >= door.length;
        }


        parent.position =
            position;


        float doorWidth =
            GetOpeningWidth(
                door,
                horizontal
            );



        GameObject panel =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );


        panel.name =
            "DoorPanel";


        panel.transform.SetParent(
            parent,
            false
        );


        if (horizontal)
        {
            panel.transform.localScale =
                new Vector3(
                    doorWidth,
                    doorHeight,
                    doorThickness
                );
        }
        else
        {
            panel.transform.localScale =
                new Vector3(
                    doorThickness,
                    doorHeight,
                    doorWidth
                );
        }


        panel.transform.localPosition =
            new Vector3(
                0f,
                doorHeight / 2f,
                0f
            );


        ApplyMaterial(
            panel,
            doorMaterial,
            new Color(
                0.35f,
                0.20f,
                0.08f
            )
        );


      

        float frameHeight =
            Mathf.Max(
                doorHeight,
                2.1f
            );


        if (horizontal)
        {
            CreatePart(
                "Frame_Left",
                parent,
                new Vector3(
                    doorFrameThickness,
                    frameHeight,
                    doorFrameDepth
                ),
                new Vector3(
                    -doorWidth / 2f,
                    frameHeight / 2f,
                    0f
                ),
                doorFrameMaterial,
                new Color(
                    0.12f,
                    0.08f,
                    0.04f
                )
            );


            CreatePart(
                "Frame_Right",
                parent,
                new Vector3(
                    doorFrameThickness,
                    frameHeight,
                    doorFrameDepth
                ),
                new Vector3(
                    doorWidth / 2f,
                    frameHeight / 2f,
                    0f
                ),
                doorFrameMaterial,
                new Color(
                    0.12f,
                    0.08f,
                    0.04f
                )
            );


            CreatePart(
                "Frame_Top",
                parent,
                new Vector3(
                    doorWidth +
                    doorFrameThickness * 2f,

                    doorFrameThickness,

                    doorFrameDepth
                ),
                new Vector3(
                    0f,
                    frameHeight,
                    0f
                ),
                doorFrameMaterial,
                new Color(
                    0.12f,
                    0.08f,
                    0.04f
                )
            );
        }
        else
        {
            CreatePart(
                "Frame_Left",
                parent,
                new Vector3(
                    doorFrameDepth,
                    frameHeight,
                    doorFrameThickness
                ),
                new Vector3(
                    0f,
                    frameHeight / 2f,
                    -doorWidth / 2f
                ),
                doorFrameMaterial,
                new Color(
                    0.12f,
                    0.08f,
                    0.04f
                )
            );


            CreatePart(
                "Frame_Right",
                parent,
                new Vector3(
                    doorFrameDepth,
                    frameHeight,
                    doorFrameThickness
                ),
                new Vector3(
                    0f,
                    frameHeight / 2f,
                    doorWidth / 2f
                ),
                doorFrameMaterial,
                new Color(
                    0.12f,
                    0.08f,
                    0.04f
                )
            );


            CreatePart(
                "Frame_Top",
                parent,
                new Vector3(
                    doorFrameDepth,
                    doorFrameThickness,
                    doorWidth +
                    doorFrameThickness * 2f
                ),
                new Vector3(
                    0f,
                    frameHeight,
                    0f
                ),
                doorFrameMaterial,
                new Color(
                    0.12f,
                    0.08f,
                    0.04f
                )
            );
        }
    }


   

    private void CreateArchitecturalWindow(
        Detection window,
        WallInfo wall,
        int index
    )
    {
        Transform parent =
            new GameObject(
                "Window_" + index
            ).transform;


        parent.SetParent(
            windowsParent,
            true
        );


        bool horizontal;


        Vector3 position;


        if (wall != null)
        {
            horizontal =
                wall.horizontal;


           

            position =
                ProjectElementOntoWall(
                    window,
                    wall
                );


            float surfaceOffset =
                (
                    wall.thickness / 2f
                ) +
                windowSurfaceOffset;


            if (horizontal)
            {
                position.z +=
                    GetSurfaceDirection(
                        window.center.z,
                        wall.centerShort
                    ) *
                    surfaceOffset;
            }
            else
            {
                position.x +=
                    GetSurfaceDirection(
                        window.center.x,
                        wall.centerShort
                    ) *
                    surfaceOffset;
            }
        }
        else
        {
            horizontal =
                window.width >=
                window.length;

            position =
                window.center;
        }


        parent.position =
            position;


       
        float windowWidth =
            GetOpeningWidth(
                window,
                horizontal
            );




        GameObject glass =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );


        glass.name =
            "Glass";


        glass.transform.SetParent(
            parent,
            false
        );


        if (horizontal)
        {
            glass.transform.localScale =
                new Vector3(
                    windowWidth,
                    windowHeight,
                    windowFrameDepth * 0.35f
                );
        }
        else
        {
            glass.transform.localScale =
                new Vector3(
                    windowFrameDepth * 0.35f,
                    windowHeight,
                    windowWidth
                );
        }


        glass.transform.localPosition =
            new Vector3(
                0f,
                windowSillHeight,
                0f
            );


        ApplyMaterial(
            glass,
            windowGlassMaterial,
            new Color(
                0.45f,
                0.75f,
                1f,
                0.45f
            )
        );


        

        float frame =
            windowFrameThickness;


        if (horizontal)
        {
            
            CreatePart(
                "Frame_Left",
                parent,
                new Vector3(
                    frame,
                    windowHeight,
                    windowFrameDepth
                ),
                new Vector3(
                    -windowWidth / 2f,
                    windowSillHeight,
                    0f
                ),
                windowFrameMaterial,
                new Color(
                    0.12f,
                    0.12f,
                    0.12f
                )
            );


            CreatePart(
                "Frame_Right",
                parent,
                new Vector3(
                    frame,
                    windowHeight,
                    windowFrameDepth
                ),
                new Vector3(
                    windowWidth / 2f,
                    windowSillHeight,
                    0f
                ),
                windowFrameMaterial,
                new Color(
                    0.12f,
                    0.12f,
                    0.12f
                )
            );


           
            CreatePart(
                "Frame_Top",
                parent,
                new Vector3(
                    windowWidth +
                    frame * 2f,

                    frame,

                    windowFrameDepth
                ),
                new Vector3(
                    0f,
                    windowSillHeight +
                    windowHeight / 2f,

                    0f
                ),
                windowFrameMaterial,
                new Color(
                    0.12f,
                    0.12f,
                    0.12f
                )
            );


           
            CreatePart(
                "Frame_Bottom",
                parent,
                new Vector3(
                    windowWidth +
                    frame * 2f,

                    frame,

                    windowFrameDepth
                ),
                new Vector3(
                    0f,
                    windowSillHeight -
                    windowHeight / 2f,

                    0f
                ),
                windowFrameMaterial,
                new Color(
                    0.12f,
                    0.12f,
                    0.12f
                )
            );


            
            CreatePart(
                "Frame_Center",
                parent,
                new Vector3(
                    frame,
                    windowHeight,
                    windowFrameDepth
                ),
                new Vector3(
                    0f,
                    windowSillHeight,
                    0f
                ),
                windowFrameMaterial,
                new Color(
                    0.12f,
                    0.12f,
                    0.12f
                )
            );
        }
        else
        {
            
            CreatePart(
                "Frame_Left",
                parent,
                new Vector3(
                    windowFrameDepth,
                    windowHeight,
                    frame
                ),
                new Vector3(
                    0f,
                    windowSillHeight,
                    -windowWidth / 2f
                ),
                windowFrameMaterial,
                new Color(
                    0.12f,
                    0.12f,
                    0.12f
                )
            );


            CreatePart(
                "Frame_Right",
                parent,
                new Vector3(
                    windowFrameDepth,
                    windowHeight,
                    frame
                ),
                new Vector3(
                    0f,
                    windowSillHeight,
                    windowWidth / 2f
                ),
                windowFrameMaterial,
                new Color(
                    0.12f,
                    0.12f,
                    0.12f
                )
            );


          
            CreatePart(
                "Frame_Top",
                parent,
                new Vector3(
                    windowFrameDepth,
                    frame,
                    windowWidth +
                    frame * 2f
                ),
                new Vector3(
                    0f,
                    windowSillHeight +
                    windowHeight / 2f,

                    0f
                ),
                windowFrameMaterial,
                new Color(
                    0.12f,
                    0.12f,
                    0.12f
                )
            );


            
            CreatePart(
                "Frame_Bottom",
                parent,
                new Vector3(
                    windowFrameDepth,
                    frame,
                    windowWidth +
                    frame * 2f
                ),
                new Vector3(
                    0f,
                    windowSillHeight -
                    windowHeight / 2f,

                    0f
                ),
                windowFrameMaterial,
                new Color(
                    0.12f,
                    0.12f,
                    0.12f
                )
            );


          
            CreatePart(
                "Frame_Center",
                parent,
                new Vector3(
                    windowFrameDepth,
                    windowHeight,
                    frame
                ),
                new Vector3(
                    0f,
                    windowSillHeight,
                    0f
                ),
                windowFrameMaterial,
                new Color(
                    0.12f,
                    0.12f,
                    0.12f
                )
            );
        }
    }


   

    private WallInfo FindNearestWall(
        Detection element,
        List<Detection> walls
    )
    {
        WallInfo best =
            null;


        float bestDistance =
            float.MaxValue;


        foreach (Detection wall in walls)
        {
            bool horizontal =
                wall.width >= wall.length;


            float distance;


            if (horizontal)
            {
                distance =
                    Mathf.Abs(
                        element.center.z -
                        wall.center.z
                    );


                float halfLength =
                    wall.width / 2f;


                float elementLong =
                    element.width / 2f;


                bool overlaps =
                    Mathf.Abs(
                        element.center.x -
                        wall.center.x
                    )
                    <=
                    halfLength +
                    elementLong +
                    maximumWallAssociationDistance;


                if (!overlaps)
                    continue;
            }
            else
            {
                distance =
                    Mathf.Abs(
                        element.center.x -
                        wall.center.x
                    );


                float halfLength =
                    wall.length / 2f;


                float elementLong =
                    element.length / 2f;


                bool overlaps =
                    Mathf.Abs(
                        element.center.z -
                        wall.center.z
                    )
                    <=
                    halfLength +
                    elementLong +
                    maximumWallAssociationDistance;


                if (!overlaps)
                    continue;
            }


            if (
                distance <
                bestDistance
            )
            {
                bestDistance =
                    distance;


                best =
                    new WallInfo
                    {
                        detection = wall,

                        horizontal =
                            horizontal,

                        longLength =
                            horizontal
                                ? wall.width
                                : wall.length,

                        thickness =
                            Mathf.Max(
                                horizontal
                                    ? wall.length
                                    : wall.width,

                                minimumWallThickness
                            ),

                        centerLong =
                            horizontal
                                ? wall.center.x
                                : wall.center.z,

                        centerShort =
                            horizontal
                                ? wall.center.z
                                : wall.center.x
                    };
            }
        }


        return best;
    }


   

    private bool IsDetectionOnWall(
        Detection element,
        Detection wall,
        bool horizontal
    )
    {
        float distance;


        if (horizontal)
        {
            distance =
                Mathf.Abs(
                    element.center.z -
                    wall.center.z
                );


            float wallHalf =
                wall.width / 2f;


            float elementHalf =
                element.width / 2f;


            return
                distance <=
                maximumWallAssociationDistance
                &&

                Mathf.Abs(
                    element.center.x -
                    wall.center.x
                )
                <=
                wallHalf +
                elementHalf +
                maximumWallAssociationDistance;
        }


        distance =
            Mathf.Abs(
                element.center.x -
                wall.center.x
            );


        float wallHalfZ =
            wall.length / 2f;


        float elementHalfZ =
            element.length / 2f;


        return
            distance <=
            maximumWallAssociationDistance
            &&

            Mathf.Abs(
                element.center.z -
                wall.center.z
            )
            <=
            wallHalfZ +
            elementHalfZ +
            maximumWallAssociationDistance;
    }




    private Vector3 ProjectElementOntoWall(
        Detection element,
        WallInfo wall
    )
    {
        Vector3 result =
            element.center;


        if (wall.horizontal)
        {
            result.z =
                wall.centerShort;
        }
        else
        {
            result.x =
                wall.centerShort;
        }


        return result;
    }


   

    private float GetLongCoordinate(
        Detection element,
        Detection wall,
        bool horizontal
    )
    {
        if (horizontal)
        {
            return
                element.center.x -
                wall.center.x;
        }


        return
            element.center.z -
            wall.center.z;
    }



    private float GetOpeningWidth(
        Detection element,
        bool horizontal
    )
    {
        return Mathf.Max(
            horizontal
                ? element.width
                : element.length,

            0.05f
        );
    }



    private Vector3 GetWallSegmentCenter(
        Detection wall,
        bool horizontal,
        float longCoordinate
    )
    {
        if (horizontal)
        {
            return new Vector3(
                wall.center.x +
                longCoordinate,

                0f,

                wall.center.z
            );
        }


        return new Vector3(
            wall.center.x,

            0f,

            wall.center.z +
            longCoordinate
        );
    }


   

    private float GetSurfaceDirection(
        float elementCoordinate,
        float wallCoordinate
    )
    {
        if (
            elementCoordinate >=
            wallCoordinate
        )
        {
            return 1f;
        }


        return -1f;
    }




    private void CreateFloor(
        float minX,
        float maxX,
        float minZ,
        float maxZ
    )
    {
        float width =
            maxX -
            minX +
            floorMargin * 2f;


        float length =
            maxZ -
            minZ +
            floorMargin * 2f;


        float centerX =
            (minX + maxX) /
            2f;


        float centerZ =
            (minZ + maxZ) /
            2f;


        GameObject floor =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );


        floor.name =
            "Floor";


        floor.transform.SetParent(
            floorParent,
            true
        );


        floor.transform.position =
            new Vector3(
                centerX,
                -floorThickness / 2f,
                centerZ
            );


        floor.transform.localScale =
            new Vector3(
                Mathf.Max(
                    width,
                    0.1f
                ),

                floorThickness,

                Mathf.Max(
                    length,
                    0.1f
                )
            );


        ApplyMaterial(
            floor,
            floorMaterial,
            new Color(
                0.25f,
                0.25f,
                0.25f
            )
        );
    }




    private GameObject CreatePart(
        string objectName,
        Transform parent,
        Vector3 scale,
        Vector3 localPosition,
        Material material,
        Color fallbackColor
    )
    {
        GameObject part =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );


        part.name =
            objectName;


        part.transform.SetParent(
            parent,
            false
        );


        part.transform.localScale =
            scale;


        part.transform.localPosition =
            localPosition;


        ApplyMaterial(
            part,
            material,
            fallbackColor
        );


        return part;
    }




    private void ApplyMaterial(
        GameObject objectToModify,
        Material material,
        Color fallbackColor
    )
    {
        Renderer renderer =
            objectToModify.GetComponent<Renderer>();


        if (renderer == null)
            return;


        if (material != null)
        {
            renderer.material =
                material;

            return;
        }


        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit"
            );


        if (shader == null)
        {
            shader =
                Shader.Find(
                    "Standard"
                );
        }


        if (shader == null)
            return;


        Material generatedMaterial =
            new Material(shader);


        generatedMaterial.color =
            fallbackColor;


        renderer.material =
            generatedMaterial;
    }


    

    public Bounds GetGeneratedFloorPlanBounds()
    {
        if (root == null)
        {
            return new Bounds(
                transform.position,
                Vector3.zero
            );
        }


        Renderer[] renderers =
            root.GetComponentsInChildren<Renderer>();


        if (
            renderers == null ||
            renderers.Length == 0
        )
        {
            return new Bounds(
                root.position,
                Vector3.zero
            );
        }


        Bounds bounds =
            renderers[0].bounds;


        for (
            int i = 1;
            i < renderers.Length;
            i++
        )
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }


        return bounds;
    }


    

    [ContextMenu("Clear Generated Floor Plan")]
    public void ClearGenerated()
    {
        if (root == null)
        {
            root =
                transform.Find(
                    "GeneratedFloorPlan"
                );
        }


        if (root != null)
        {
            if (Application.isPlaying)
            {
                Destroy(
                    root.gameObject
                );
            }
            else
            {
                DestroyImmediate(
                    root.gameObject
                );
            }
        }


        root = null;

        floorParent = null;

        wallsParent = null;

        doorsParent = null;

        windowsParent = null;
    }


   

    private void EnsureHierarchy()
    {
        GameObject rootObject =
            new GameObject(
                "GeneratedFloorPlan"
            );


        rootObject.transform.SetParent(
            transform,
            false
        );


        root =
            rootObject.transform;


        floorParent =
            new GameObject(
                "Floor"
            ).transform;


        floorParent.SetParent(
            root,
            false
        );


        wallsParent =
            new GameObject(
                "Walls"
            ).transform;


        wallsParent.SetParent(
            root,
            false
        );


        doorsParent =
            new GameObject(
                "Doors"
            ).transform;


        doorsParent.SetParent(
            root,
            false
        );


        windowsParent =
            new GameObject(
                "Windows"
            ).transform;


        windowsParent.SetParent(
            root,
            false
        );
    }




    private Vector3 ConvertPosition(
        float x,
        float z,
        float widthMeters,
        float lengthMeters
    )
    {
        float unityX =
            x /
            pixelsPerMeter;


        float unityZ =
            -z /
            pixelsPerMeter;


        if (positionIsTopLeft)
        {
            unityX +=
                widthMeters / 2f;


            unityZ -=
                lengthMeters / 2f;
        }


        return new Vector3(
            unityX,
            0f,
            unityZ
        );
    }
}



[Serializable]
public class FloorPlanData
{
    public string filename;

    public int image_width;

    public int image_height;

    public int total_elements;

    public Element[] elements;
}


[Serializable]
public class Element
{
    public string type;

    public float confidence;

    public Position position;

    public Size size;
}


[Serializable]
public class Position
{
    public float x;

    public float z;
}


[Serializable]
public class Size
{
    public float width;

    public float length;
}



public struct FloorPlanGenerationResult
{
    public bool success;

    public string message;

    public int wallCount;

    public int doorCount;

    public int windowCount;


    public static FloorPlanGenerationResult Success(
        string message,
        int wallCount,
        int doorCount,
        int windowCount
    )
    {
        return new FloorPlanGenerationResult
        {
            success = true,

            message = message,

            wallCount = wallCount,

            doorCount = doorCount,

            windowCount = windowCount
        };
    }


    public static FloorPlanGenerationResult Failed(
        string message
    )
    {
        return new FloorPlanGenerationResult
        {
            success = false,

            message = message,

            wallCount = 0,

            doorCount = 0,

            windowCount = 0
        };
    }
}