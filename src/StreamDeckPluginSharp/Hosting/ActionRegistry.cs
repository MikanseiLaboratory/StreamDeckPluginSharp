using System.Reflection;

namespace StreamDeckPluginSharp.Hosting;

internal sealed class ActionRegistry
{
    private readonly Dictionary<string, Type> _actions = new(StringComparer.OrdinalIgnoreCase);

    public void Add(string uuid, Type actionType)
    {
        if (!typeof(Actions.ActionBase).IsAssignableFrom(actionType))
        {
            throw new ArgumentException($"{actionType.FullName} must inherit ActionBase.", nameof(actionType));
        }

        _actions[uuid] = actionType;
    }

    public void AddFromAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<StreamDeckActionAttribute>();
            if (attribute is null || type.IsAbstract)
            {
                continue;
            }

            Add(attribute.Uuid, type);
        }
    }

    public bool TryGet(string uuid, out Type actionType) => _actions.TryGetValue(uuid, out actionType!);

    public IReadOnlyDictionary<string, Type> Actions => _actions;
}
