namespace starry;

/// <summary>
/// its a component :)
/// </summary>
public interface IComponent {
    public void create(IEntity entity) {}
    public void update(IEntity entity, double delta) {}
    public void draw(IEntity entity) {}
}