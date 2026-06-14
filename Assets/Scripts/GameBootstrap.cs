using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private EntityConfig _playerConfig;

    private World world;
    private ViewEntityManager viewManager;

    void Start()
    {
        var eventBus = new EventBus();
        world = new World(eventBus);
        
        world.RegisterSystem(new InputSystem(), 0);
        world.RegisterSystem(new ComboSystem(), 10);
        world.RegisterSystem(new DodgeSystem(), 20);
        world.RegisterSystem(new StateSystem(), 0);
        world.RegisterSystem(new MovementSystem(), 10);

        viewManager = new ViewEntityManager(world, _playerConfig);
        EntityFactory.CreatePlayer(world, Vector3.zero, _playerConfig);
    }

    void Update()
    {
        world.Tick(Time.deltaTime);
        viewManager.UpdateAll(Time.deltaTime);
    }

    void OnDestroy()
    {
        viewManager.Destroy();
        world.Destroy();
    }
}
