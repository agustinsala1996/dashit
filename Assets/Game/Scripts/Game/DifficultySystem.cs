using UnityEngine;
using System.Collections.Generic; // Esto nos permitirá usar listas

/// <summary>
/// Gestiona la dificultad del juego ajustando la cadencia de los spawners
/// basándose en la puntuación recolectada. No modifica la lógica interna de los spawners.
/// </summary>
public class DifficultySystem : MonoBehaviour
{
    // Hacemos que sea un "Singleton". Esto significa que solo existirá una instancia
    // de este script en todo tu juego, y podemos acceder a ella fácilmente.
    public static DifficultySystem Instance { get; private set; }

    [Header("Referencias a Spawners")]
    [Tooltip("Arrastra aquí el GameObject que tiene el script PointsSpawner.")]
    public PointsSpawner pointsSpawner;
    [Tooltip("Arrastra aquí TODOS los GameObjects que tienen el script ObstacleSpawner (ej. el vertical y el horizontal).")]
    public List<ObstacleSpawner> obstacleSpawners; // ¡Ahora es una LISTA!
    [Tooltip("Arrastra aquí el GameObject que tiene el script EnemySpawner.")]
    public EnemySpawner enemySpawner;

    /// <summary>
    /// Estructura que define los parámetros de una etapa de dificultad.
    /// Se serializa para poder configurarla en el Inspector de Unity.
    /// </summary>
    [System.Serializable] // Permite que se vea y configure en el Inspector de Unity
    public class DifficultyStage
    {
        public string stageName; // Un nombre para esta etapa (ej: "Nivel 1")
        [Tooltip("La cantidad TOTAL de puntos que el jugador debe haber recolectado para llegar a esta etapa.")]
        public int scoreThreshold; // Cuántos puntos para pasar a este nivel

        [Header("Modificadores de Cadencia (más pequeño = más rápido)")]
        [Tooltip("Multiplicador para el retraso de aparición de los puntos. 1.0 = normal, 0.5 = doble de rápido.")]
        [Range(0.1f, 5.0f)] public float pointSpawnDelayMultiplier = 1.0f;
        [Tooltip("Multiplicador para el retraso de aparición de los obstáculos.")]
        [Range(0.1f, 5.0f)] public float obstacleSpawnDelayMultiplier = 1.0f;
        [Tooltip("Multiplicador para el retraso de aparición de los enemigos.")]
        [Range(0.1f, 5.0f)] public float enemySpawnDelayMultiplier = 1.0f;

        // IMPORTANTE: En este enfoque, no tocaremos "más enemigos" o "más obstáculos"
        // directamente desde este script, ya que eso implicaría modificar tus otros scripts de spawner.
        // Por ahora, solo la cadencia.
    }

    [Tooltip("Aquí defines todos los 'niveles' de dificultad. Asegúrate de que el 'Score Threshold' sea creciente y que el primer stage tenga Score Threshold 0.")]
    public List<DifficultyStage> difficultyStages; // Una lista donde pondremos nuestros niveles

    // Variables internas para el seguimiento del estado
    private int _currentStageIndex = 0; // El índice del nivel actual en la lista
    private int _lastKnownScore = 0;    // Guardaremos el último score para ver si ha cambiado

    // Guardaremos los valores originales de los delays de los spawners.
    // Esto es para que los multiplicadores siempre se basen en el valor original, no en el ya modificado.
    private float _originalPointSpawnDelay;
    private List<float> _originalObstacleSpawnDelays = new List<float>(); // ¡Ahora es una LISTA para guardar los delays de CADA spawner!
    private float _originalEnemySpawnDelay;

    // Este método se llama automáticamente cuando el script se activa por primera vez
    void Awake()
    {
        // Aseguramos que solo haya una instancia de este script
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Si ya existe otra instancia, destruimos esta
        }
    }

    // Este método se llama justo antes de que el primer frame del juego se actualice
    void Start()
    {
        // ***** CORRECCIÓN CLAVE PARA EL ArgumentOutOfRangeException *****
        // Aseguramos que haya al menos una etapa de dificultad definida.
        if (difficultyStages == null || difficultyStages.Count == 0)
        {
            Debug.LogError("¡ERROR: No hay etapas de dificultad configuradas en el Inspector de DifficultySystem! Por favor, añade al menos una.");
            enabled = false; // Desactiva este script para evitar más errores
            return;
        }

        // Guardamos los delays originales de tus spawners.
        // Asumimos que tus scripts PointsSpawner, ObstacleSpawner, EnemySpawner tienen una propiedad pública 'delayBetweenSpawn'.
        if (pointsSpawner != null) _originalPointSpawnDelay = pointsSpawner.delayBetweenSpawn;
        else Debug.LogWarning("PointsSpawner no asignado en DifficultySystem.");

        if (obstacleSpawners != null && obstacleSpawners.Count > 0)
        {
            _originalObstacleSpawnDelays.Clear(); // Limpiamos por si acaso
            foreach (var spawner in obstacleSpawners)
            {
                if (spawner != null)
                {
                    // Asegúrate de que tu ObstacleSpawner tenga 'public float delayBetweenSpawn'
                    _originalObstacleSpawnDelays.Add(spawner.delayBetweenSpawn);
                }
                else
                {
                    // Esto ocurre si hay un slot vacío en la lista de ObstacleSpawners en el Inspector
                    Debug.LogWarning("Un slot de ObstacleSpawner está vacío en la lista. Se usará un delay original por defecto de 1.0f.");
                    _originalObstacleSpawnDelays.Add(1.0f); // Valor por defecto si un slot está vacío
                }
            }
        }
        else Debug.LogWarning("No hay ObstacleSpawners asignados en DifficultySystem.");


        if (enemySpawner != null) _originalEnemySpawnDelay = enemySpawner.delayBetweenSpawn;
        else Debug.LogWarning("EnemySpawner no asignado en DifficultySystem.");

        // Inicializamos la dificultad aplicando los valores de la primera etapa (índice 0)
        ApplyCurrentStageDifficulty();
        // Guardamos la puntuación actual para empezar a monitorear desde ahí
        _lastKnownScore = GetCurrentScore();
    }

    // Este método se llama en cada frame del juego
    void Update()
    {
        int currentScore = GetCurrentScore(); // Obtenemos la puntuación actual del juego

        // Si la puntuación ha cambiado...
        if (currentScore != _lastKnownScore)
        {
            _lastKnownScore = currentScore; // Actualizamos el último score conocido
            CheckDifficultyProgression();   // Verificamos si debemos avanzar de nivel de dificultad
        }
    }

    // Método para obtener la puntuación actual del juego
    private int GetCurrentScore()
    {
        // Asume que tienes un script llamado 'Score' que es un Singleton (Score.instance)
        // y que tiene una variable pública o propiedad llamada 'currentScore'.
        if (Score.instance != null)
        {
            return Score.instance.currentScore;
        }
        return 0; // Si no hay score, devuelve 0
    }

    // Método para verificar si hemos alcanzado el umbral para el siguiente nivel de dificultad
    private void CheckDifficultyProgression()
    {
        // Si no estamos en la última etapa de dificultad...
        if (_currentStageIndex + 1 < difficultyStages.Count)
        {
            DifficultyStage nextStage = difficultyStages[_currentStageIndex + 1]; // Obtenemos la siguiente etapa

            // Si la puntuación actual es mayor o igual al umbral de la siguiente etapa...
            if (_lastKnownScore >= nextStage.scoreThreshold)
            {
                _currentStageIndex++; // Avanzamos al siguiente nivel de dificultad
                Debug.Log($"¡Avanzando a la etapa de dificultad: {difficultyStages[_currentStageIndex].stageName}! Puntuación actual: {_lastKnownScore}");
                ApplyCurrentStageDifficulty(); // Aplicamos los nuevos ajustes de dificultad
            }
        }
    }

    // Método para aplicar los ajustes de dificultad del nivel actual a los spawners
    private void ApplyCurrentStageDifficulty()
    {
        // ***** CORRECCIÓN ADICIONAL PARA EL ArgumentOutOfRangeException *****
        // Aseguramos que haya etapas y que el índice esté dentro de los límites.
        if (difficultyStages == null || difficultyStages.Count == 0)
        {
            Debug.LogError("No se pueden aplicar los ajustes de dificultad porque no hay etapas configuradas.");
            return;
        }
        if (_currentStageIndex >= difficultyStages.Count)
        {
            _currentStageIndex = difficultyStages.Count - 1; // Nos quedamos en la última etapa si nos excedimos
            Debug.LogWarning("Se alcanzó la última etapa de dificultad. Manteniendo la configuración máxima.");
        }

        DifficultyStage currentStage = difficultyStages[_currentStageIndex]; // Obtenemos la etapa actual

        // Ajustamos los delays de los spawners multiplicando su delay original por el factor de la etapa actual
        if (pointsSpawner != null)
        {
            pointsSpawner.delayBetweenSpawn = _originalPointSpawnDelay * currentStage.pointSpawnDelayMultiplier;
            Debug.Log($"PointsSpawner delay ajustado a: {pointsSpawner.delayBetweenSpawn} (Original: {_originalPointSpawnDelay}, Multiplicador: {currentStage.pointSpawnDelayMultiplier})");
        }

        if (obstacleSpawners != null && obstacleSpawners.Count > 0)
        {
            for (int i = 0; i < obstacleSpawners.Count; i++)
            {
                var spawner = obstacleSpawners[i];
                if (spawner != null)
                {
                    // Usa el delay original guardado para ESTE spawner
                    float originalDelay = (i < _originalObstacleSpawnDelays.Count) ? _originalObstacleSpawnDelays[i] : 1.0f; // Seguridad si falta un original

                    spawner.delayBetweenSpawn = originalDelay * currentStage.obstacleSpawnDelayMultiplier;
                    Debug.Log($"ObstacleSpawner [{i}] delay ajustado a: {spawner.delayBetweenSpawn} (Original: {originalDelay}, Multiplicador: {currentStage.obstacleSpawnDelayMultiplier})");
                }
                else
                {
                    Debug.LogWarning($"El ObstacleSpawner en el índice {i} de la lista está vacío. No se puede ajustar su delay.");
                }
            }
        }

        if (enemySpawner != null)
        {
            enemySpawner.delayBetweenSpawn = _originalEnemySpawnDelay * currentStage.enemySpawnDelayMultiplier;
            Debug.Log($"EnemySpawner delay ajustado a: {enemySpawner.delayBetweenSpawn} (Original: {_originalEnemySpawnDelay}, Multiplicador: {currentStage.enemySpawnDelayMultiplier})");
        }
    }
}