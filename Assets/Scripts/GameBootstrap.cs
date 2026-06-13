using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _enemyPrefab;

    private World world;
    private ViewEntityManager viewManager;

    void Start()
    {
        var eventBus = new EventBus();
        world = new World(eventBus);
        
        world.RegisterSystem(new InputSystem(), 1);
        world.RegisterSystem(new StateSystem(), 2);
        world.RegisterSystem(new MovementSystem(), 3);

        viewManager = new ViewEntityManager(world, _playerPrefab);
        EntityFactory.CreatePlayer(world, Vector3.zero);
    }

    void Update()
    {
        world.Tick(Time.deltaTime);
        viewManager.UpdateAll(Time.deltaTime);
    }

    void OnDestroy()
    {
        viewManager.Shutdown();
        world.Destroy();
    }
}
