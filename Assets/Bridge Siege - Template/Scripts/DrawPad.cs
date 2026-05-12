using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class DrawPad : MonoBehaviour
    {
        // ---------- Singleton Pattern ----------
        
        [Tooltip("Singleton instance of DrawPad")]
        public static DrawPad instance; // Singleton instance of DrawPad
        
        // Constructor to initialize the singleton instance
        DrawPad() => instance = this;

        // ---------- Drawing Components ----------
        
        [Header("Drawing Components")]
        [Tooltip("Layer mask for the draw pad")]
        public LayerMask drawPadLayer; // Layer mask to specify drawing area

        [Tooltip("LineRenderer for drawing on the draw pad")]
        public LineRenderer drawPadLR; // LineRenderer for drawing lines on the draw pad

        [Tooltip("LineRenderer for drawing on the floor")]
        public LineRenderer floorLR; // LineRenderer for drawing on the floor

        [Tooltip("Color of the line being drawn")]
        [SerializeField] private Color lineCol = Color.black; // Color of the drawing line

        [SerializeField] [Tooltip("Indicates if drawing is in progress")]
        private bool isDrawing = false; // Boolean to check if the player is currently drawing

        [Space(10)]
        // ---------- Soldier Detection and Spawn Settings ----------
        
        [Header("Soldier Detection and Spawn Settings")]
        [Tooltip("Layer mask for detecting soldiers")]
        public LayerMask whatIsSoldier; // Layer mask for detecting soldiers

        [Tooltip("Radius around the draw pad for action detection")]
        public float radius = 0.75f; // Radius to check around for spawning soldiers

        [Tooltip("GameObject used for spawning soldiers")]
        public GameObject spawnSoldier; // Prefab for spawning soldier units

        [Tooltip("List of soldiers in the game")]
        public List<GameObject> soldiers; // List to track all spawned soldier objects

        [Space(10)]
        // ---------- Drawing Constraints ----------
        
        [Header("Drawing Constraints")]
        [Tooltip("Points accumulated while drawing")]
        private int points = 0; // Number of points collected while drawing

        [Tooltip("Total distance that can be drawn per wave")]
        public float totalDistanceDrawnPerWave = 25f; // Maximum distance allowed for drawing in a wave
        private float distanceDrawnPerWave; // Distance drawn in the current wave so far

        [Space(10)]
        // ---------- Time and Touch Controls ----------
        
        [Header("Time and Touch Controls")]
        [Range(0f, 3f)]
        [Tooltip("Initial minimum time before starting an action")]
        public float startMinTime = 0.5f; // Initial minimum time for spawning soldiers

        private float minTime = 0.5f; // Updated time variable for each action

        [Range(0f, 3f)]
        [Tooltip("Maximum time the mouse can be held for an action")]
        public float maxHoldTime = 1.5f; // Maximum allowed hold time for drawing

        private float timeHoldingMouse; // Tracks how long the mouse is held down

        [Space(10)]
        // ---------- Touch UI Elements ----------
        
        [Header("Touch UI Elements")]
        [Tooltip("UI slider for touch feedback")]
        public UnityEngine.UI.Image touchSlider; // UI slider for touch feedback

        [Tooltip("Object that holds the touch slider UI")]
        public GameObject touchSliderObj; // Object that holds the touch slider UI

        // ---------- Reference to GameManager ----------
        private GameManager gameManager; // Reference to GameManager
        
        // ---------- Initialization ----------
        
        void Start()
        {
            minTime = startMinTime; // Initialize minimum time for actions
            lineCol = Color.black; // Set default line color

            // Access GameManager's touch slider UI elements
            touchSlider = GameManager.instance.touchSlider;
            touchSliderObj = GameManager.instance.touchSliderObj;

            distanceDrawnPerWave = totalDistanceDrawnPerWave; // Initialize drawing distance
            gameManager = GameManager.instance; // Reference to the GameManager instance
        }

        // ---------- Soldier Management ----------

        public void DestroySoldiers()
        {
            distanceDrawnPerWave = totalDistanceDrawnPerWave; // Reset drawing distance

            // Destroy all soldiers in the soldiers list
            foreach (GameObject G in soldiers)
            {
                if(G != null)
                    Destroy(G);
            }
        }

        // ---------- Update for Drawing Mechanism ----------

        private void Update()
        {
            // Start drawing when the left mouse button is pressed
            if (Input.GetMouseButtonDown(0))
            {
                if(!gameManager.shopSystem.shopWindow.activeInHierarchy || !gameManager.pauseWindow.activeInHierarchy)
                    touchSliderObj.SetActive(true); // Show touch slider UI

                timeHoldingMouse = maxHoldTime; // Reset hold time
                points = 0; // Reset points counter
                isDrawing = true; // Set drawing to true
                lineCol = Color.black; // Set line color to black

                // Set line renderer colors for the draw pad and floor
                drawPadLR.startColor = lineCol;
                drawPadLR.endColor = lineCol;
            }

            // While holding the mouse button, continue drawing
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Create a ray at the mouse position

                // If ray hits the draw pad layer and drawing is active
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, drawPadLayer) && isDrawing)
                {
                    // Stop drawing if max distance is reached
                    if(distanceDrawnPerWave < 0f)
                    {
                        mouseUp();
                        return;
                    }

                    // Update drawing if time is up
                    if (minTime <= 0f)
                    {
                        points++; // Increment points

                        // Set new position for the line renderer on draw pad and floor
                        drawPadLR.positionCount = points;
                        drawPadLR.SetPosition(points - 1, hit.point);
                        floorLR.positionCount = points;
                        floorLR.SetPosition(points - 1, drawPadLR.transform.InverseTransformPoint(hit.point));
                        Vector3 Pos = floorLR.transform.TransformPoint(floorLR.GetPosition(points - 1));

                        // Check if there’s no soldier at the point to avoid overlap
                        if (!Physics.CheckSphere(Pos, radius, whatIsSoldier))
                        {
                            soldiers.Add(Instantiate(spawnSoldier, Pos, Quaternion.identity)); // Spawn soldier at position
                            minTime = startMinTime; // Reset min time
                        }

                        // Deduct drawing distance as line progresses
                        if(points > 0)
                            distanceDrawnPerWave -= Vector2.Distance(floorLR.GetPosition(points - 1), floorLR.GetPosition(points - 2));
                    }
                    else
                    {
                        minTime -= Time.deltaTime; // Countdown for min time between actions
                    }

                    timeHoldingMouse -= Time.deltaTime; // Update hold time countdown
                    touchSlider.fillAmount = distanceDrawnPerWave / totalDistanceDrawnPerWave; // Update slider based on remaining distance
                }
            }
            else if (Input.GetMouseButtonUp(0)) // Stop drawing when mouse is released
            {
                mouseUp();
            }

            // Smoothly transition the line color back to initial color when not drawing
            drawPadLR.startColor = Color.Lerp(drawPadLR.startColor, lineCol, Time.deltaTime * 2.5f);
            drawPadLR.endColor = Color.Lerp(drawPadLR.startColor, lineCol, Time.deltaTime * 2.5f);
        }

        // Called to handle end of drawing action
        void mouseUp()
        {
            touchSliderObj.SetActive(false); // Hide touch slider UI
            lineCol = new Vector4(Color.black.r, Color.black.g, Color.black.b, 0f); // Set line color to transparent
            isDrawing = false; // Stop drawing
        }
    }
}
